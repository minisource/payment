using Application.DTOs;
using Application.Options;
using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Minisource.Common.Domain;
using Minisource.Common.Exceptions;
using Minisource.Common.Locking;
using Parbad;
using Polly;
using Polly.Retry;
using System.Net.Http.Json;
using System.Text.Json;

namespace Application.Services;

/// <summary>
/// Payment service implementing payment processing with distributed locking and transactions.
/// </summary>
public class PaymentService : IPaymentService
{
    private readonly IOnlinePayment _onlinePayment;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IWalletService _walletService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDistributedLockService _lockService;
    private readonly IOptions<PaymentOptions> _options;
    private readonly ILogger<PaymentService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IHttpClientFactory _httpClientFactory;

    public PaymentService(
        IOnlinePayment onlinePayment,
        IPaymentRepository paymentRepository,
        IWalletService walletService,
        IUnitOfWork unitOfWork,
        IDistributedLockService lockService,
        IOptions<PaymentOptions> options,
        ILogger<PaymentService> logger,
        IHttpContextAccessor httpContextAccessor,
        IHttpClientFactory httpClientFactory)
    {
        _onlinePayment = onlinePayment;
        _paymentRepository = paymentRepository;
        _walletService = walletService;
        _unitOfWork = unitOfWork;
        _lockService = lockService;
        _options = options;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _httpClientFactory = httpClientFactory;
    }

    /// <inheritdoc/>
    public async Task<PayResponse> InitiatePaymentAsync(PayRequest request, CancellationToken cancellationToken = default)
    {
        // Validate request
        if (request.Amount <= 0)
        {
            throw new ValidationException("Amount", "Amount must be greater than zero");
        }

        var gateway = request.Gateway ?? _options.Value.DefaultGateway;
        if (string.IsNullOrEmpty(gateway))
        {
            throw new ValidationException("Gateway", "No payment gateway specified");
        }

        // Check idempotency
        if (!string.IsNullOrWhiteSpace(request.IdempotencyKey))
        {
            var existing = await _paymentRepository.GetByIdempotencyKeyAsync(request.IdempotencyKey, cancellationToken);
            if (existing != null)
            {
                _logger.LogDebug("Returning existing payment for idempotency key: {Key}", request.IdempotencyKey);
                return CreatePayResponse(existing);
            }
        }

        var userId = request.UserId ?? GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            throw new ValidationException("UserId", "User ID is required");
        }

        // Generate idempotency key if not provided
        var idempotencyKey = request.IdempotencyKey ?? Guid.NewGuid().ToString("N");

        // Acquire lock to prevent duplicate payment creation
        await using var paymentLock = await _lockService.AcquireAsync(
            $"payment:create:{idempotencyKey}",
            TimeSpan.FromSeconds(30),
            cancellationToken);

        if (!paymentLock.IsAcquired)
        {
            throw new ConflictException("Unable to acquire lock for payment creation");
        }

        // Double-check idempotency after acquiring lock
        if (!string.IsNullOrWhiteSpace(request.IdempotencyKey))
        {
            var existing = await _paymentRepository.GetByIdempotencyKeyAsync(request.IdempotencyKey, cancellationToken);
            if (existing != null)
            {
                return CreatePayResponse(existing);
            }
        }

        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            decimal creditApplied = 0;
            Guid? walletId = null;
            var amountDue = request.Amount;

            // Apply wallet credit if requested
            if (request.UseWallet && !string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogDebug("Applying wallet credit for user {UserId}", userId);
                var creditResult = await _walletService.ApplyCreditAsync(userId, amountDue, "Payment deduction", cancellationToken);
                creditApplied = creditResult.Applied;
                amountDue = creditResult.Remaining;

                // Get wallet ID for tracking
                if (creditApplied > 0)
                {
                    var wallet = await _walletService.GetOrCreateAsync(userId, cancellationToken);
                    walletId = wallet.Id;
                }
            }

            // Generate tracking number
            var trackingNumber = await _paymentRepository.GenerateTrackingNumberAsync(cancellationToken);

            // Create payment using domain factory method
            var payment = Payment.Create(
                trackingNumber: trackingNumber,
                amount: request.Amount,
                currency: request.Currency,
                gateway: gateway,
                callbackUrl: request.CallbackUrl,
                returnUrl: request.ReturnUrl,
                userId: userId,
                metadata: request.Metadata,
                idempotencyKey: idempotencyKey);

            // Apply credit if any was used
            if (creditApplied > 0 && walletId.HasValue)
            {
                payment.ApplyCredit(creditApplied, walletId.Value);
            }

            // If payment fully covered by credit
            if (amountDue <= 0)
            {
                // Mark as processing then complete (to satisfy state machine)
                payment.StartProcessing();
                payment.Complete("WALLET_CREDIT");
                await _paymentRepository.AddAsync(payment, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation("Payment {PaymentId} completed via wallet credit", payment.Id);
                return CreatePayResponse(payment);
            }

            // Start payment processing with gateway
            payment.StartProcessing();

            // Create retry policy for gateway calls
            var retryPolicy = new ResiliencePipelineBuilder()
                .AddRetry(new RetryStrategyOptions
                {
                    MaxRetryAttempts = _options.Value.Polly.RetryCount,
                    Delay = TimeSpan.FromMilliseconds(500),
                    BackoffType = DelayBackoffType.Exponential
                })
                .AddTimeout(TimeSpan.FromSeconds(_options.Value.Polly.TimeoutSeconds))
                .Build();

            var result = await retryPolicy.ExecuteAsync(async ct =>
            {
                return await _onlinePayment.RequestAsync(invoice =>
                {
                    invoice
                        .SetAmount(amountDue)
                        .SetCallbackUrl(request.CallbackUrl)
                        .SetGateway(gateway)
                        .UseAutoIncrementTrackingNumber();
                });
            }, cancellationToken);

            if (!result.IsSucceed)
            {
                payment.Fail(result.Message);
                await _paymentRepository.AddAsync(payment, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                _logger.LogWarning("Payment initiation failed: {Message}", result.Message);
                throw new BusinessException("PAYMENT_INITIATION_FAILED", $"Payment initiation failed: {result.Message}");
            }

            // Update payment with tracking number
            payment.AddLog("GatewayResponse", $"TrackingNumber: {result.TrackingNumber}");

            await _paymentRepository.AddAsync(payment, cancellationToken);
            await _paymentRepository.UpdateTrackingNumberAsync(payment.Id, result.TrackingNumber, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation("Payment {PaymentId} initiated with tracking number {TrackingNumber}",
                payment.Id, result.TrackingNumber);

            return new PayResponse
            {
                PaymentId = payment.Id,
                TrackingNumber = result.TrackingNumber,
                GatewayUrl = result.GatewayTransporter.Descriptor.Url,
                IsSuccessful = true
            };
        }
        catch (Exception ex) when (ex is not AppException)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Failed to initiate payment");
            throw new BusinessException("PAYMENT_ERROR", "An error occurred while processing payment", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<VerifyResponse> VerifyPaymentAsync(CancellationToken cancellationToken = default)
    {
        var retryPolicy = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = _options.Value.Polly.RetryCount,
                Delay = TimeSpan.FromMilliseconds(500),
                BackoffType = DelayBackoffType.Exponential
            })
            .AddTimeout(TimeSpan.FromSeconds(_options.Value.Polly.TimeoutSeconds))
            .Build();

        var fetchResult = await retryPolicy.ExecuteAsync(async ct =>
        {
            return await _onlinePayment.FetchAsync();
        }, cancellationToken);

        var trackingNumber = fetchResult.TrackingNumber;

        // Acquire lock for payment verification
        await using var verifyLock = await _lockService.AcquireAsync(
            $"payment:verify:{trackingNumber}",
            TimeSpan.FromSeconds(30),
            cancellationToken);

        if (!verifyLock.IsAcquired)
        {
            throw new ConflictException("Unable to acquire lock for payment verification");
        }

        var payment = await _paymentRepository.GetByTrackingNumberAsync(trackingNumber, cancellationToken);
        if (payment == null)
        {
            throw new NotFoundException("Payment", trackingNumber.ToString());
        }

        // Check if already processed
        if (payment.Status == PaymentStatus.Completed)
        {
            return new VerifyResponse
            {
                Status = payment.Status,
                Message = "Payment already verified",
                IsSuccessful = true,
                TrackingNumber = payment.TrackingNumber,
                Amount = payment.Amount,
                Gateway = payment.Gateway.ToString()
            };
        }

        if (fetchResult.Status == PaymentFetchResultStatus.AlreadyProcessed)
        {
            return new VerifyResponse
            {
                Status = payment.Status,
                Message = "Payment already processed",
                IsSuccessful = payment.Status == PaymentStatus.Completed,
                TrackingNumber = payment.TrackingNumber,
                Amount = payment.Amount,
                Gateway = payment.Gateway.ToString()
            };
        }

        await using var dbTransaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var verifyResult = await retryPolicy.ExecuteAsync(async ct =>
            {
                return await _onlinePayment.VerifyAsync(fetchResult);
            }, cancellationToken);

            var providerResponse = JsonSerializer.Serialize(new
            {
                fetchResult.Status,
                fetchResult.TrackingNumber,
                fetchResult.Amount,
                fetchResult.GatewayName
            });

            payment.AddAttempt(
                verifyResult.IsSucceed ? PaymentStatus.Completed : PaymentStatus.Failed,
                providerResponse);

            if (verifyResult.IsSucceed)
            {
                payment.Complete(verifyResult.TransactionCode ?? string.Empty);
            }
            else
            {
                payment.Fail(verifyResult.Message);
            }

            await _paymentRepository.UpdateAsync(payment, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await dbTransaction.CommitAsync(cancellationToken);

            // Notify external systems
            await NotifyShopAsync(payment);

            return new VerifyResponse
            {
                Status = payment.Status,
                Message = verifyResult.Message,
                IsSuccessful = verifyResult.IsSucceed,
                TransactionCode = verifyResult.TransactionCode,
                TrackingNumber = payment.TrackingNumber,
                Amount = payment.Amount,
                Gateway = payment.Gateway.ToString()
            };
        }
        catch (Exception ex) when (ex is not AppException)
        {
            await dbTransaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Failed to verify payment {TrackingNumber}", trackingNumber);
            throw new BusinessException("VERIFICATION_ERROR", "An error occurred while verifying payment", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<PaymentDto> GetPaymentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var payment = await _paymentRepository.GetByIdAsync(id, cancellationToken);
        if (payment == null)
        {
            throw new NotFoundException("Payment", id);
        }

        // Check access
        var currentUserId = GetCurrentUserId();
        if (!string.IsNullOrEmpty(currentUserId) && payment.UserId != currentUserId && !IsAdmin())
        {
            throw new ForbiddenException("You don't have access to this payment");
        }

        return MapToDto(payment);
    }

    /// <inheritdoc/>
    public async Task<PagedResult<PaymentDto>> GetPaymentsAsync(
        string? status,
        DateTime? from,
        DateTime? to,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var items = await _paymentRepository.GetPaymentsAsync(userId, status, from, to, page, pageSize, cancellationToken);
        var totalCount = await _paymentRepository.GetTotalCountAsync(userId, status, from, to, cancellationToken);

        return new PagedResult<PaymentDto>
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    /// <inheritdoc/>
    public async Task<List<PaymentLogDto>> GetPaymentLogsAsync(Guid paymentId, CancellationToken cancellationToken = default)
    {
        var payment = await _paymentRepository.GetByIdWithLogsAsync(paymentId, cancellationToken);
        if (payment == null)
        {
            throw new NotFoundException("Payment", paymentId);
        }

        return payment.Logs.Select(l => new PaymentLogDto
        {
            Id = l.Id,
            PaymentId = l.PaymentId,
            Action = l.Action,
            Details = l.Details ?? string.Empty,
            Timestamp = l.CreatedAt
        }).ToList();
    }

    private async Task NotifyShopAsync(Payment payment)
    {
        if (string.IsNullOrWhiteSpace(_options.Value.ShopCallbackUrl))
        {
            return;
        }

        try
        {
            var client = _httpClientFactory.CreateClient();
            var payload = new
            {
                payment.Id,
                payment.TrackingNumber,
                Status = payment.Status.ToString(),
                payment.Amount,
                payment.AmountDue,
                payment.CreditApplied,
                payment.Metadata,
                payment.UserId
            };

            await client.PostAsJsonAsync(_options.Value.ShopCallbackUrl, payload);
            _logger.LogDebug("Shop notification sent for payment {PaymentId}", payment.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to notify shop for payment {PaymentId}", payment.Id);
        }
    }

    private string? GetCurrentUserId()
    {
        return _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value;
    }

    private bool IsAdmin()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null) return false;
        
        // Check for admin permissions (PAYMENT_MANAGE or WALLET_MANAGE)
        return user.HasClaim("permission", "PAYMENT_MANAGE") ||
               user.HasClaim("permission", "WALLET_MANAGE") ||
               user.HasClaim("role", "admin");
    }

    private static PayResponse CreatePayResponse(Payment payment)
    {
        return new PayResponse
        {
            PaymentId = payment.Id,
            TrackingNumber = payment.TrackingNumber,
            GatewayUrl = string.Empty,
            IsSuccessful = payment.Status == PaymentStatus.Completed ||
                           payment.Status == PaymentStatus.Processing ||
                           payment.Status == PaymentStatus.Pending
        };
    }

    private static PaymentDto MapToDto(Payment payment)
    {
        return new PaymentDto
        {
            Id = payment.Id,
            TrackingNumber = payment.TrackingNumber,
            Amount = payment.Amount,
            Currency = payment.Currency,
            Status = payment.Status,
            Gateway = payment.Gateway,
            CallbackUrl = payment.CallbackUrl,
            ReturnUrl = payment.ReturnUrl,
            Metadata = payment.Metadata,
            UserId = payment.UserId,
            CreditApplied = payment.CreditApplied,
            AmountDue = payment.AmountDue,
            CreatedAt = payment.CreatedAt,
            UpdatedAt = payment.UpdatedAt
        };
    }
}
