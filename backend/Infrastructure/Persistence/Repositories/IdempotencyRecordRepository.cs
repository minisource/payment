using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class IdempotencyRecordRepository : IIdempotencyRecordRepository
{
    private readonly PaymentDbContext _ctx;
    public IdempotencyRecordRepository(PaymentDbContext ctx) => _ctx = ctx;

    public async Task<IdempotencyRecord?> GetByKeyAsync(Guid tenantId, string idempotencyKey, CancellationToken ct = default)
        => await _ctx.IdempotencyRecords.FirstOrDefaultAsync(r => r.TenantId == tenantId && r.IdempotencyKey == idempotencyKey, ct);

    public async Task AddAsync(IdempotencyRecord record, CancellationToken ct = default)
        => await _ctx.IdempotencyRecords.AddAsync(record, ct);

    public Task UpdateAsync(IdempotencyRecord record, CancellationToken ct = default)
    { _ctx.IdempotencyRecords.Update(record); return Task.CompletedTask; }
}
