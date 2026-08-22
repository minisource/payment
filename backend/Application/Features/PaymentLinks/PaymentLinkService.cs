using System.Security.Cryptography;
using System.Text.Json;
using Application.Features.PaymentIntents;
using Application.DTOs;
using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using Microsoft.Extensions.Logging;
using Minisource.Common.Domain;
using Minisource.Common.Exceptions;

namespace Application.Features.PaymentLinks;

public class PaymentLinkService : IPaymentLinkService
{
    private readonly IPaymentLinkRepository _linkRepo;
    private readonly IPaymentIntentRepository _intentRepo;
    private readonly IWalletAccountRepository _walletRepo;
    private readonly IAuditLogRepository _auditRepo;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<PaymentLinkService> _logger;
    private const int TokenLength = 32;

    public PaymentLinkService(
        IPaymentLinkRepository linkRepo,
        IPaymentIntentRepository intentRepo,
        IWalletAccountRepository walletRepo,
        IAuditLogRepository auditRepo,
        IUnitOfWork uow,
        ILogger<PaymentLinkService> logger)
    {
        _linkRepo = linkRepo;
        _intentRepo = intentRepo;
        _walletRepo = walletRepo;
        _auditRepo = auditRepo;
        _uow = uow;
        _logger = logger;
    }

    public async Task<CreatePaymentLinkResponse> CreateAsync(Guid tenantId, Guid actorUserId,
        CreatePaymentLinkRequest request, string? applicationCode, string? publicBaseUrl, CancellationToken ct)
    {
        // Validate recipient wallet
        var wallet = await _walletRepo.GetByIdAsync(request.RecipientWalletId, ct)
            ?? throw new BusinessException("Recipient wallet not found", "payment_link_recipient_wallet_invalid");

        if (wallet.TenantId != tenantId)
            throw new BusinessException("Wallet does not belong to tenant", "payment_link_recipient_wallet_invalid");

        if (!wallet.Currency.Equals(request.Currency, StringComparison.OrdinalIgnoreCase))
            throw new BusinessException("Currency mismatch with recipient wallet", "payment_link_currency_mismatch");

        // Parse amount type
        var amountType = Enum.TryParse<PaymentLinkAmountType>(request.AmountType, true, out var at)
            ? at : PaymentLinkAmountType.Open;

        // Generate token
        var rawToken = GenerateToken();
        var tokenHash = HashToken(rawToken);

        var link = PaymentLink.Create(
            tenantId, "user", actorUserId, request.RecipientWalletId,
            request.Title, request.Currency, amountType, tokenHash,
            applicationCode: applicationCode,
            description: request.Description,
            fixedAmount: request.FixedAmount,
            suggestedAmount: request.SuggestedAmount,
            minAmount: request.MinAmount,
            maxAmount: request.MaxAmount,
            allowAnonymousPayer: request.AllowAnonymousPayer,
            requirePayerName: request.RequirePayerName,
            requirePayerMobile: request.RequirePayerMobile,
            requireDescription: request.RequireDescription,
            successMessage: request.SuccessMessage,
            failureMessage: request.FailureMessage,
            returnUrl: request.ReturnUrl,
            expiresAt: request.ExpiresAt,
            usageLimit: request.UsageLimit,
            publicCode: request.PublicCode,
            metadata: request.Metadata,
            createdByUserId: actorUserId);

        await _linkRepo.AddAsync(link, ct);

        await _auditRepo.AddAsync(new AuditLog
        {
            TenantId = tenantId, ActorUserId = actorUserId,
            Action = "payment_link.created", EntityType = "PaymentLink",
            EntityId = link.Id.ToString(),
            AfterSnapshot = JsonSerializer.Serialize(new { link.Title, link.AmountType, link.Currency })
        }, ct);

        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("Payment link {LinkId} created for tenant {TenantId}", link.Id, tenantId);

        var baseUrl = publicBaseUrl ?? "https://payment.example.com";
        return new CreatePaymentLinkResponse
        {
            Id = link.Id,
            PublicUrl = $"{baseUrl}/p/{rawToken}",
            Token = rawToken,
            Title = link.Title,
            Status = link.Status.ToString().ToLowerInvariant()
        };
    }

    public async Task<PaymentLinkDto> GetAsync(Guid paymentLinkId, Guid? tenantId, string? publicBaseUrl, CancellationToken ct)
    {
        var link = await _linkRepo.GetByIdAsync(paymentLinkId, ct)
            ?? throw new NotFoundException("PaymentLink", paymentLinkId);
        
        if (tenantId.HasValue && link.TenantId != tenantId.Value)
            throw new ForbiddenException("Payment link does not belong to tenant");
        return MapToDto(link, publicBaseUrl);
    }

    public async Task<List<PaymentLinkDto>> GetOwnerLinksAsync(Guid tenantId, string ownerType, Guid ownerId,
        int skip, int take, string? publicBaseUrl, CancellationToken ct)
    {
        var links = await _linkRepo.GetByOwnerAsync(tenantId, ownerType, ownerId, skip, take, ct);
        return links.Select(l => MapToDto(l, publicBaseUrl)).ToList();
    }

    public async Task<PaymentLinkDto> UpdateAsync(Guid paymentLinkId, Guid? tenantId, Guid actorUserId,
        UpdatePaymentLinkRequest request, CancellationToken ct)
    {
        var link = await _linkRepo.GetByIdAsync(paymentLinkId, ct)
            ?? throw new NotFoundException("PaymentLink", paymentLinkId);

        if (tenantId.HasValue && link.TenantId != tenantId.Value)
            throw new ForbiddenException("Payment link does not belong to tenant");

        var amountType = request.AmountType != null
            ? Enum.TryParse<PaymentLinkAmountType>(request.AmountType, true, out var at) ? at : (PaymentLinkAmountType?)null
            : null;

        link.Update(request.Title, request.Description, amountType,
            request.FixedAmount, request.SuggestedAmount, request.MinAmount, request.MaxAmount,
            request.AllowAnonymousPayer, request.RequirePayerName,
            request.RequirePayerMobile, request.RequireDescription,
            request.SuccessMessage, request.FailureMessage,
            request.ReturnUrl, request.ExpiresAt, request.UsageLimit, request.Metadata);

        await _linkRepo.UpdateAsync(link, ct);

        await _auditRepo.AddAsync(new AuditLog
        {
            TenantId = tenantId, ActorUserId = actorUserId,
            Action = "payment_link.updated", EntityType = "PaymentLink",
            EntityId = link.Id.ToString()
        }, ct);

        await _uow.SaveChangesAsync(ct);

        return MapToDto(link, null);
    }

    public async Task PauseAsync(Guid paymentLinkId, Guid? tenantId, CancellationToken ct)
    {
        var link = await _linkRepo.GetByIdAsync(paymentLinkId, ct)
            ?? throw new NotFoundException("PaymentLink", paymentLinkId);
        if (tenantId.HasValue && link.TenantId != tenantId.Value) throw new ForbiddenException();
        link.Pause();
        await _linkRepo.UpdateAsync(link, ct);
        await _auditRepo.AddAsync(new AuditLog { TenantId = tenantId, Action = "payment_link.paused", EntityType = "PaymentLink", EntityId = link.Id.ToString() }, ct);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task ResumeAsync(Guid paymentLinkId, Guid? tenantId, CancellationToken ct)
    {
        var link = await _linkRepo.GetByIdAsync(paymentLinkId, ct)
            ?? throw new NotFoundException("PaymentLink", paymentLinkId);
        if (tenantId.HasValue && link.TenantId != tenantId.Value) throw new ForbiddenException();
        link.Resume();
        await _linkRepo.UpdateAsync(link, ct);
        await _auditRepo.AddAsync(new AuditLog { TenantId = tenantId, Action = "payment_link.resumed", EntityType = "PaymentLink", EntityId = link.Id.ToString() }, ct);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task DisableAsync(Guid paymentLinkId, Guid? tenantId, CancellationToken ct)
    {
        var link = await _linkRepo.GetByIdAsync(paymentLinkId, ct)
            ?? throw new NotFoundException("PaymentLink", paymentLinkId);
        if (tenantId.HasValue && link.TenantId != tenantId.Value) throw new ForbiddenException();
        link.Disable();
        await _linkRepo.UpdateAsync(link, ct);
        await _auditRepo.AddAsync(new AuditLog { TenantId = tenantId, Action = "payment_link.disabled", EntityType = "PaymentLink", EntityId = link.Id.ToString() }, ct);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task SoftDeleteAsync(Guid paymentLinkId, Guid? tenantId, CancellationToken ct)
    {
        var link = await _linkRepo.GetByIdAsync(paymentLinkId, ct)
            ?? throw new NotFoundException("PaymentLink", paymentLinkId);
        if (tenantId.HasValue && link.TenantId != tenantId.Value) throw new ForbiddenException();
        link.SoftDelete();
        await _linkRepo.UpdateAsync(link, ct);
        await _auditRepo.AddAsync(new AuditLog { TenantId = tenantId, Action = "payment_link.deleted", EntityType = "PaymentLink", EntityId = link.Id.ToString() }, ct);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task<List<PaymentIntentDto>> GetPaymentsAsync(Guid paymentLinkId, CancellationToken ct)
    {
        // Find intents linked to this payment link via external_reference
        var link = await _linkRepo.GetByIdAsync(paymentLinkId, ct)
            ?? throw new NotFoundException("PaymentLink", paymentLinkId);
        var intents = await _intentRepo.GetByExternalReferenceAsync(link.TenantId, "payment_link", paymentLinkId.ToString(), ct);
        return intents.Select(PaymentIntentService.MapIntentToDto).ToList();
    }

    public async Task RecordSuccessfulPaymentAsync(Guid paymentLinkId, decimal amount, CancellationToken ct)
    {
        var link = await _linkRepo.GetByIdAsync(paymentLinkId, ct);
        if (link == null) return;
        link.RecordSuccessfulPayment(amount);
        await _linkRepo.UpdateAsync(link, ct);
        await _uow.SaveChangesAsync(ct);
    }

    public Task<PaymentLink?> GetByTokenHashAsync(string tokenHash, CancellationToken ct)
        => _linkRepo.GetByTokenHashAsync(tokenHash, ct);

    // ═══ Token helpers ═══════════════════════════════════

    public static string GenerateToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(TokenLength);
        return "plink_" + ConvertToBase64Url(bytes);
    }

    public static string HashToken(string token)
    {
        var hash = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(hash);
    }

    private static string ConvertToBase64Url(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }

    // ═══ Mapping ═════════════════════════════════════════

    private static PaymentLinkDto MapToDto(PaymentLink link, string? publicBaseUrl) => new()
    {
        Id = link.Id, TenantId = link.TenantId, OwnerType = link.OwnerType,
        OwnerId = link.OwnerId, RecipientWalletId = link.RecipientWalletId,
        PublicCode = link.PublicCode, Title = link.Title, Description = link.Description,
        AmountType = link.AmountType.ToString().ToLowerInvariant(),
        FixedAmount = link.FixedAmount, SuggestedAmount = link.SuggestedAmount,
        MinAmount = link.MinAmount, MaxAmount = link.MaxAmount, Currency = link.Currency,
        Status = link.Status.ToString().ToLowerInvariant(),
        AllowAnonymousPayer = link.AllowAnonymousPayer,
        RequirePayerName = link.RequirePayerName,
        RequirePayerMobile = link.RequirePayerMobile,
        RequireDescription = link.RequireDescription,
        SuccessMessage = link.SuccessMessage, FailureMessage = link.FailureMessage,
        ReturnUrl = link.ReturnUrl, ExpiresAt = link.ExpiresAt,
        UsageLimit = link.UsageLimit,
        SuccessfulPaymentCount = link.SuccessfulPaymentCount,
        TotalPaidAmount = link.TotalPaidAmount,
        PublicUrl = publicBaseUrl != null ? $"{publicBaseUrl}/p/{link.PublicCode ?? link.Id.ToString("N")[..8]}" : null,
        CreatedAt = link.CreatedAt, UpdatedAt = link.UpdatedAt
    };
}
