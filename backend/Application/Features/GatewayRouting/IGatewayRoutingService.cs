using Application.DTOs;

namespace Application.Features.GatewayRouting;

/// <summary>
/// Service for managing gateway routing policies and rules.
/// </summary>
public interface IGatewayRoutingService
{
    Task<GatewaySelectionResult> SelectGatewayAsync(GatewaySelectionRequest request, CancellationToken ct);
    Task<GatewayRoutingPolicyDto> CreatePolicyAsync(CreateRoutingPolicyRequest request, CancellationToken ct);
    Task<GatewayRoutingPolicyDto> UpdatePolicyAsync(Guid policyId, CreateRoutingPolicyRequest request, CancellationToken ct);
    Task DeletePolicyAsync(Guid policyId, CancellationToken ct);
    Task<GatewayRoutingPolicyDto?> GetPolicyAsync(Guid? tenantId, string? applicationCode, CancellationToken ct);
    Task<List<GatewayRoutingPolicyDto>> GetAllPoliciesAsync(Guid? tenantId, string? applicationCode, CancellationToken ct);
    Task<GatewayRoutingRuleDto> AddRuleAsync(Guid policyId, CreateRoutingRuleRequest request, CancellationToken ct);
    Task UpdateRuleAsync(Guid ruleId, UpdateRoutingRuleRequest request, CancellationToken ct);
    Task DeleteRuleAsync(Guid ruleId, CancellationToken ct);
    Task<List<GatewayRoutingRuleDto>> GetRulesAsync(Guid policyId, CancellationToken ct);
}
