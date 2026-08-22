using Domain.Entities;

namespace Domain.Repositories;

public interface IGatewayRoutingRuleRepository
{
    Task<GatewayRoutingRule?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<GatewayRoutingRule>> GetByPolicyIdAsync(Guid policyId, CancellationToken ct = default);
    Task AddAsync(GatewayRoutingRule rule, CancellationToken ct = default);
    Task UpdateAsync(GatewayRoutingRule rule, CancellationToken ct = default);
}
