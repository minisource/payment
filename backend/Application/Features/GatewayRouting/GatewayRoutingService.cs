using Application.DTOs;
using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using Microsoft.Extensions.Logging;
using Minisource.Common.Domain;
using Minisource.Common.Exceptions;

namespace Application.Features.GatewayRouting;

public class GatewayRoutingService : IGatewayRoutingService
{
    private readonly IGatewayConfigRepository _configRepo;
    private readonly IGatewayRoutingPolicyRepository _policyRepo;
    private readonly IGatewayRoutingRuleRepository _ruleRepo;
    private readonly IAuditLogRepository _auditRepo;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<GatewayRoutingService> _logger;

    public GatewayRoutingService(
        IGatewayConfigRepository configRepo,
        IGatewayRoutingPolicyRepository policyRepo,
        IGatewayRoutingRuleRepository ruleRepo,
        IAuditLogRepository auditRepo,
        IUnitOfWork uow,
        ILogger<GatewayRoutingService> logger)
    {
        _configRepo = configRepo;
        _policyRepo = policyRepo;
        _ruleRepo = ruleRepo;
        _auditRepo = auditRepo;
        _uow = uow;
        _logger = logger;
    }

    public async Task<GatewaySelectionResult> SelectGatewayAsync(GatewaySelectionRequest request, CancellationToken ct)
    {
        _logger.LogInformation("Selecting gateway for tenant {TenantId} amount {Amount} {Currency}",
            request.TenantId, request.Amount, request.Currency);

        // If explicit gateway_config_id provided
        if (request.GatewayConfigId.HasValue)
        {
            var explicitConfig = await _configRepo.GetByIdAsync(request.GatewayConfigId.Value, ct)
                ?? throw new BusinessException("Gateway config not found", "gateway_config_not_found");

            if (!explicitConfig.IsUsable)
                throw new BusinessException("Gateway config is not active", "gateway_config_disabled");
            if (!explicitConfig.AcceptsCurrency(request.Currency))
                throw new BusinessException("Currency not supported by gateway config", "gateway_config_currency_unsupported");
            if (!explicitConfig.AcceptsAmount(request.Amount))
                throw new BusinessException("Amount out of range for gateway config", "gateway_config_amount_out_of_range");

            return new GatewaySelectionResult(
                explicitConfig.Id, explicitConfig.ProviderCode, explicitConfig.AdapterType, "explicit", Array.Empty<Guid>());
        }

        // Load routing policy by scope
        var policy = await _policyRepo.GetActiveForScopeAsync(request.TenantId, request.ApplicationCode, ct);
        List<GatewayConfig> candidates;

        if (policy != null && policy.Rules.Any())
        {
            // Use routing rules to select candidates
            var rules = policy.Rules
                .Where(r => r.IsActive)
                .OrderBy(r => r.Priority)
                .ToList();

            // Filter rules by currency and amount
            var filteredRules = rules.Where(r =>
                (r.Currency == null || r.Currency.Equals(request.Currency, StringComparison.OrdinalIgnoreCase))
                && (!r.MinAmount.HasValue || request.Amount >= r.MinAmount.Value)
                && (!r.MaxAmount.HasValue || request.Amount <= r.MaxAmount.Value))
                .ToList();

            var configIds = filteredRules.Select(r => r.GatewayConfigId).Distinct().ToList();
            var allConfigs = await _configRepo.GetActiveAsync(request.TenantId, request.ApplicationCode, ct);
            candidates = allConfigs.Where(c => configIds.Contains(c.Id)).ToList();
        }
        else
        {
            // Fallback: use active gateway configs directly
            candidates = await _configRepo.GetActiveAsync(request.TenantId, request.ApplicationCode, ct);
        }

        // Apply filters
        candidates = candidates
            .Where(c => c.IsUsable
                && c.AcceptsCurrency(request.Currency)
                && c.AcceptsAmount(request.Amount))
            .OrderBy(c => c.Priority)
            .ToList();

        // Apply preferred provider filter
        if (!string.IsNullOrEmpty(request.PreferredProviderCode))
        {
            var preferred = candidates
                .Where(c => c.ProviderCode.Equals(request.PreferredProviderCode, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (preferred.Any())
                candidates = preferred.OrderBy(c => c.Priority).ToList();
        }

        if (!candidates.Any())
            throw new BusinessException("No available gateway for the given criteria", "gateway_routing_no_available_gateway");

        // Apply strategy
        string strategy;
        GatewayConfig selected;

        if (policy != null)
        {
            strategy = policy.Strategy.ToString().ToLowerInvariant();
            switch (policy.Strategy)
            {
                case GatewayRoutingStrategy.Random:
                    selected = candidates[Random.Shared.Next(candidates.Count)];
                    break;
                case GatewayRoutingStrategy.WeightedRandom:
                    selected = SelectWeightedRandom(candidates);
                    break;
                default:
                    selected = candidates.First();
                    break;
            }
        }
        else
        {
            strategy = "priority";
            selected = candidates.First();
        }

        // Build fallback list
        var fallbackIds = policy?.FallbackEnabled == true
            ? candidates.Where(c => c.Id != selected.Id).Select(c => c.Id).ToList()
            : new List<Guid>();

        _logger.LogInformation("Selected gateway {ProviderCode} config {ConfigId} using strategy {Strategy}",
            selected.ProviderCode, selected.Id, strategy);

        return new GatewaySelectionResult(
            selected.Id, selected.ProviderCode, selected.AdapterType, strategy,
            fallbackIds.AsReadOnly());
    }

    private static GatewayConfig SelectWeightedRandom(List<GatewayConfig> candidates)
    {
        var totalWeight = candidates.Sum(c => c.Weight);
        var roll = Random.Shared.Next(totalWeight);
        var cumulative = 0;
        foreach (var c in candidates)
        {
            cumulative += c.Weight;
            if (roll < cumulative) return c;
        }
        return candidates.Last();
    }

    // ═══ Policy CRUD ═══

    public async Task<GatewayRoutingPolicyDto> CreatePolicyAsync(CreateRoutingPolicyRequest request, CancellationToken ct)
    {
        var policy = new GatewayRoutingPolicy
        {
            TenantId = request.TenantId,
            ApplicationCode = request.ApplicationCode,
            Name = request.Name,
            Strategy = Enum.TryParse<GatewayRoutingStrategy>(request.Strategy, true, out var s)
                ? s : GatewayRoutingStrategy.Priority,
            FallbackEnabled = request.FallbackEnabled,
            RandomizeSamePriority = request.RandomizeSamePriority,
            Metadata = request.Metadata
        };

        if (request.Rules != null)
        {
            foreach (var ruleReq in request.Rules)
            {
                policy.AddRule(new GatewayRoutingRule
                {
                    GatewayConfigId = ruleReq.GatewayConfigId,
                    Priority = ruleReq.Priority, Weight = ruleReq.Weight,
                    Currency = ruleReq.Currency, MinAmount = ruleReq.MinAmount,
                    MaxAmount = ruleReq.MaxAmount
                });
            }
        }

        await _policyRepo.AddAsync(policy, ct);
        await _uow.SaveChangesAsync(ct);

        await _auditRepo.AddAsync(new AuditLog { Action = "gateway_routing_policy.created", EntityType = "GatewayRoutingPolicy", EntityId = policy.Id.ToString() }, ct);
        await _uow.SaveChangesAsync(ct);

        return MapPolicy(policy);
    }

    public async Task<GatewayRoutingPolicyDto> UpdatePolicyAsync(Guid policyId, CreateRoutingPolicyRequest request, CancellationToken ct)
    {
        var policy = await _policyRepo.GetByIdWithRulesAsync(policyId, ct)
            ?? throw new NotFoundException("GatewayRoutingPolicy", policyId);

        policy.Name = request.Name;
        policy.Strategy = Enum.TryParse<GatewayRoutingStrategy>(request.Strategy, true, out var strat)
            ? strat : policy.Strategy;
        policy.FallbackEnabled = request.FallbackEnabled;
        policy.RandomizeSamePriority = request.RandomizeSamePriority;
        policy.Metadata = request.Metadata;

        await _policyRepo.UpdateAsync(policy, ct);
        await _uow.SaveChangesAsync(ct);

        return MapPolicy(policy);
    }

    public async Task DeletePolicyAsync(Guid policyId, CancellationToken ct)
    {
        var policy = await _policyRepo.GetByIdAsync(policyId, ct)
            ?? throw new NotFoundException("GatewayRoutingPolicy", policyId);
        policy.DeletedAt = DateTime.UtcNow;
        await _policyRepo.UpdateAsync(policy, ct);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task<GatewayRoutingPolicyDto?> GetPolicyAsync(Guid? tenantId, string? applicationCode, CancellationToken ct)
    {
        var policy = await _policyRepo.GetActiveForScopeAsync(tenantId, applicationCode, ct);
        return policy == null ? null : MapPolicy(policy);
    }

    public async Task<List<GatewayRoutingPolicyDto>> GetAllPoliciesAsync(Guid? tenantId, string? applicationCode, CancellationToken ct)
    {
        var policies = await _policyRepo.GetAllAsync(tenantId, applicationCode, ct);
        return policies.Select(MapPolicy).ToList();
    }

    public async Task<GatewayRoutingRuleDto> AddRuleAsync(Guid policyId, CreateRoutingRuleRequest request, CancellationToken ct)
    {
        var policy = await _policyRepo.GetByIdAsync(policyId, ct)
            ?? throw new NotFoundException("GatewayRoutingPolicy", policyId);

        var rule = new GatewayRoutingRule
        {
            PolicyId = policyId, GatewayConfigId = request.GatewayConfigId,
            Priority = request.Priority, Weight = request.Weight,
            Currency = request.Currency, MinAmount = request.MinAmount,
            MaxAmount = request.MaxAmount
        };

        await _ruleRepo.AddAsync(rule, ct);
        await _uow.SaveChangesAsync(ct);

        return MapRule(rule);
    }

    public async Task UpdateRuleAsync(Guid ruleId, UpdateRoutingRuleRequest request, CancellationToken ct)
    {
        var rule = await _ruleRepo.GetByIdAsync(ruleId, ct)
            ?? throw new NotFoundException("GatewayRoutingRule", ruleId);

        if (request.Priority.HasValue) rule.Priority = request.Priority.Value;
        if (request.Weight.HasValue) rule.Weight = request.Weight.Value;
        if (request.Currency != null) rule.Currency = request.Currency;
        if (request.MinAmount.HasValue) rule.MinAmount = request.MinAmount;
        if (request.MaxAmount.HasValue) rule.MaxAmount = request.MaxAmount;
        if (request.Status != null) rule.Status = request.Status;

        await _ruleRepo.UpdateAsync(rule, ct);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task DeleteRuleAsync(Guid ruleId, CancellationToken ct)
    {
        var rule = await _ruleRepo.GetByIdAsync(ruleId, ct)
            ?? throw new NotFoundException("GatewayRoutingRule", ruleId);
        rule.DeletedAt = DateTime.UtcNow;
        await _ruleRepo.UpdateAsync(rule, ct);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task<List<GatewayRoutingRuleDto>> GetRulesAsync(Guid policyId, CancellationToken ct)
    {
        var rules = await _ruleRepo.GetByPolicyIdAsync(policyId, ct);
        return rules.Select(MapRule).ToList();
    }

    private static GatewayRoutingPolicyDto MapPolicy(GatewayRoutingPolicy p) => new()
    {
        Id = p.Id, TenantId = p.TenantId, ApplicationCode = p.ApplicationCode,
        Name = p.Name, Strategy = p.Strategy.ToString(),
        FallbackEnabled = p.FallbackEnabled, RandomizeSamePriority = p.RandomizeSamePriority,
        Status = p.Status, Rules = p.Rules.Select(r => MapRule(r)).ToList(),
        CreatedAt = p.CreatedAt, UpdatedAt = p.UpdatedAt
    };

    private static GatewayRoutingRuleDto MapRule(GatewayRoutingRule r) => new()
    {
        Id = r.Id, PolicyId = r.PolicyId, GatewayConfigId = r.GatewayConfigId,
        Priority = r.Priority, Weight = r.Weight,
        Currency = r.Currency, MinAmount = r.MinAmount, MaxAmount = r.MaxAmount,
        Status = r.Status
    };
}
