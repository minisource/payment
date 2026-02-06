using Domain.Entities;
using Domain.Enums;

namespace Domain.Repositories;

/// <summary>
/// Repository interface for WalletTransaction entity.
/// </summary>
public interface IWalletTransactionRepository
{
    /// <summary>
    /// Adds a new transaction.
    /// </summary>
    Task AddAsync(WalletTransaction transaction, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets transactions by wallet ID with pagination.
    /// </summary>
    Task<List<WalletTransaction>> GetByWalletIdAsync(
        Guid walletId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets transactions by wallet ID and type.
    /// </summary>
    Task<List<WalletTransaction>> GetByWalletIdAndTypeAsync(
        Guid walletId,
        WalletTransactionType type,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets transactions by reference.
    /// </summary>
    Task<List<WalletTransaction>> GetByReferenceAsync(
        string referenceId,
        string referenceType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets total count for pagination.
    /// </summary>
    Task<int> GetTotalCountAsync(Guid walletId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets total count for a specific wallet.
    /// </summary>
    Task<int> GetTotalCountByWalletIdAsync(Guid walletId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets sum of all credits for a wallet.
    /// </summary>
    Task<decimal> GetTotalCreditsAsync(Guid walletId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets sum of all debits for a wallet.
    /// </summary>
    Task<decimal> GetTotalDebitsAsync(Guid walletId, CancellationToken cancellationToken = default);
}
