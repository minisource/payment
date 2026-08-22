using Application.DTOs;
using Domain.Entities;
using Domain.Enums;
using Domain.Events;
using Domain.Repositories;
using Microsoft.Extensions.Logging;
using Minisource.Common.Domain;
using Minisource.Common.Exceptions;

namespace Application.Features.PaymentIntents;

public class PaymentIntentService : IPaymentIntentService
{
    private readonly IPaymentIntentRepository _intentRepo;
    private readonly IPaymentTransactionRepository _txnRepo;
    private readonly IIdempotencyRecordRepository _idemRepo;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<PaymentIntentService> _logger;

    public PaymentIntentService(
        IPaymentIntentRepository intentRepo,
        IPaymentTransactionRepository txnRepo,
        IIdempotencyRecordRepository idemRepo,
        IUnitOfWork uow,
        ILogger<PaymentIntentService> logger)
    {
        _intentRepo = intentRepo;
        _txnRepo = txnRepo;
        _idemRepo = idemRepo;
        _uow = uow;
        _logger = logger;
    }

    public async Task<PaymentIntentDto> CreateAsync(Guid tenantId, Guid actorUserId, CreatePaymentIntentRequest request,
        string? applicationCode, string? idempotencyKey, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
            throw new ValidationException("IdempotencyKey", "X-Idempotency-Key header is required");

        if (request.Amount <= 0)
            throw new ValidationException("Amount", "Amount must be positive");

        // Idempotency check
        var existing = await _intentRepo.GetByIdempotencyKeyAsync(tenantId, idempotencyKey, ct);
        if (existing != null)
            return MapIntentToDto(existing);

        var existingIdem = await _idemRepo.GetByKeyAsync(tenantId, idempotencyKey, ct);
        if (existingIdem is { Status: "completed" })
            throw new ConflictException("Idempotency key already used with different request");

        // Parse purpose and wallet behavior
        var purpose = Enum.TryParse<PaymentIntentPurpose>(request.Purpose, true, out var p)
            ? p : PaymentIntentPurpose.WalletTopup;
        var walletBehavior = Enum.TryParse<WalletBehavior>(request.WalletBehavior, true, out var wb)
            ? wb : WalletBehavior.None;

        var intent = PaymentIntent.Create(
            tenantId, request.Amount, request.Currency,
            purpose, walletBehavior,
            applicationCode: applicationCode,
            payerUserId: request.PayerUserId,
            payerWalletId: request.PayerWalletId,
            recipientWalletId: request.RecipientWalletId,
            externalReferenceType: request.ExternalReferenceType,
            externalReferenceId: request.ExternalReferenceId,
            description: request.Description,
            returnUrl: request.ReturnUrl,
            expiresAt: request.ExpiresAt,
            idempotencyKey: idempotencyKey,
            metadata: request.Metadata,
            createdByUserId: actorUserId);

        await _intentRepo.AddAsync(intent, ct);

        // Pre-insert idempotency record
        await _idemRepo.AddAsync(new IdempotencyRecord
        {
            TenantId = tenantId, IdempotencyKey = idempotencyKey!,
            Operation = "payment_intent_create", Status = "completed",
            ResourceType = "PaymentIntent", ResourceId = intent.Id.ToString(),
            CreatedByUserId = actorUserId, CompletedAt = DateTime.UtcNow
        }, ct);

        await _uow.SaveChangesAsync(ct);
        _logger.LogInformation("Payment intent {IntentId} created for tenant {TenantId}", intent.Id, tenantId);

        return MapIntentToDto(intent);
    }

    public async Task<PaymentIntentDto> GetAsync(Guid paymentIntentId, CancellationToken ct)
    {
        var intent = await _intentRepo.GetByIdWithTransactionsAsync(paymentIntentId, ct)
            ?? throw new NotFoundException("PaymentIntent", paymentIntentId);
        return MapIntentToDto(intent);
    }

    public async Task<List<PaymentIntentDto>> GetUserIntentsAsync(Guid tenantId, Guid? userId, int skip, int take, CancellationToken ct)
    {
        var intents = await _intentRepo.GetByTenantAsync(tenantId, skip, take, payerUserId: userId, ct: ct);
        return intents.Select(MapIntentToDto).ToList();
    }

    public async Task<StartPaymentIntentResponse> StartAsync(Guid tenantId, Guid paymentIntentId, StartPaymentIntentRequest request, CancellationToken ct)
    {
        var intent = await _intentRepo.GetByIdAsync(paymentIntentId, ct)
            ?? throw new NotFoundException("PaymentIntent", paymentIntentId);

        if (intent.TenantId != tenantId)
            throw new ForbiddenException("Payment intent does not belong to this tenant");

        if (!intent.CanStart)
            throw new BusinessException($"Cannot start payment intent in {intent.Status} status", "payment_intent_invalid_state");

        // NOTE: Payment start is orchestrated through GatewayPaymentService.StartPaymentAsync.
        // This method exists for API parity but delegates to the gateway service.
        throw new BusinessException(
            "Use IGatewayPaymentService.StartPaymentAsync to start a payment with gateway selection",
            "payment_intent_use_gateway_service");
    }

    public async Task<PaymentIntentDto> CancelAsync(Guid tenantId, Guid paymentIntentId, string reason, CancellationToken ct)
    {
        var intent = await _intentRepo.GetByIdAsync(paymentIntentId, ct)
            ?? throw new NotFoundException("PaymentIntent", paymentIntentId);

        if (intent.TenantId != tenantId)
            throw new ForbiddenException("Payment intent does not belong to this tenant");

        intent.Cancel(reason);
        await _intentRepo.UpdateAsync(intent, ct);
        await _uow.SaveChangesAsync(ct);

        return MapIntentToDto(intent);
    }

    public async Task<List<PaymentTransactionDto>> GetTransactionsAsync(Guid paymentIntentId, CancellationToken ct)
    {
        var txns = await _txnRepo.GetByIntentIdAsync(paymentIntentId, ct);
        return txns.Select(MapTxnToDto).ToList();
    }

    public static PaymentIntentDto MapIntentToDto(PaymentIntent intent) => new()
    {
        Id = intent.Id, TenantId = intent.TenantId, ApplicationCode = intent.ApplicationCode,
        Amount = intent.Amount, Currency = intent.Currency, GrossAmount = intent.GrossAmount,
        FeeAmount = intent.FeeAmount, NetAmount = intent.NetAmount,
        Status = intent.Status.ToString(), Purpose = intent.Purpose.ToString(),
        WalletBehavior = intent.WalletBehavior.ToString(),
        PayerUserId = intent.PayerUserId, PayerWalletId = intent.PayerWalletId,
        RecipientWalletId = intent.RecipientWalletId,
        ExternalReferenceType = intent.ExternalReferenceType,
        ExternalReferenceId = intent.ExternalReferenceId,
        Description = intent.Description, ReturnUrl = intent.ReturnUrl,
        ExpiresAt = intent.ExpiresAt, IdempotencyKey = intent.IdempotencyKey,
        CreatedAt = intent.CreatedAt, UpdatedAt = intent.UpdatedAt,
        SucceededAt = intent.SucceededAt, FailedAt = intent.FailedAt,
        CancelledAt = intent.CancelledAt,
        WalletPostingStatus = intent.WalletPostingStatus,
        WalletPostedAt = intent.WalletPostedAt, WalletTransactionId = intent.WalletTransactionId
    };

    public static PaymentTransactionDto MapTxnToDto(PaymentTransaction txn) => new()
    {
        Id = txn.Id, PaymentIntentId = txn.PaymentIntentId,
        GatewayConfigId = txn.GatewayConfigId, TenantId = txn.TenantId,
        ProviderCode = txn.ProviderCode, AdapterType = txn.AdapterType,
        Amount = txn.Amount, Currency = txn.Currency,
        Status = txn.Status.ToString(), Authority = txn.Authority,
        GatewayReferenceId = txn.GatewayReferenceId, TraceNumber = txn.TraceNumber,
        Rrn = txn.Rrn, CardPanMasked = txn.CardPanMasked,
        GatewayResponseCode = txn.GatewayResponseCode,
        GatewayResponseMessage = txn.GatewayResponseMessage,
        CreatedAt = txn.CreatedAt, VerifiedAt = txn.VerifiedAt, FailedAt = txn.FailedAt
    };
}
