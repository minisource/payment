using Domain.Entities;

namespace Domain.Repositories;

public interface IOutboxEventRepository
{
    Task AddAsync(OutboxEvent evt, CancellationToken ct = default);
    Task<List<OutboxEvent>> GetPendingAsync(int take = 100, CancellationToken ct = default);
    Task MarkAsPendingForRetryAsync(Guid id, CancellationToken ct = default);
    Task<List<OutboxEvent>> GetDispatchBatchAsync(int batchSize, string lockedBy, TimeSpan lockDuration, CancellationToken ct = default);
    Task MarkAsProcessedAsync(Guid id, CancellationToken ct = default);
    Task MarkAsFailedAsync(Guid id, string? errorMessage, DateTime? nextRetryAt, CancellationToken ct = default);
    Task MarkAsDeadLetteredAsync(Guid id, string? errorMessage, CancellationToken ct = default);
    Task MarkAsSkippedAsync(Guid id, string reason, CancellationToken ct = default);
    Task<OutboxEvent?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<OutboxEvent>> ListAsync(Guid? tenantId, string? applicationCode, string? eventType, string? status, string? aggregateType, string? aggregateId, DateTime? dateFrom, DateTime? dateTo, string? query, int skip, int take, CancellationToken ct = default);
    Task<int> CountAsync(Guid? tenantId, string? applicationCode, string? eventType, string? status, string? aggregateType, string? aggregateId, DateTime? dateFrom, DateTime? dateTo, string? query, CancellationToken ct = default);
    Task<List<OutboxEvent>> GetByStatusAsync(string status, int take, CancellationToken ct = default);
}
