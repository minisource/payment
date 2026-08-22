using Domain.Entities;

namespace Domain.Repositories;

public interface IPayoutAccountRepository
{
    Task<PayoutAccount?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<PayoutAccount>> GetByOwnerAsync(Guid tenantId, string ownerType, Guid ownerId, int skip = 0, int take = 50, CancellationToken ct = default);
    Task<List<PayoutAccount>> GetByTenantAsync(Guid tenantId, PayoutAccountStatus? status = null, int skip = 0, int take = 50, CancellationToken ct = default);
    Task<PayoutAccount?> GetByIbanHashAsync(Guid tenantId, string ibanHash, CancellationToken ct = default);
    Task<PayoutAccount?> GetByCardHashAsync(Guid tenantId, string cardHash, CancellationToken ct = default);
    Task AddAsync(PayoutAccount account, CancellationToken ct = default);
    Task UpdateAsync(PayoutAccount account, CancellationToken ct = default);
}
