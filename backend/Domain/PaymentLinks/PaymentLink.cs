using Domain.Enums;
using Domain.Events;
using Minisource.Common.Domain;

namespace Domain.Entities;

/// <summary>
/// Public payment link. Generates a unique token that payers use to pay a recipient.
/// Token hash is stored; raw token returned only on creation.
/// </summary>
public class PaymentLink : AggregateRoot<Guid>
{
    public Guid TenantId { get; private set; }
    public string? ApplicationCode { get; private set; }

    public string OwnerType { get; private set; } = "user";
    public Guid OwnerId { get; private set; }

    public Guid RecipientWalletId { get; private set; }

    public string TokenHash { get; private set; } = string.Empty;
    public string? PublicCode { get; private set; }

    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    public PaymentLinkAmountType AmountType { get; private set; } = PaymentLinkAmountType.Open;
    public decimal? FixedAmount { get; private set; }
    public decimal? SuggestedAmount { get; private set; }
    public decimal? MinAmount { get; private set; }
    public decimal? MaxAmount { get; private set; }
    public string Currency { get; private set; } = "IRT";

    public PaymentLinkStatus Status { get; private set; } = PaymentLinkStatus.Active;

    public bool AllowAnonymousPayer { get; private set; } = true;
    public bool RequirePayerName { get; private set; }
    public bool RequirePayerMobile { get; private set; }
    public bool RequireDescription { get; private set; }

    public string? SuccessMessage { get; private set; }
    public string? FailureMessage { get; private set; }

    public string? ReturnUrl { get; private set; }

    public DateTime? ExpiresAt { get; private set; }

    public int? UsageLimit { get; private set; }
    public int SuccessfulPaymentCount { get; private set; }
    public decimal TotalPaidAmount { get; private set; }

    public string? Metadata { get; private set; }

    public Guid? CreatedByUserId { get; private set; }
    public string? CreatedByClientId { get; private set; }

    public DateTime? PausedAt { get; private set; }
    public DateTime? DisabledAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    private PaymentLink() { }

    public static PaymentLink Create(
        Guid tenantId,
        string ownerType,
        Guid ownerId,
        Guid recipientWalletId,
        string title,
        string currency,
        PaymentLinkAmountType amountType,
        string tokenHash,
        string? applicationCode = null,
        string? description = null,
        decimal? fixedAmount = null,
        decimal? suggestedAmount = null,
        decimal? minAmount = null,
        decimal? maxAmount = null,
        bool allowAnonymousPayer = true,
        bool requirePayerName = false,
        bool requirePayerMobile = false,
        bool requireDescription = false,
        string? successMessage = null,
        string? failureMessage = null,
        string? returnUrl = null,
        DateTime? expiresAt = null,
        int? usageLimit = null,
        string? publicCode = null,
        string? metadata = null,
        Guid? createdByUserId = null,
        string? createdByClientId = null)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId is required", nameof(tenantId));
        if (recipientWalletId == Guid.Empty) throw new ArgumentException("RecipientWalletId is required", nameof(recipientWalletId));
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title is required", nameof(title));
        if (string.IsNullOrWhiteSpace(currency)) throw new ArgumentException("Currency is required", nameof(currency));
        if (string.IsNullOrWhiteSpace(tokenHash)) throw new ArgumentException("TokenHash is required", nameof(tokenHash));

        ValidateAmountType(amountType, fixedAmount, suggestedAmount, minAmount, maxAmount);

        var link = new PaymentLink
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ApplicationCode = applicationCode,
            OwnerType = ownerType.ToLowerInvariant(),
            OwnerId = ownerId,
            RecipientWalletId = recipientWalletId,
            TokenHash = tokenHash,
            PublicCode = publicCode,
            Title = title,
            Description = description,
            AmountType = amountType,
            FixedAmount = fixedAmount,
            SuggestedAmount = suggestedAmount,
            MinAmount = minAmount,
            MaxAmount = maxAmount,
            Currency = currency.ToUpperInvariant(),
            AllowAnonymousPayer = allowAnonymousPayer,
            RequirePayerName = requirePayerName,
            RequirePayerMobile = requirePayerMobile,
            RequireDescription = requireDescription,
            SuccessMessage = successMessage,
            FailureMessage = failureMessage,
            ReturnUrl = returnUrl,
            ExpiresAt = expiresAt,
            UsageLimit = usageLimit,
            Metadata = metadata,
            CreatedByUserId = createdByUserId,
            CreatedByClientId = createdByClientId
        };

        link.RaiseDomainEvent(new PaymentLinkCreatedEvent(link.Id, tenantId, recipientWalletId, title, link.CreatedAt));
        return link;
    }

    public void Update(string? title, string? description, PaymentLinkAmountType? amountType,
        decimal? fixedAmount, decimal? suggestedAmount, decimal? minAmount, decimal? maxAmount,
        bool? allowAnonymousPayer, bool? requirePayerName, bool? requirePayerMobile,
        bool? requireDescription, string? successMessage, string? failureMessage,
        string? returnUrl, DateTime? expiresAt, int? usageLimit, string? metadata)
    {
        if (title != null) Title = title;
        if (description != null) Description = description;
        if (amountType.HasValue) AmountType = amountType.Value;
        if (fixedAmount.HasValue) FixedAmount = fixedAmount;
        if (suggestedAmount.HasValue) SuggestedAmount = suggestedAmount;
        if (minAmount.HasValue) MinAmount = minAmount;
        if (maxAmount.HasValue) MaxAmount = maxAmount;
        if (allowAnonymousPayer.HasValue) AllowAnonymousPayer = allowAnonymousPayer.Value;
        if (requirePayerName.HasValue) RequirePayerName = requirePayerName.Value;
        if (requirePayerMobile.HasValue) RequirePayerMobile = requirePayerMobile.Value;
        if (requireDescription.HasValue) RequireDescription = requireDescription.Value;
        if (successMessage != null) SuccessMessage = successMessage;
        if (failureMessage != null) FailureMessage = failureMessage;
        if (returnUrl != null) ReturnUrl = returnUrl;
        if (expiresAt.HasValue) ExpiresAt = expiresAt;
        if (usageLimit.HasValue) UsageLimit = usageLimit;
        if (metadata != null) Metadata = metadata;

        ValidateAmountType(AmountType, FixedAmount, SuggestedAmount, MinAmount, MaxAmount);
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new PaymentLinkUpdatedEvent(Id, TenantId));
    }

    public void Pause()
    {
        if (Status != PaymentLinkStatus.Active)
            throw new InvalidOperationException($"Cannot pause link in {Status} status");
        Status = PaymentLinkStatus.Paused;
        PausedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Resume()
    {
        if (Status != PaymentLinkStatus.Paused)
            throw new InvalidOperationException($"Cannot resume link in {Status} status");
        if (IsExpired)
            throw new InvalidOperationException("Cannot resume expired link");
        Status = PaymentLinkStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Disable()
    {
        Status = PaymentLinkStatus.Disabled;
        DisabledAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SoftDelete()
    {
        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecordSuccessfulPayment(decimal amount)
    {
        SuccessfulPaymentCount++;
        TotalPaidAmount += amount;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool CanBePaid => Status == PaymentLinkStatus.Active
        && DeletedAt == null
        && !IsExpired
        && (!UsageLimit.HasValue || SuccessfulPaymentCount < UsageLimit.Value);

    public bool IsExpired => ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value;

    public decimal ResolveAmount(decimal? payerAmount)
    {
        return AmountType switch
        {
            PaymentLinkAmountType.Fixed => FixedAmount ?? throw new InvalidOperationException("Fixed amount not set"),
            PaymentLinkAmountType.Open => payerAmount ?? throw new ArgumentException("Amount is required for open payment links"),
            PaymentLinkAmountType.Suggested => payerAmount ?? SuggestedAmount ?? throw new ArgumentException("Amount is required"),
            _ => throw new InvalidOperationException($"Unknown amount type: {AmountType}")
        };
    }

    public void ValidatePayerAmount(decimal amount)
    {
        if (MinAmount.HasValue && amount < MinAmount.Value)
            throw new ArgumentException($"Amount must be at least {MinAmount}");
        if (MaxAmount.HasValue && amount > MaxAmount.Value)
            throw new ArgumentException($"Amount must not exceed {MaxAmount}");
    }

    private static void ValidateAmountType(PaymentLinkAmountType type, decimal? fixedAmount,
        decimal? suggestedAmount, decimal? minAmount, decimal? maxAmount)
    {
        if (type == PaymentLinkAmountType.Fixed && (!fixedAmount.HasValue || fixedAmount <= 0))
            throw new ArgumentException("Fixed amount is required and must be positive for fixed links");
        if (minAmount.HasValue && minAmount <= 0)
            throw new ArgumentException("Min amount must be positive");
        if (maxAmount.HasValue && maxAmount <= 0)
            throw new ArgumentException("Max amount must be positive");
        if (minAmount.HasValue && maxAmount.HasValue && minAmount > maxAmount)
            throw new ArgumentException("Min amount cannot exceed max amount");
    }
}
