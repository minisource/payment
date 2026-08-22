using Domain.Enums;
using Domain.Events;
using Minisource.Common.Domain;

namespace Domain.Entities;

/// <summary>
/// Wallet account aggregate root. Tenant-aware multi-currency wallet
/// with available, locked, and pending balance tracking.
/// </summary>
public class WalletAccount : AggregateRoot<Guid>
{
    public Guid TenantId { get; private set; }
    public string OwnerType { get; private set; } = "user";
    public Guid OwnerId { get; private set; }
    public string Currency { get; private set; } = "IRT";
    public decimal AvailableBalance { get; private set; }
    public decimal LockedBalance { get; private set; }
    public decimal PendingBalance { get; private set; }
    public WalletAccountStatus Status { get; private set; } = WalletAccountStatus.Active;
    public long Version { get; private set; }
    public string? Metadata { get; private set; }

    private readonly List<WalletLedgerEntry> _ledgerEntries = [];
    public IReadOnlyCollection<WalletLedgerEntry> LedgerEntries => _ledgerEntries.AsReadOnly();

    private WalletAccount() { }

    public static WalletAccount Create(Guid tenantId, string ownerType, Guid ownerId, string currency)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId is required", nameof(tenantId));
        if (string.IsNullOrWhiteSpace(ownerType)) throw new ArgumentException("OwnerType is required", nameof(ownerType));
        if (ownerId == Guid.Empty) throw new ArgumentException("OwnerId is required", nameof(ownerId));
        if (string.IsNullOrWhiteSpace(currency)) throw new ArgumentException("Currency is required", nameof(currency));

        var account = new WalletAccount
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            OwnerType = ownerType.ToLowerInvariant(),
            OwnerId = ownerId,
            Currency = currency.ToUpperInvariant(),
            Status = WalletAccountStatus.Active,
            Version = 0
        };

        account.RaiseDomainEvent(new WalletAccountCreatedEvent(account.Id, tenantId, ownerType, ownerId, currency, account.CreatedAt));
        return account;
    }

    public WalletLedgerEntry PostCredit(
        decimal amount,
        LedgerEntryType entryType,
        Guid? walletTransactionId = null,
        string? reason = null,
        string? referenceType = null,
        string? referenceId = null,
        Guid? createdByUserId = null)
    {
        EnsureCanMutate();
        if (amount <= 0) throw new ArgumentException("Credit amount must be positive", nameof(amount));

        var entry = new WalletLedgerEntry(
            Id, TenantId, entryType, LedgerDirection.Credit,
            amount, Currency,
            AvailableBalance, AvailableBalance + amount,
            LockedBalance, LockedBalance,
            PendingBalance, PendingBalance,
            reason, referenceType, referenceId, createdByUserId)
        {
            WalletTransactionId = walletTransactionId
        };

        _ledgerEntries.Add(entry);
        AvailableBalance += amount;
        Version++;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new WalletBalanceChangedEvent(Id, TenantId, entry.Id, amount, AvailableBalance, Currency, DateTime.UtcNow));
        return entry;
    }

    public WalletLedgerEntry PostDebit(
        decimal amount,
        LedgerEntryType entryType,
        bool allowNegative = false,
        Guid? walletTransactionId = null,
        string? reason = null,
        string? referenceType = null,
        string? referenceId = null,
        Guid? createdByUserId = null)
    {
        EnsureCanMutate();
        if (amount <= 0) throw new ArgumentException("Debit amount must be positive", nameof(amount));

        if (!allowNegative && amount > AvailableBalance)
            throw new InvalidOperationException($"Insufficient available balance. Available: {AvailableBalance} {Currency}, Requested: {amount} {Currency}");

        var newBalance = AvailableBalance - amount;
        var entry = new WalletLedgerEntry(
            Id, TenantId, entryType, LedgerDirection.Debit,
            amount, Currency,
            AvailableBalance, newBalance,
            LockedBalance, LockedBalance,
            PendingBalance, PendingBalance,
            reason, referenceType, referenceId, createdByUserId)
        {
            WalletTransactionId = walletTransactionId
        };

        _ledgerEntries.Add(entry);
        AvailableBalance = newBalance;
        Version++;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new WalletBalanceChangedEvent(Id, TenantId, entry.Id, -amount, AvailableBalance, Currency, DateTime.UtcNow));
        return entry;
    }

    /// <summary>Lock funds from available for withdrawal. Moves from available to locked.</summary>
    public WalletLedgerEntry LockFunds(
        decimal amount,
        Guid? walletTransactionId = null,
        string? reason = null,
        string? referenceType = null,
        string? referenceId = null,
        Guid? createdByUserId = null)
    {
        EnsureCanMutate();
        if (amount <= 0) throw new ArgumentException("Lock amount must be positive", nameof(amount));
        if (amount > AvailableBalance)
            throw new InvalidOperationException($"Insufficient available balance for lock. Available: {AvailableBalance} {Currency}, Requested: {amount} {Currency}");

        var entry = new WalletLedgerEntry(
            Id, TenantId, LedgerEntryType.WithdrawalLock,
            LedgerDirection.Lock, amount, Currency,
            AvailableBalance, AvailableBalance - amount,
            LockedBalance, LockedBalance + amount,
            PendingBalance, PendingBalance,
            reason, referenceType, referenceId, createdByUserId)
        { WalletTransactionId = walletTransactionId };

        _ledgerEntries.Add(entry);
        AvailableBalance -= amount;
        LockedBalance += amount;
        Version++;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new WalletBalanceChangedEvent(Id, TenantId, entry.Id, -amount, AvailableBalance, Currency, DateTime.UtcNow));
        return entry;
    }

    /// <summary>Release locked funds back to available (e.g., withdrawal cancelled/rejected).</summary>
    public WalletLedgerEntry UnlockFunds(
        decimal amount,
        Guid? walletTransactionId = null,
        string? reason = null,
        string? referenceType = null,
        string? referenceId = null,
        Guid? createdByUserId = null)
    {
        EnsureCanMutate();
        if (amount <= 0) throw new ArgumentException("Unlock amount must be positive", nameof(amount));
        if (amount > LockedBalance)
            throw new InvalidOperationException($"Insufficient locked balance for unlock. Locked: {LockedBalance} {Currency}, Requested: {amount} {Currency}");

        var entry = new WalletLedgerEntry(
            Id, TenantId, LedgerEntryType.WithdrawalRelease,
            LedgerDirection.Unlock, amount, Currency,
            AvailableBalance, AvailableBalance + amount,
            LockedBalance, LockedBalance - amount,
            PendingBalance, PendingBalance,
            reason, referenceType, referenceId, createdByUserId)
        { WalletTransactionId = walletTransactionId };

        _ledgerEntries.Add(entry);
        AvailableBalance += amount;
        LockedBalance -= amount;
        Version++;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new WalletBalanceChangedEvent(Id, TenantId, entry.Id, amount, AvailableBalance, Currency, DateTime.UtcNow));
        return entry;
    }

    /// <summary>Capture (permanently debit) locked funds for completed withdrawal.</summary>
    public WalletLedgerEntry CaptureLockedFunds(
        decimal amount,
        Guid? walletTransactionId = null,
        string? reason = null,
        string? referenceType = null,
        string? referenceId = null,
        Guid? createdByUserId = null)
    {
        EnsureCanMutate();
        if (amount <= 0) throw new ArgumentException("Capture amount must be positive", nameof(amount));
        if (amount > LockedBalance)
            throw new InvalidOperationException($"Insufficient locked balance for capture. Locked: {LockedBalance} {Currency}, Requested: {amount} {Currency}");

        var entry = new WalletLedgerEntry(
            Id, TenantId, LedgerEntryType.WithdrawalCapture,
            LedgerDirection.Debit, amount, Currency,
            AvailableBalance, AvailableBalance,
            LockedBalance, LockedBalance - amount,
            PendingBalance, PendingBalance,
            reason, referenceType, referenceId, createdByUserId)
        { WalletTransactionId = walletTransactionId };

        _ledgerEntries.Add(entry);
        LockedBalance -= amount;
        Version++;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new WalletBalanceChangedEvent(Id, TenantId, entry.Id, 0, AvailableBalance, Currency, DateTime.UtcNow));
        return entry;
    }

    public void SetStatus(WalletAccountStatus newStatus)
    {
        if (Status == WalletAccountStatus.Closed)
            throw new InvalidOperationException("Cannot change status of closed wallet");

        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;
    }

    private void EnsureCanMutate()
    {
        if (Status == WalletAccountStatus.Closed)
            throw new InvalidOperationException("Cannot mutate closed wallet");
        if (Status == WalletAccountStatus.Disabled)
            throw new InvalidOperationException("Cannot mutate disabled wallet");
        if (Status == WalletAccountStatus.Locked)
            throw new InvalidOperationException("Cannot mutate locked wallet");
    }

    public decimal TotalBalance => AvailableBalance + LockedBalance + PendingBalance;
}
