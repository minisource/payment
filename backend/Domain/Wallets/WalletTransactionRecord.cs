using Domain.Enums;
using Minisource.Common.Domain;

namespace Domain.Entities;

/// <summary>
/// Business-level wallet transaction that groups ledger entries.
/// Represents a single financial operation (admin credit, debit, transfer, etc.).
/// </summary>
public class WalletTransactionRecord : AggregateRoot<Guid>
{
    public Guid TenantId { get; private set; }
    public string TransactionType { get; private set; } = string.Empty;
    public WalletTransactionStatus Status { get; private set; } = WalletTransactionStatus.Pending;
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public Guid? SourceWalletId { get; private set; }
    public Guid? DestinationWalletId { get; private set; }
    public string? ReferenceType { get; private set; }
    public string? ReferenceId { get; private set; }
    public string? IdempotencyKey { get; private set; }
    public string? Description { get; private set; }
    public string? Reason { get; private set; }
    public Guid? CreatedByUserId { get; private set; }
    public string? CreatedByClientId { get; private set; }
    public string? Metadata { get; private set; }
    public DateTime? PostedAt { get; private set; }
    public DateTime? ReversedAt { get; private set; }
    public DateTime? FailedAt { get; private set; }

    private readonly List<WalletLedgerEntry> _ledgerEntries = [];
    public IReadOnlyCollection<WalletLedgerEntry> LedgerEntries => _ledgerEntries.AsReadOnly();

    private WalletTransactionRecord() { }

    public static WalletTransactionRecord Create(
        Guid tenantId,
        string transactionType,
        decimal amount,
        string currency,
        string? idempotencyKey = null,
        string? description = null,
        string? reason = null,
        Guid? createdByUserId = null,
        string? referenceType = null,
        string? referenceId = null,
        Guid? sourceWalletId = null,
        Guid? destinationWalletId = null)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId is required", nameof(tenantId));
        if (string.IsNullOrWhiteSpace(transactionType)) throw new ArgumentException("TransactionType is required", nameof(transactionType));
        if (amount <= 0) throw new ArgumentException("Amount must be positive", nameof(amount));
        if (string.IsNullOrWhiteSpace(currency)) throw new ArgumentException("Currency is required", nameof(currency));

        return new WalletTransactionRecord
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            TransactionType = transactionType,
            Amount = amount,
            Currency = currency.ToUpperInvariant(),
            IdempotencyKey = idempotencyKey,
            Description = description,
            Reason = reason,
            CreatedByUserId = createdByUserId,
            ReferenceType = referenceType,
            ReferenceId = referenceId,
            SourceWalletId = sourceWalletId,
            DestinationWalletId = destinationWalletId,
            Status = WalletTransactionStatus.Pending
        };
    }

    public void Post()
    {
        if (Status != WalletTransactionStatus.Pending)
            throw new InvalidOperationException($"Cannot post transaction in {Status} status");
        Status = WalletTransactionStatus.Posted;
        PostedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Fail(string? reason = null)
    {
        if (Status == WalletTransactionStatus.Posted)
            throw new InvalidOperationException("Cannot fail posted transaction");
        Status = WalletTransactionStatus.Failed;
        FailedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        if (reason != null) Reason = (Reason ?? "") + "; " + reason;
    }

    public void Reverse(string reason)
    {
        if (Status != WalletTransactionStatus.Posted)
            throw new InvalidOperationException($"Cannot reverse transaction in {Status} status");
        Status = WalletTransactionStatus.Reversed;
        ReversedAt = DateTime.UtcNow;
        Reason = string.IsNullOrEmpty(Reason) ? reason : Reason + "; " + reason;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddLedgerEntry(WalletLedgerEntry entry)
    {
        _ledgerEntries.Add(entry);
    }
}
