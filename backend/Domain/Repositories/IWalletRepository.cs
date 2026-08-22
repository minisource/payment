using Domain.Entities;

namespace Domain.Repositories;

/// <summary>
/// Repository interface for Wallet aggregate.
/// </summary>
public interface IWalletRepository
{
    /// <summary>
    /// Gets a wallet by ID.
    /// </summary>
    Task<Wallet?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a wallet by user ID.
    /// </summary>
    Task<Wallet?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a wallet by user ID with transactions loaded.
    /// </summary>
    Task<Wallet?> GetByUserIdWithTransactionsAsync(
        string userId,
        int? transactionLimit = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new wallet.
    /// </summary>
    Task AddAsync(Wallet wallet, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing wallet.
    /// </summary>
    Task UpdateAsync(Wallet wallet, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates just the balance for a wallet.
    /// </summary>
    Task UpdateBalanceAsync(Guid walletId, decimal balance, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a wallet exists for user.
    /// </summary>
    Task<bool> ExistsAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculates balance from transactions (for verification).
    /// </summary>
    Task<decimal> CalculateBalanceFromTransactionsAsync(
        string userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets wallet with locking for update (pessimistic lock).
    /// </summary>
    Task<Wallet?> GetByUserIdForUpdateAsync(string userId, CancellationToken cancellationToken = default);
}
