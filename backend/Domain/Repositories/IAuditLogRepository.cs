using Domain.Entities;

namespace Domain.Repositories;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog log, CancellationToken ct = default);
    Task<List<AuditLog>> GetByEntityAsync(string entityType, string entityId, int skip = 0, int take = 50, CancellationToken ct = default);
}
