using Minisource.Common.Domain;

namespace Domain.Entities;

/// <summary>
/// Payout account / bank account for withdrawals. Stores masked/hashed sensitive data.
/// </summary>
public class PayoutAccount : AggregateRoot<Guid>
{
    public Guid TenantId { get; private set; }
    public string OwnerType { get; private set; } = "user";
    public Guid OwnerId { get; private set; }
    public string AccountType { get; private set; } = "iban";
    public string? BankName { get; private set; }
    public string? BankCode { get; private set; }
    public string? CardNumberMasked { get; private set; }
    public string? CardNumberHash { get; private set; }
    public string? Iban { get; private set; }
    public string? IbanHash { get; private set; }
    public string? AccountNumberMasked { get; private set; }
    public string? AccountNumberHash { get; private set; }
    public string HolderName { get; private set; } = string.Empty;
    public string? HolderNationalIdHash { get; private set; }
    public string Currency { get; private set; } = "IRT";
    public PayoutAccountStatus Status { get; private set; } = PayoutAccountStatus.PendingVerification;
    public bool IsDefault { get; private set; }
    public DateTime? VerifiedAt { get; private set; }
    public Guid? VerifiedByUserId { get; private set; }
    public DateTime? RejectedAt { get; private set; }
    public string? RejectionReason { get; private set; }
    public DateTime? DisabledAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public string? Metadata { get; private set; }
    public Guid? CreatedByUserId { get; private set; }
    public string? CreatedByClientId { get; private set; }

    private PayoutAccount() { }

    public static PayoutAccount Create(
        Guid tenantId, string ownerType, Guid ownerId, string accountType,
        string holderName, string currency,
        string? bankName = null, string? bankCode = null,
        string? cardNumberMasked = null, string? cardNumberHash = null,
        string? iban = null, string? ibanHash = null,
        string? accountNumberMasked = null, string? accountNumberHash = null,
        string? metadata = null, Guid? createdByUserId = null)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId required", nameof(tenantId));
        if (string.IsNullOrWhiteSpace(ownerType)) throw new ArgumentException("OwnerType required", nameof(ownerType));
        if (ownerId == Guid.Empty) throw new ArgumentException("OwnerId required", nameof(ownerId));
        if (string.IsNullOrWhiteSpace(holderName)) throw new ArgumentException("HolderName required", nameof(holderName));

        return new PayoutAccount
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            OwnerType = ownerType.ToLowerInvariant(),
            OwnerId = ownerId,
            AccountType = accountType.ToLowerInvariant(),
            BankName = bankName, BankCode = bankCode,
            CardNumberMasked = cardNumberMasked, CardNumberHash = cardNumberHash,
            Iban = iban, IbanHash = ibanHash,
            AccountNumberMasked = accountNumberMasked, AccountNumberHash = accountNumberHash,
            HolderName = holderName,
            Currency = currency.ToUpperInvariant(),
            Status = PayoutAccountStatus.PendingVerification,
            Metadata = metadata,
            CreatedByUserId = createdByUserId
        };
    }

    public void Update(string? holderName, string? bankName, string? bankCode, string? metadata)
    {
        if (holderName != null) HolderName = holderName;
        if (bankName != null) BankName = bankName;
        if (bankCode != null) BankCode = bankCode;
        if (metadata != null) Metadata = metadata;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetDefault()
    {
        IsDefault = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ClearDefault()
    {
        IsDefault = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Verify(Guid verifiedByUserId)
    {
        Status = PayoutAccountStatus.Verified;
        VerifiedAt = DateTime.UtcNow;
        VerifiedByUserId = verifiedByUserId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reject(string reason)
    {
        Status = PayoutAccountStatus.Rejected;
        RejectedAt = DateTime.UtcNow;
        RejectionReason = reason;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Disable()
    {
        Status = PayoutAccountStatus.Disabled;
        DisabledAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SoftDelete()
    {
        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool CanBeUsed => Status == PayoutAccountStatus.Verified
        && DeletedAt == null
        && DisabledAt == null;
}

public enum PayoutAccountStatus
{
    PendingVerification = 1,
    Verified = 2,
    Rejected = 3,
    Disabled = 4,
    Deleted = 5
}
