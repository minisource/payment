using Domain.Entities;
using Domain.Enums;

namespace Domain.Repositories;

/// <summary>
/// Repository for wallet hold persistence.
/// </summary>
public interface IWalletHoldRepository
{
    Task<WalletHold?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<WalletHold?> GetByIdForUpdateAsync(Guid id, CancellationToken ct = default);
    Task<WalletHold?> GetActiveByReferenceAsync(string referenceType, Guid referenceId, CancellationToken ct = default);
    Task<List<WalletHold>> GetByWalletAccountAsync(Guid walletAccountId, WalletHoldStatus? status = null, CancellationToken ct = default);
    Task<decimal> GetActiveHoldSumAsync(Guid walletAccountId, CancellationToken ct = default);
    Task AddAsync(WalletHold hold, CancellationToken ct = default);
    Task UpdateAsync(WalletHold hold, CancellationToken ct = default);
}
