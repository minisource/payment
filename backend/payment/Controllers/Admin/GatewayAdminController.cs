using Application.DTOs;
using Application.Features.Gateways;
using Application.Features.GatewayRouting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Minisource.Common.Exceptions;
using Presentaion.Middlewares;

namespace payment.Controllers;

[Route("api/v1/admin")]
[Authorize(Policy = "WalletAdmin")]
public class GatewayAdminController : PaymentControllerBase
{
    private readonly IGatewayConfigService _configSvc;
    private readonly IGatewayRoutingService _routingSvc;
    private readonly ILogger<GatewayAdminController> _logger;

    public GatewayAdminController(IGatewayConfigService configSvc, IGatewayRoutingService routingSvc,
        ILogger<GatewayAdminController> logger)
    {
        _configSvc = configSvc;
        _routingSvc = routingSvc;
        _logger = logger;
    }

    // ═══ Gateway Providers ═══════════════════════════

    [HttpGet("gateway-providers")]
    public async Task<ActionResult<List<GatewayProviderDto>>> GetProviders(CancellationToken ct)
        => Ok(await _configSvc.GetAllProvidersAsync(ct));

    [HttpGet("gateway-providers/{providerCode}")]
    public async Task<ActionResult<GatewayProviderDto>> GetProvider(string providerCode, CancellationToken ct)
        => Ok(await _configSvc.GetProviderAsync(providerCode, ct) ?? throw new NotFoundException("GatewayProvider", providerCode));

    [HttpPost("gateway-providers")]
    public async Task<ActionResult<GatewayProviderDto>> CreateProvider(
        [FromBody] CreateGatewayProviderRequest request, CancellationToken ct)
        => Ok(await _configSvc.CreateProviderAsync(request, ct));

    [HttpPatch("gateway-providers/{providerCode}")]
    public async Task<ActionResult<GatewayProviderDto>> UpdateProvider(
        string providerCode, [FromBody] UpdateGatewayProviderRequest request, CancellationToken ct)
        => Ok(await _configSvc.UpdateProviderAsync(providerCode, request, ct));

    // ═══ Gateway Configs ═════════════════════════════

    [HttpGet("gateway-configs")]
    public async Task<ActionResult<List<GatewayConfigDto>>> GetConfigs(
        [FromQuery] int skip = 0, [FromQuery] int take = 50, CancellationToken ct = default)
        => Ok(await _configSvc.GetAllConfigsAsync(TenantId == Guid.Empty ? null : TenantId, ApplicationCode, skip, take, ct));

    [HttpPost("gateway-configs")]
    public async Task<ActionResult<GatewayConfigDto>> CreateConfig(
        [FromBody] CreateGatewayConfigRequest request, CancellationToken ct)
        => Ok(await _configSvc.CreateConfigAsync(request, ct));

    [HttpGet("gateway-configs/{configId:guid}")]
    public async Task<ActionResult<GatewayConfigDto>> GetConfig(Guid configId, CancellationToken ct)
        => Ok(await _configSvc.GetConfigAsync(configId, ct));

    [HttpPatch("gateway-configs/{configId:guid}")]
    public async Task<ActionResult<GatewayConfigDto>> UpdateConfig(
        Guid configId, [FromBody] UpdateGatewayConfigRequest request, CancellationToken ct)
        => Ok(await _configSvc.UpdateConfigAsync(configId, request, ct));

    [HttpPost("gateway-configs/{configId:guid}/enable")]
    public async Task<IActionResult> EnableConfig(Guid configId, CancellationToken ct)
    { await _configSvc.EnableConfigAsync(configId, ct); return Ok(); }

    [HttpPost("gateway-configs/{configId:guid}/disable")]
    public async Task<IActionResult> DisableConfig(Guid configId, CancellationToken ct)
    { await _configSvc.DisableConfigAsync(configId, ct); return Ok(); }

    [HttpDelete("gateway-configs/{configId:guid}")]
    public async Task<IActionResult> DeleteConfig(Guid configId, CancellationToken ct)
    { await _configSvc.SoftDeleteConfigAsync(configId, ct); return Ok(); }

    [HttpPost("gateway-configs/{configId:guid}/test")]
    public async Task<ActionResult<GatewayConfigTestResultDto>> TestConfig(Guid configId, CancellationToken ct)
        => Ok(await _configSvc.TestConfigAsync(configId, ct));

    // ═══ Routing Policies ═══════════════════════════

    [HttpGet("gateway-routing-policies")]
    public async Task<ActionResult<List<GatewayRoutingPolicyDto>>> GetPolicies(CancellationToken ct)
        => Ok(await _routingSvc.GetAllPoliciesAsync(TenantId == Guid.Empty ? null : TenantId, ApplicationCode, ct));

    [HttpPost("gateway-routing-policies")]
    public async Task<ActionResult<GatewayRoutingPolicyDto>> CreatePolicy(
        [FromBody] CreateRoutingPolicyRequest request, CancellationToken ct)
        => Ok(await _routingSvc.CreatePolicyAsync(request, ct));

    [HttpGet("gateway-routing-policies/{policyId:guid}")]
    public async Task<ActionResult<GatewayRoutingPolicyDto>> GetPolicy(Guid policyId, CancellationToken ct)
    {
        var rules = await _routingSvc.GetRulesAsync(policyId, ct);
        return Ok(new { PolicyId = policyId, Rules = rules });
    }

    [HttpPatch("gateway-routing-policies/{policyId:guid}")]
    public async Task<ActionResult<GatewayRoutingPolicyDto>> UpdatePolicy(
        Guid policyId, [FromBody] CreateRoutingPolicyRequest request, CancellationToken ct)
        => Ok(await _routingSvc.UpdatePolicyAsync(policyId, request, ct));

    [HttpDelete("gateway-routing-policies/{policyId:guid}")]
    public async Task<IActionResult> DeletePolicy(Guid policyId, CancellationToken ct)
    { await _routingSvc.DeletePolicyAsync(policyId, ct); return Ok(); }

    // ═══ Routing Rules ══════════════════════════════

    [HttpGet("gateway-routing-policies/{policyId:guid}/rules")]
    public async Task<ActionResult<List<GatewayRoutingRuleDto>>> GetRules(Guid policyId, CancellationToken ct)
        => Ok(await _routingSvc.GetRulesAsync(policyId, ct));

    [HttpPost("gateway-routing-policies/{policyId:guid}/rules")]
    public async Task<ActionResult<GatewayRoutingRuleDto>> CreateRule(
        Guid policyId, [FromBody] CreateRoutingRuleRequest request, CancellationToken ct)
        => Ok(await _routingSvc.AddRuleAsync(policyId, request, ct));

    [HttpPatch("gateway-routing-rules/{ruleId:guid}")]
    public async Task<IActionResult> UpdateRule(
        Guid ruleId, [FromBody] UpdateRoutingRuleRequest request, CancellationToken ct)
    { await _routingSvc.UpdateRuleAsync(ruleId, request, ct); return Ok(); }

    [HttpDelete("gateway-routing-rules/{ruleId:guid}")]
    public async Task<IActionResult> DeleteRule(Guid ruleId, CancellationToken ct)
    { await _routingSvc.DeleteRuleAsync(ruleId, ct); return Ok(); }
}
