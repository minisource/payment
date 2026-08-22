using Domain.Enums;
using Domain.Events;
using Domain.ValueObjects;
using Minisource.Common.Domain;
using Minisource.Common.Exceptions;

namespace Domain.Entities;

/// <summary>
/// Wallet aggregate root. Manages user balance and transactions.
/// Balance is calculated from transactions sum for consistency.
/// </summary>
public class Wallet : AggregateRoot<Guid>
{
    /// <summary>
    /// User ID who owns the wallet.
    /// </summary>
    public string UserId { get; private set; } = string.Empty;

    /// <summary>
    /// Cached balance for quick access (recalculated from transactions).
    /// </summary>
    public decimal Balance { get; private set; }

    /// <summary>
    /// Currency code for the wallet.
    /// </summary>
    public string Currency { get; private set; } = "IRR";

    /// <summary>
    /// Whether the wallet is active.
    /// </summary>
    public bool IsActive { get; private set; } = true;

    // Navigation properties
    private readonly List<WalletTransaction> _transactions = [];
    public IReadOnlyCollection<WalletTransaction> Transactions => _transactions.AsReadOnly();

    // EF Core constructor
    private Wallet() { }

    /// <summary>
    /// Creates a new wallet for a user.
    /// </summary>
    public static Wallet Create(string userId, string currency = "IRR")
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID is required", nameof(userId));

        var wallet = new Wallet
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Balance = 0,
            Currency = currency.ToUpperInvariant(),
            IsActive = true
        };

        wallet.RaiseDomainEvent(new WalletCreatedEvent(wallet.Id, userId, wallet.CreatedAt));

        return wallet;
    }

    /// <summary>
    /// Credits the wallet (adds funds).
    /// </summary>
    public WalletTransaction Credit(
        decimal amount,
        string description,
        string? referenceId = null,
        string? referenceType = null)
    {
        EnsureActive();

        if (amount <= 0)
            throw new ArgumentException("Credit amount must be positive", nameof(amount));

        var transaction = WalletTransaction.CreateCredit(
            Id,
            amount,
            description,
            referenceId,
            referenceType);

        _transactions.Add(transaction);

        var oldBalance = Balance;
        Balance += amount;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new WalletCreditedEvent(
            Id,
            UserId,
            transaction.Id,
            amount,
            Balance,
            description,
            DateTime.UtcNow));

        return transaction;
    }

    /// <summary>
    /// Debits the wallet (removes funds).
    /// </summary>
    public WalletTransaction Debit(
        decimal amount,
        string description,
        string? referenceId = null,
        string? referenceType = null)
    {
        EnsureActive();

        if (amount <= 0)
            throw new ArgumentException("Debit amount must be positive", nameof(amount));

        if (amount > Balance)
        {
            RaiseDomainEvent(new InsufficientFundsEvent(
                Id,
                UserId,
                amount,
                Balance,
                DateTime.UtcNow));

            throw new WalletException(
                "Insufficient funds",
                $"Requested: {amount} {Currency}, Available: {Balance} {Currency}");
        }

        var transaction = WalletTransaction.CreateDebit(
            Id,
            amount,
            description,
            referenceId,
            referenceType);

        _transactions.Add(transaction);

        var oldBalance = Balance;
        Balance -= amount;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new WalletDebitedEvent(
            Id,
            UserId,
            transaction.Id,
            amount,
            Balance,
            description,
            DateTime.UtcNow));

        return transaction;
    }

    /// <summary>
    /// Recalculates balance from all transactions.
    /// Use this to ensure data consistency.
    /// </summary>
    public void RecalculateBalance()
    {
        var oldBalance = Balance;
        var calculatedBalance = _transactions
            .Where(t => !t.IsReversed)
            .Sum(t => t.Type == WalletTransactionType.Credit ? t.Amount : -t.Amount);

        if (calculatedBalance != Balance)
        {
            Balance = calculatedBalance;
            UpdatedAt = DateTime.UtcNow;

            RaiseDomainEvent(new WalletBalanceRecalculatedEvent(
                Id,
                oldBalance,
                Balance,
                DateTime.UtcNow));
        }
    }

    /// <summary>
    /// Checks if wallet has sufficient funds.
    /// </summary>
    public bool HasSufficientFunds(decimal amount) => Balance >= amount;

    /// <summary>
    /// Gets available balance as Money value object.
    /// </summary>
    public Money GetBalance() => Money.Create(Balance, Currency);

    /// <summary>
    /// Deactivates the wallet.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Reactivates the wallet.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    private void EnsureActive()
    {
        if (!IsActive)
            throw new WalletException("Wallet is inactive", "Cannot perform operations on an inactive wallet");
    }
}
