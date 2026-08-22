using Domain.Enums;
using Minisource.Common.Domain;

namespace Domain.Entities;

/// <summary>
/// Wallet hold/reservation record. Used by refund system to reserve wallet balance
/// before calling gateway refund API (strict pre-hold policy).
/// </summary>
public sealed class WalletHold : AggregateRoot<Guid>
{
    public Guid TenantId { get; private set; }
    public string? ApplicationCode { get; private set; }

    public Guid WalletAccountId { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = string.Empty;

    public WalletHoldStatus Status { get; private set; } = WalletHoldStatus.Active;

    // Reference to the originating operation
    public string ReferenceType { get; private set; } = string.Empty;
    public Guid ReferenceId { get; private set; }

    public string Reason { get; private set; } = string.Empty;
    public string? MetadataJson { get; private set; }
    public string? CreatedByUserId { get; private set; }

    // Timestamps
    public DateTime? CapturedAt { get; private set; }
    public DateTime? ReleasedAt { get; private set; }
    public DateTime? ExpiresAt { get; private set; }

    public DateTime? DeletedAt { get; private set; }

    private WalletHold() { }

    public static WalletHold Create(
        Guid tenantId,
        Guid walletAccountId,
        decimal amount,
        string currency,
        string referenceType,
        Guid referenceId,
        string reason,
        string? applicationCode = null,
        string? createdByUserId = null,
        string? metadataJson = null,
        DateTime? expiresAt = null)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("TenantId is required", nameof(tenantId));
        if (walletAccountId == Guid.Empty)
            throw new ArgumentException("WalletAccountId is required", nameof(walletAccountId));
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive", nameof(amount));
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency is required", nameof(currency));
        if (string.IsNullOrWhiteSpace(referenceType))
            throw new ArgumentException("ReferenceType is required", nameof(referenceType));
        if (referenceId == Guid.Empty)
            throw new ArgumentException("ReferenceId is required", nameof(referenceId));

        return new WalletHold
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ApplicationCode = applicationCode,
            WalletAccountId = walletAccountId,
            Amount = amount,
            Currency = currency.ToUpperInvariant(),
            Status = WalletHoldStatus.Active,
            ReferenceType = referenceType,
            ReferenceId = referenceId,
            Reason = reason,
            CreatedByUserId = createdByUserId,
            MetadataJson = metadataJson,
            ExpiresAt = expiresAt
        };
    }

    public void Capture()
    {
        EnsureStatus(WalletHoldStatus.Active);
        Status = WalletHoldStatus.Captured;
        CapturedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Release()
    {
        EnsureStatus(WalletHoldStatus.Active);
        Status = WalletHoldStatus.Released;
        ReleasedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Expire()
    {
        if (Status != WalletHoldStatus.Active)
            return;
        Status = WalletHoldStatus.Expired;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkFailed()
    {
        if (Status == WalletHoldStatus.Captured)
            throw new InvalidOperationException("Cannot fail captured hold");
        Status = WalletHoldStatus.Failed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SoftDelete()
    {
        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsActive => Status == WalletHoldStatus.Active && DeletedAt == null;

    private void EnsureStatus(WalletHoldStatus expected)
    {
        if (Status != expected)
            throw new InvalidOperationException(
                $"WalletHold {Id} has status {Status}, expected {expected}");
    }
}
