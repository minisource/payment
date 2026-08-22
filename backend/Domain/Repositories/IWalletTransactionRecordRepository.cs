using Domain.Entities;

namespace Domain.Repositories;

public interface IWalletTransactionRecordRepository
{
    Task<WalletTransactionRecord?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<WalletTransactionRecord?> GetByIdempotencyKeyAsync(Guid tenantId, string idempotencyKey, CancellationToken ct = default);
    Task AddAsync(WalletTransactionRecord record, CancellationToken ct = default);
    Task UpdateAsync(WalletTransactionRecord record, CancellationToken ct = default);
    Task<List<WalletTransactionRecord>> GetByTenantAsync(Guid tenantId, int skip = 0, int take = 50, string? transactionType = null, Guid? walletId = null, CancellationToken ct = default);
    Task<int> CountByTenantAsync(Guid tenantId, string? transactionType = null, Guid? walletId = null, CancellationToken ct = default);
}
