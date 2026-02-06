using Application.DTOs;

namespace Application.Services;

/// <summary>
/// Wallet service interface for wallet operations.
/// </summary>
public interface IWalletService
{
    /// <summary>
    /// Gets or creates a wallet for the specified user.
    /// </summary>
    Task<WalletDto> GetOrCreateAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current balance for a user.
    /// </summary>
    Task<WalletDto> GetBalanceAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Credits the user's wallet with the specified amount.
    /// </summary>
    Task<WalletDto> CreditAsync(
        string userId,
        decimal amount,
        string description,
        string? referenceId = null,
        string? referenceType = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Debits the user's wallet with the specified amount.
    /// </summary>
    Task<WalletDto> DebitAsync(
        string userId,
        decimal amount,
        string description,
        string? referenceId = null,
        string? referenceType = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Applies available credit from wallet towards a payment.
    /// Returns the amount applied and the remaining amount.
    /// </summary>
    Task<(decimal Applied, decimal Remaining)> ApplyCreditAsync(
        string userId,
        decimal amount,
        string description,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets paginated wallet transactions for a user.
    /// </summary>
    Task<PagedResult<WalletTransactionDto>> GetTransactionsAsync(
        string userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Recalculates wallet balance from transaction history.
    /// </summary>
    Task<WalletDto> RecalculateBalanceAsync(string userId, CancellationToken cancellationToken = default);
}
