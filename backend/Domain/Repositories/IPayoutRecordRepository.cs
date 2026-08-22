using Domain.Entities;

namespace Domain.Repositories;

public interface IPayoutRecordRepository
{
    Task<PayoutRecord?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PayoutRecord?> GetByWithdrawalIdAsync(Guid withdrawalRequestId, CancellationToken ct = default);
    Task<List<PayoutRecord>> GetByTenantAsync(Guid tenantId, PayoutRecordStatus? status = null, int skip = 0, int take = 50, CancellationToken ct = default);
    Task AddAsync(PayoutRecord record, CancellationToken ct = default);
    Task UpdateAsync(PayoutRecord record, CancellationToken ct = default);
}
