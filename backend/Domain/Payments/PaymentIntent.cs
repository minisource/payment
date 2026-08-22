using Domain.Enums;
using Domain.Events;
using Minisource.Common.Domain;

namespace Domain.Entities;

/// <summary>
/// Payment intent aggregate root. Represents a user's intent to pay via a gateway.
/// </summary>
public class PaymentIntent : AggregateRoot<Guid>
{
    public Guid TenantId { get; private set; }
    public string? ApplicationCode { get; private set; }

    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "IRT";

    public decimal? GrossAmount { get; private set; }
    public decimal FeeAmount { get; private set; }
    public decimal? NetAmount { get; private set; }

    public PaymentIntentStatus Status { get; private set; } = PaymentIntentStatus.Created;
    public PaymentIntentPurpose Purpose { get; private set; }

    public Guid? PayerUserId { get; private set; }
    public Guid? PayerWalletId { get; private set; }
    public Guid? RecipientWalletId { get; private set; }

    public WalletBehavior WalletBehavior { get; private set; } = WalletBehavior.None;

    public Guid? PaymentLinkId { get; private set; }

    public string? ExternalReferenceType { get; private set; }
    public string? ExternalReferenceId { get; private set; }

    public string? Description { get; private set; }
    public string? CallbackUrl { get; private set; }
    public string? ReturnUrl { get; private set; }

    public DateTime? ExpiresAt { get; private set; }

    public string? IdempotencyKey { get; private set; }
    public string? Metadata { get; private set; }

    public Guid? CreatedByUserId { get; private set; }
    public string? CreatedByClientId { get; private set; }

    public DateTime? SucceededAt { get; private set; }
    public DateTime? FailedAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public DateTime? ExpiredAt { get; private set; }

    // Wallet posting tracking
    public DateTime? WalletPostedAt { get; private set; }
    public Guid? WalletTransactionId { get; private set; }
    public string WalletPostingStatus { get; private set; } = "not_required";

    private readonly List<PaymentTransaction> _transactions = [];
    public IReadOnlyCollection<PaymentTransaction> Transactions => _transactions.AsReadOnly();

    private PaymentIntent() { }

    public static PaymentIntent Create(
        Guid tenantId,
        decimal amount,
        string currency,
        PaymentIntentPurpose purpose,
        WalletBehavior walletBehavior,
        string? applicationCode = null,
        Guid? payerUserId = null,
        Guid? payerWalletId = null,
        Guid? recipientWalletId = null,
        Guid? paymentLinkId = null,
        string? externalReferenceType = null,
        string? externalReferenceId = null,
        string? description = null,
        string? returnUrl = null,
        DateTime? expiresAt = null,
        string? idempotencyKey = null,
        string? metadata = null,
        Guid? createdByUserId = null,
        string? createdByClientId = null)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId is required", nameof(tenantId));
        if (amount <= 0) throw new ArgumentException("Amount must be positive", nameof(amount));
        if (string.IsNullOrWhiteSpace(currency)) throw new ArgumentException("Currency is required", nameof(currency));

        ValidateWalletBehavior(walletBehavior, payerWalletId, recipientWalletId);

        var intent = new PaymentIntent
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ApplicationCode = applicationCode,
            Amount = amount,
            Currency = currency.ToUpperInvariant(),
            Purpose = purpose,
            WalletBehavior = walletBehavior,
            PayerUserId = payerUserId,
            PayerWalletId = payerWalletId,
            RecipientWalletId = recipientWalletId,
            PaymentLinkId = paymentLinkId,
            ExternalReferenceType = externalReferenceType,
            ExternalReferenceId = externalReferenceId,
            Description = description,
            ReturnUrl = returnUrl,
            ExpiresAt = expiresAt,
            IdempotencyKey = idempotencyKey,
            Metadata = metadata,
            CreatedByUserId = createdByUserId,
            CreatedByClientId = createdByClientId,
            WalletPostingStatus = walletBehavior == WalletBehavior.None ? "not_required" : "pending"
        };

        intent.RaiseDomainEvent(new PaymentIntentCreatedEvent(intent.Id, tenantId, amount, currency, purpose.ToString()));
        return intent;
    }

    public void MarkGatewaySelected()
    {
        if (Status != PaymentIntentStatus.Created && Status != PaymentIntentStatus.Failed)
            throw new InvalidOperationException($"Cannot select gateway in {Status} status");
        Status = PaymentIntentStatus.GatewaySelected;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkRedirectRequired()
    {
        Status = PaymentIntentStatus.RedirectRequired;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkProcessing()
    {
        Status = PaymentIntentStatus.Processing;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkSucceeded(Guid? walletTransactionId = null)
    {
        if (Status == PaymentIntentStatus.Succeeded)
            return; // Idempotent — already succeeded
        Status = PaymentIntentStatus.Succeeded;
        SucceededAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        if (walletTransactionId.HasValue) WalletTransactionId = walletTransactionId;

        RaiseDomainEvent(new PaymentIntentSucceededEvent(Id, TenantId, Amount, Currency));
    }

    public void MarkFailed()
    {
        if (Status == PaymentIntentStatus.Succeeded)
            throw new InvalidOperationException("Cannot fail a succeeded payment intent");
        Status = PaymentIntentStatus.Failed;
        FailedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new PaymentIntentFailedEvent(Id, TenantId, Amount, Currency));
    }

    public void Cancel(string reason)
    {
        if (Status == PaymentIntentStatus.Succeeded)
            throw new InvalidOperationException("Cannot cancel a succeeded payment intent");
        if (Status == PaymentIntentStatus.Cancelled)
            return; // Already cancelled
        Status = PaymentIntentStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        Description = string.IsNullOrEmpty(Description) ? reason : Description + "; " + reason;

        RaiseDomainEvent(new PaymentIntentCancelledEvent(Id, TenantId, reason));
    }

    public void MarkExpired()
    {
        Status = PaymentIntentStatus.Expired;
        ExpiredAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkWalletPosted(Guid walletTransactionId)
    {
        WalletTransactionId = walletTransactionId;
        WalletPostedAt = DateTime.UtcNow;
        WalletPostingStatus = "posted";
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkWalletPostingFailed()
    {
        WalletPostingStatus = "failed";
        UpdatedAt = DateTime.UtcNow;
    }

    public bool CanStart => Status == PaymentIntentStatus.Created
        || Status == PaymentIntentStatus.GatewaySelected
        || Status == PaymentIntentStatus.Failed;

    public bool IsExpired => ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value;

    private static void ValidateWalletBehavior(WalletBehavior behavior, Guid? payerWalletId, Guid? recipientWalletId)
    {
        switch (behavior)
        {
            case WalletBehavior.CreditRecipientWallet:
                if (recipientWalletId == null || recipientWalletId == Guid.Empty)
                    throw new ArgumentException("recipientWalletId is required for CreditRecipientWallet");
                break;
            case WalletBehavior.CreditPayerWallet:
                if (payerWalletId == null || payerWalletId == Guid.Empty)
                    throw new ArgumentException("payerWalletId is required for CreditPayerWallet");
                break;
            case WalletBehavior.CreditPayerThenTransferToRecipient:
                if (payerWalletId == null || payerWalletId == Guid.Empty)
                    throw new ArgumentException("payerWalletId is required");
                if (recipientWalletId == null || recipientWalletId == Guid.Empty)
                    throw new ArgumentException("recipientWalletId is required");
                break;
        }
    }
}
