using Domain.Entities;
using Domain.Enums;

namespace Domain.Repositories;

public interface IWalletAccountRepository
{
    Task<WalletAccount?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<WalletAccount?> GetByOwnerAsync(Guid tenantId, string ownerType, Guid ownerId, string currency, CancellationToken ct = default);
    Task<WalletAccount?> GetByIdForUpdateAsync(Guid id, CancellationToken ct = default);
    Task<List<WalletAccount>> GetByTenantAsync(Guid tenantId, WalletAccountStatus? status = null, int skip = 0, int take = 50, CancellationToken ct = default);
    Task<List<WalletAccount>> GetByOwnerAsync(Guid tenantId, string ownerType, Guid ownerId, CancellationToken ct = default);
    Task AddAsync(WalletAccount account, CancellationToken ct = default);
    Task UpdateAsync(WalletAccount account, CancellationToken ct = default);
    Task<int> CountByTenantAsync(Guid tenantId, CancellationToken ct = default);
    Task<Dictionary<string, (decimal available, decimal locked, decimal pending, int count)>> GetTenantLiabilityAsync(Guid tenantId, CancellationToken ct = default);
}
