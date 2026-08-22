using Domain.Entities;

namespace Domain.Repositories;

public interface IGatewayRoutingPolicyRepository
{
    Task<GatewayRoutingPolicy?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<GatewayRoutingPolicy?> GetByIdWithRulesAsync(Guid id, CancellationToken ct = default);
    Task<GatewayRoutingPolicy?> GetActiveForScopeAsync(Guid? tenantId, string? applicationCode, CancellationToken ct = default);
    Task<List<GatewayRoutingPolicy>> GetAllAsync(Guid? tenantId = null, string? applicationCode = null, CancellationToken ct = default);
    Task AddAsync(GatewayRoutingPolicy policy, CancellationToken ct = default);
    Task UpdateAsync(GatewayRoutingPolicy policy, CancellationToken ct = default);
}
