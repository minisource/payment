using Application.DTOs;
using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using Microsoft.Extensions.Logging;
using Minisource.Common.Domain;
using Application.Features.PaymentIntents;
using Application.Features.Gateways;
using Minisource.Common.Exceptions;

namespace Application.Features.PaymentLinks;

public class PublicPaymentService : IPublicPaymentService
{
    private readonly IPaymentLinkService _linkSvc;
    private readonly IPaymentIntentRepository _intentRepo;
    private readonly IPaymentIntentService _intentSvc;
    private readonly IGatewayPaymentService _gatewaySvc;
    private readonly IAuditLogRepository _auditRepo;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<PublicPaymentService> _logger;

    public PublicPaymentService(
        IPaymentLinkService linkSvc,
        IPaymentIntentRepository intentRepo,
        IPaymentIntentService intentSvc,
        IGatewayPaymentService gatewaySvc,
        IAuditLogRepository auditRepo,
        IUnitOfWork uow,
        ILogger<PublicPaymentService> logger)
    {
        _linkSvc = linkSvc;
        _intentRepo = intentRepo;
        _intentSvc = intentSvc;
        _gatewaySvc = gatewaySvc;
        _auditRepo = auditRepo;
        _uow = uow;
        _logger = logger;
    }

    public async Task<PublicPaymentLinkResponse> GetPublicLinkAsync(string rawToken, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(rawToken))
            throw new BusinessException("Payment link not found", "payment_link_not_found");

        var tokenHash = PaymentLinkService.HashToken(rawToken);
        var link = await _linkSvc.GetByTokenHashAsync(tokenHash, ct);

        if (link == null || !link.CanBePaid)
        {
            // Return consistent inactive response — don't reveal if token exists
            return new PublicPaymentLinkResponse
            {
                Title = "Payment Link",
                Status = link?.Status.ToString().ToLowerInvariant() ?? "inactive",
                AmountType = "open", Currency = "IRT"
            };
        }

        return new PublicPaymentLinkResponse
        {
            PaymentLinkId = link.Id.ToString(),
            Title = link.Title,
            Description = link.Description,
            AmountType = link.AmountType.ToString().ToLowerInvariant(),
            FixedAmount = link.FixedAmount,
            SuggestedAmount = link.SuggestedAmount,
            MinAmount = link.MinAmount,
            MaxAmount = link.MaxAmount,
            Currency = link.Currency,
            Status = "active",
            RequirePayerName = link.RequirePayerName,
            RequirePayerMobile = link.RequirePayerMobile,
            RequireDescription = link.RequireDescription,
            GatewayEnabled = true,
            ExpiresAt = link.ExpiresAt,
            SuccessMessage = link.SuccessMessage
        };
    }

    public async Task<PublicPayResponse> StartPublicPaymentAsync(string rawToken, PublicPayRequest request,
        string? applicationCode, string? ipAddress, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(rawToken))
            throw new BusinessException("Payment link not found", "payment_link_token_invalid");

        // Resolve payment link
        var tokenHash = PaymentLinkService.HashToken(rawToken);
        var link = await _linkSvc.GetByTokenHashAsync(tokenHash, ct)
            ?? throw new BusinessException("Payment link not found", "payment_link_not_found");

        // Validate link state
        if (!link.CanBePaid)
        {
            var reason = link.IsExpired ? "payment_link_expired"
                : link.Status == PaymentLinkStatus.Paused ? "payment_link_inactive"
                : link.Status == PaymentLinkStatus.Disabled ? "payment_link_disabled"
                : link.DeletedAt != null ? "payment_link_deleted"
                : "payment_link_usage_limit_reached";

            throw new BusinessException($"Payment link cannot be paid", reason);
        }

        // Validate payer fields
        if (link.RequirePayerName && string.IsNullOrWhiteSpace(request.PayerName))
            throw new BusinessException("Payer name is required", "payment_link_payer_name_required");
        if (link.RequirePayerMobile && string.IsNullOrWhiteSpace(request.PayerMobile))
            throw new BusinessException("Payer mobile is required", "payment_link_payer_mobile_required");
        if (link.RequireDescription && string.IsNullOrWhiteSpace(request.PayerDescription))
            throw new BusinessException("Description is required", "payment_link_description_required");

        // Resolve and validate amount
        var amount = link.ResolveAmount(request.Amount);
        link.ValidatePayerAmount(amount);

        // Build payer metadata (safe, sanitized)
        var payerMeta = System.Text.Json.JsonSerializer.Serialize(new
        {
            payer_name = request.PayerName,
            payer_mobile = request.PayerMobile,
            payer_description = request.PayerDescription,
            ip_address = ipAddress
        });

        // Create payment intent
        var intent = PaymentIntent.Create(
            link.TenantId, amount, link.Currency,
            PaymentIntentPurpose.PublicPayment,
            WalletBehavior.CreditRecipientWallet,
            applicationCode: link.ApplicationCode,
            recipientWalletId: link.RecipientWalletId,
            paymentLinkId: link.Id,
            externalReferenceType: "payment_link",
            externalReferenceId: link.Id.ToString(),
            description: $"Payment via link: {link.Title}",
            returnUrl: link.ReturnUrl,
            expiresAt: link.ExpiresAt,
            metadata: payerMeta);

        await _intentRepo.AddAsync(intent, ct);

        // Audit
        await _auditRepo.AddAsync(new AuditLog
        {
            TenantId = link.TenantId,
            Action = "payment_link.payment_started",
            EntityType = "PaymentLink", EntityId = link.Id.ToString(),
            AfterSnapshot = System.Text.Json.JsonSerializer.Serialize(new
            {
                PaymentIntentId = intent.Id, amount, link.Currency
            })
        }, ct);

        await _uow.SaveChangesAsync(ct);

        // Start gateway payment using existing gateway flow
        var startRequest = new StartPaymentIntentRequest
        {
            PreferredProviderCode = request.PreferredProviderCode,
            Metadata = request.Metadata
        };

        var startResult = await _gatewaySvc.StartPaymentAsync(
            link.TenantId, intent.Id, startRequest, link.ApplicationCode, ct);

        _logger.LogInformation("Public payment started: link {LinkId}, intent {IntentId}, txn {TxnId}",
            link.Id, intent.Id, startResult.PaymentTransactionId);

        return new PublicPayResponse
        {
            PaymentIntentId = startResult.PaymentIntentId.ToString(),
            PaymentTransactionId = startResult.PaymentTransactionId.ToString(),
            Status = startResult.Status,
            RedirectUrl = startResult.RedirectUrl
        };
    }
}
