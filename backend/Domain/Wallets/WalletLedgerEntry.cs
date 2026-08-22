using Domain.Enums;
using Minisource.Common.Domain;

namespace Domain.Entities;

/// <summary>
/// Append-only wallet ledger entry. Records every balance change with before/after snapshots.
/// Immutable once posted.
/// </summary>
public class WalletLedgerEntry : Entity<Guid>
{
    public Guid WalletAccountId { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid? WalletTransactionId { get; set; }
    public LedgerEntryType EntryType { get; private set; }
    public LedgerDirection Direction { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; }
    public decimal BalanceAvailableBefore { get; private set; }
    public decimal BalanceAvailableAfter { get; private set; }
    public decimal BalanceLockedBefore { get; private set; }
    public decimal BalanceLockedAfter { get; private set; }
    public decimal BalancePendingBefore { get; private set; }
    public decimal BalancePendingAfter { get; private set; }
    public string? Reason { get; private set; }
    public string? ReferenceType { get; private set; }
    public string? ReferenceId { get; private set; }
    public Guid? CreatedByUserId { get; private set; }
    public string? CreatedByClientId { get; private set; }
    public string? Metadata { get; private set; }

    // Hash chain for tamper-evident ledger
    public string PreviousEntryHash { get; set; } = string.Empty;
    public string EntryHash { get; set; } = string.Empty;
    public string HashAlgorithm { get; set; } = "SHA256";
    public int HashVersion { get; set; } = 1;

    public WalletAccount WalletAccount { get; private set; } = null!;

    private WalletLedgerEntry() { }

    internal WalletLedgerEntry(
        Guid walletAccountId,
        Guid tenantId,
        LedgerEntryType entryType,
        LedgerDirection direction,
        decimal amount,
        string currency,
        decimal balanceAvailableBefore,
        decimal balanceAvailableAfter,
        decimal balanceLockedBefore,
        decimal balanceLockedAfter,
        decimal balancePendingBefore,
        decimal balancePendingAfter,
        string? reason = null,
        string? referenceType = null,
        string? referenceId = null,
        Guid? createdByUserId = null)
    {
        Id = Guid.NewGuid();
        WalletAccountId = walletAccountId;
        TenantId = tenantId;
        EntryType = entryType;
        Direction = direction;
        Amount = amount;
        Currency = currency.ToUpperInvariant();
        BalanceAvailableBefore = balanceAvailableBefore;
        BalanceAvailableAfter = balanceAvailableAfter;
        BalanceLockedBefore = balanceLockedBefore;
        BalanceLockedAfter = balanceLockedAfter;
        BalancePendingBefore = balancePendingBefore;
        BalancePendingAfter = balancePendingAfter;
        Reason = reason;
        ReferenceType = referenceType;
        ReferenceId = referenceId;
        CreatedByUserId = createdByUserId;
    }
}
