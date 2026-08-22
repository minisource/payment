using Domain.Entities;

namespace Domain.Repositories;

public interface IIdempotencyRecordRepository
{
    Task<IdempotencyRecord?> GetByKeyAsync(Guid tenantId, string idempotencyKey, CancellationToken ct = default);
    Task AddAsync(IdempotencyRecord record, CancellationToken ct = default);
    Task UpdateAsync(IdempotencyRecord record, CancellationToken ct = default);
}
