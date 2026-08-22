using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class OutboxEventRepository : IOutboxEventRepository
{
    private readonly PaymentDbContext _ctx;
    public OutboxEventRepository(PaymentDbContext ctx) => _ctx = ctx;

    public async Task AddAsync(OutboxEvent evt, CancellationToken ct = default)
        => await _ctx.OutboxEvents.AddAsync(evt, ct);

    public async Task<List<OutboxEvent>> GetPendingAsync(int take = 100, CancellationToken ct = default)
        => await _ctx.OutboxEvents
            .Where(e => e.Status == "pending")
            .OrderBy(e => e.OccurredAt)
            .Take(take).ToListAsync(ct);

    public async Task<List<OutboxEvent>> GetDispatchBatchAsync(int batchSize, string lockedBy, TimeSpan lockDuration, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var lockUntil = now + lockDuration;

        // Use explicit transaction to hold UPDLOCK from SELECT through UPDATE.
        // Without a transaction, the lock hint only applies for the duration of the SELECT,
        // creating a race condition where another dispatcher could grab the same rows.
        await using var tx = await _ctx.Database.BeginTransactionAsync(ct);

        var sqlSelect = @"
            SELECT Id FROM [OutboxEvents] WITH (UPDLOCK, READPAST, ROWLOCK)
            WHERE (Status = 'pending' OR Status = 'failed')
              AND (LockedUntil IS NULL OR LockedUntil <= {0})
              AND (NextRetryAt IS NULL OR NextRetryAt <= {0})
              AND AvailableAt <= {0}
            ORDER BY OccurredAt
            OFFSET 0 ROWS FETCH NEXT {1} ROWS ONLY";

        var ids = await _ctx.Database.SqlQueryRaw<Guid>(sqlSelect, now, batchSize)
            .ToListAsync(ct);

        if (ids.Count == 0)
        {
            await tx.RollbackAsync(ct);
            return new List<OutboxEvent>();
        }

        // Update locked events (runs within the transaction — UPDLOCK from SELECT is still held)
        await _ctx.OutboxEvents
            .Where(e => ids.Contains(e.Id))
            .ExecuteUpdateAsync(s => s
                .SetProperty(e => e.Status, "processing")
                .SetProperty(e => e.LockedUntil, lockUntil)
                .SetProperty(e => e.LockedBy, lockedBy), ct);

        await tx.CommitAsync(ct);

        // Load full entities after commit (locks already released, safe to read)
        return await _ctx.OutboxEvents
            .Where(e => ids.Contains(e.Id))
            .ToListAsync(ct);
    }

    public async Task MarkAsProcessedAsync(Guid id, CancellationToken ct = default)
        => await _ctx.OutboxEvents.Where(e => e.Id == id).ExecuteUpdateAsync(s => s
            .SetProperty(e => e.Status, "processed")
            .SetProperty(e => e.ProcessedAt, DateTime.UtcNow)
            .SetProperty(e => e.LockedUntil, (DateTime?)null)
            .SetProperty(e => e.LockedBy, (string?)null), ct);

    public async Task MarkAsFailedAsync(Guid id, string? errorMessage, DateTime? nextRetryAt, CancellationToken ct = default)
        => await _ctx.OutboxEvents.Where(e => e.Id == id).ExecuteUpdateAsync(s => s
            .SetProperty(e => e.Status, "failed")
            .SetProperty(e => e.ErrorMessage, errorMessage)
            .SetProperty(e => e.LastErrorAt, DateTime.UtcNow)
            .SetProperty(e => e.NextRetryAt, nextRetryAt)
            .SetProperty(e => e.RetryCount, e => e.RetryCount + 1)
            .SetProperty(e => e.LockedUntil, (DateTime?)null)
            .SetProperty(e => e.LockedBy, (string?)null), ct);

    public async Task MarkAsDeadLetteredAsync(Guid id, string? errorMessage, CancellationToken ct = default)
        => await _ctx.OutboxEvents.Where(e => e.Id == id).ExecuteUpdateAsync(s => s
            .SetProperty(e => e.Status, "dead_lettered")
            .SetProperty(e => e.ErrorMessage, errorMessage)
            .SetProperty(e => e.DeadLetteredAt, DateTime.UtcNow)
            .SetProperty(e => e.LockedUntil, (DateTime?)null)
            .SetProperty(e => e.LockedBy, (string?)null), ct);

    public async Task MarkAsSkippedAsync(Guid id, string reason, CancellationToken ct = default)
        => await _ctx.OutboxEvents.Where(e => e.Id == id).ExecuteUpdateAsync(s => s
            .SetProperty(e => e.Status, "skipped")
            .SetProperty(e => e.ErrorMessage, reason)
            .SetProperty(e => e.LockedUntil, (DateTime?)null)
            .SetProperty(e => e.LockedBy, (string?)null), ct);

    public async Task MarkAsPendingForRetryAsync(Guid id, CancellationToken ct = default)
        => await _ctx.OutboxEvents.Where(e => e.Id == id).ExecuteUpdateAsync(s => s
            .SetProperty(e => e.Status, "pending")
            .SetProperty(e => e.RetryCount, 0)
            .SetProperty(e => e.NextRetryAt, DateTime.UtcNow)
            .SetProperty(e => e.ErrorMessage, (string?)null)
            .SetProperty(e => e.DeadLetteredAt, (DateTime?)null)
            .SetProperty(e => e.LockedUntil, (DateTime?)null)
            .SetProperty(e => e.LockedBy, (string?)null), ct);

    public async Task<OutboxEvent?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _ctx.OutboxEvents.FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<List<OutboxEvent>> ListAsync(Guid? tenantId, string? applicationCode, string? eventType,
        string? status, string? aggregateType, string? aggregateId, DateTime? dateFrom, DateTime? dateTo,
        string? query, int skip, int take, CancellationToken ct = default)
    {
        var q = _ctx.OutboxEvents.AsQueryable();
        if (tenantId.HasValue) q = q.Where(e => e.TenantId == tenantId.Value);
        if (!string.IsNullOrEmpty(applicationCode)) q = q.Where(e => e.ApplicationCode == applicationCode);
        if (!string.IsNullOrEmpty(eventType)) q = q.Where(e => e.EventType == eventType);
        if (!string.IsNullOrEmpty(status)) q = q.Where(e => e.Status == status);
        if (!string.IsNullOrEmpty(aggregateType)) q = q.Where(e => e.AggregateType == aggregateType);
        if (!string.IsNullOrEmpty(aggregateId)) q = q.Where(e => e.AggregateId == aggregateId);
        if (dateFrom.HasValue) q = q.Where(e => e.OccurredAt >= dateFrom.Value);
        if (dateTo.HasValue) q = q.Where(e => e.OccurredAt <= dateTo.Value);
        if (!string.IsNullOrEmpty(query))
            q = q.Where(e => e.EventType.Contains(query) || e.AggregateType.Contains(query) || e.AggregateId.Contains(query));
        return await q.OrderByDescending(e => e.OccurredAt).Skip(skip).Take(take).ToListAsync(ct);
    }

    public async Task<int> CountAsync(Guid? tenantId, string? applicationCode, string? eventType,
        string? status, string? aggregateType, string? aggregateId, DateTime? dateFrom, DateTime? dateTo,
        string? query, CancellationToken ct = default)
    {
        var q = _ctx.OutboxEvents.AsQueryable();
        if (tenantId.HasValue) q = q.Where(e => e.TenantId == tenantId.Value);
        if (!string.IsNullOrEmpty(applicationCode)) q = q.Where(e => e.ApplicationCode == applicationCode);
        if (!string.IsNullOrEmpty(eventType)) q = q.Where(e => e.EventType == eventType);
        if (!string.IsNullOrEmpty(status)) q = q.Where(e => e.Status == status);
        if (!string.IsNullOrEmpty(aggregateType)) q = q.Where(e => e.AggregateType == aggregateType);
        if (!string.IsNullOrEmpty(aggregateId)) q = q.Where(e => e.AggregateId == aggregateId);
        if (dateFrom.HasValue) q = q.Where(e => e.OccurredAt >= dateFrom.Value);
        if (dateTo.HasValue) q = q.Where(e => e.OccurredAt <= dateTo.Value);
        if (!string.IsNullOrEmpty(query))
            q = q.Where(e => e.EventType.Contains(query) || e.AggregateType.Contains(query) || e.AggregateId.Contains(query));
        return await q.CountAsync(ct);
    }

    public async Task<List<OutboxEvent>> GetByStatusAsync(string status, int take, CancellationToken ct = default)
        => await _ctx.OutboxEvents.Where(e => e.Status == status).OrderByDescending(e => e.OccurredAt).Take(take).ToListAsync(ct);
}
