using Domain.Enums;
using Minisource.Common.Domain;

namespace Domain.Entities;

/// <summary>
/// Represents a wallet transaction (credit or debit).
/// Immutable once created - use reversal for corrections.
/// </summary>
public class WalletTransaction : Entity<Guid>
{
    /// <summary>
    /// Wallet ID this transaction belongs to.
    /// </summary>
    public Guid WalletId { get; private set; }

    /// <summary>
    /// Transaction amount (always positive).
    /// </summary>
    public decimal Amount { get; private set; }

    /// <summary>
    /// Transaction type (credit or debit).
    /// </summary>
    public WalletTransactionType Type { get; private set; }

    /// <summary>
    /// Description of the transaction.
    /// </summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// External reference ID (e.g., payment ID).
    /// </summary>
    public string? ReferenceId { get; private set; }

    /// <summary>
    /// Type of reference (e.g., "Payment", "Refund").
    /// </summary>
    public string? ReferenceType { get; private set; }

    /// <summary>
    /// Balance after this transaction.
    /// </summary>
    public decimal BalanceAfter { get; private set; }

    /// <summary>
    /// Whether this transaction has been reversed.
    /// </summary>
    public bool IsReversed { get; private set; }

    /// <summary>
    /// ID of the reversal transaction if reversed.
    /// </summary>
    public Guid? ReversalTransactionId { get; private set; }

    /// <summary>
    /// Reason for reversal if reversed.
    /// </summary>
    public string? ReversalReason { get; private set; }

    /// <summary>
    /// Timestamp of reversal if reversed.
    /// </summary>
    public DateTime? ReversedAt { get; private set; }

    // Navigation
    public Wallet Wallet { get; private set; } = null!;

    // EF Core constructor
    private WalletTransaction() { }

    /// <summary>
    /// Creates a credit transaction.
    /// </summary>
    internal static WalletTransaction CreateCredit(
        Guid walletId,
        decimal amount,
        string description,
        string? referenceId = null,
        string? referenceType = null)
    {
        return new WalletTransaction
        {
            Id = Guid.NewGuid(),
            WalletId = walletId,
            Amount = amount,
            Type = WalletTransactionType.Credit,
            Description = description,
            ReferenceId = referenceId,
            ReferenceType = referenceType,
            IsReversed = false
        };
    }

    /// <summary>
    /// Creates a debit transaction.
    /// </summary>
    internal static WalletTransaction CreateDebit(
        Guid walletId,
        decimal amount,
        string description,
        string? referenceId = null,
        string? referenceType = null)
    {
        return new WalletTransaction
        {
            Id = Guid.NewGuid(),
            WalletId = walletId,
            Amount = amount,
            Type = WalletTransactionType.Debit,
            Description = description,
            ReferenceId = referenceId,
            ReferenceType = referenceType,
            IsReversed = false
        };
    }

    /// <summary>
    /// Marks this transaction as reversed.
    /// </summary>
    internal void MarkAsReversed(Guid reversalTransactionId, string reason)
    {
        if (IsReversed)
            throw new InvalidOperationException("Transaction is already reversed");

        IsReversed = true;
        ReversalTransactionId = reversalTransactionId;
        ReversalReason = reason;
        ReversedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Sets the balance after this transaction.
    /// </summary>
    internal void SetBalanceAfter(decimal balance)
    {
        BalanceAfter = balance;
    }

    /// <summary>
    /// Gets the effective amount (positive for credit, negative for debit).
    /// </summary>
    public decimal GetEffectiveAmount()
    {
        if (IsReversed) return 0;
        return Type == WalletTransactionType.Credit ? Amount : -Amount;
    }
}
