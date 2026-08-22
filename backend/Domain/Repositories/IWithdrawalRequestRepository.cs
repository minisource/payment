using Domain.Entities;

namespace Domain.Repositories;

public interface IWithdrawalRequestRepository
{
    Task<WithdrawalRequest?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<WithdrawalRequest?> GetByIdempotencyKeyAsync(Guid tenantId, string idempotencyKey, CancellationToken ct = default);
    Task<List<WithdrawalRequest>> GetByOwnerAsync(Guid tenantId, string ownerType, Guid ownerId, int skip = 0, int take = 50, CancellationToken ct = default);
    Task<List<WithdrawalRequest>> GetByTenantAsync(Guid tenantId, WithdrawalStatus? status = null, int skip = 0, int take = 50, CancellationToken ct = default);
    Task<List<WithdrawalRequest>> GetByWalletAsync(Guid walletId, int skip = 0, int take = 50, CancellationToken ct = default);
    Task<int> CountByTenantAsync(Guid tenantId, WithdrawalStatus? status = null, CancellationToken ct = default);
    Task AddAsync(WithdrawalRequest request, CancellationToken ct = default);
    Task UpdateAsync(WithdrawalRequest request, CancellationToken ct = default);
}
