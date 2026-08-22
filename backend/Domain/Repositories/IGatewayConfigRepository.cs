using Domain.Entities;

namespace Domain.Repositories;

public interface IGatewayConfigRepository
{
    Task<GatewayConfig?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<GatewayConfig>> GetActiveAsync(Guid? tenantId = null, string? applicationCode = null, CancellationToken ct = default);
    Task<List<GatewayConfig>> GetAllForAdminAsync(Guid? tenantId = null, string? applicationCode = null, int skip = 0, int take = 50, CancellationToken ct = default);
    Task AddAsync(GatewayConfig config, CancellationToken ct = default);
    Task UpdateAsync(GatewayConfig config, CancellationToken ct = default);
    Task<int> CountAsync(Guid? tenantId = null, string? applicationCode = null, CancellationToken ct = default);
}
