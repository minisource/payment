namespace Domain.Enums;

/// <summary>
/// Wallet transaction type enum.
/// </summary>
public enum WalletTransactionType
{
    /// <summary>
    /// Credit (add funds to wallet).
    /// </summary>
    Credit = 1,

    /// <summary>
    /// Debit (remove funds from wallet).
    /// </summary>
    Debit = 2,

    /// <summary>
    /// Reversal (undo a previous transaction).
    /// </summary>
    Reversal = 3,

    /// <summary>
    /// Refund (return funds from payment).
    /// </summary>
    Refund = 4,

    /// <summary>
    /// Bonus credit (promotional).
    /// </summary>
    Bonus = 5,

    /// <summary>
    /// Payment usage (credit applied to payment).
    /// </summary>
    PaymentUsage = 6
}
