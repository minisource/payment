using System.Text.Json;
using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Minisource.Common.Domain;

namespace Application.Features.Risk;

public class RiskEvaluationService : IRiskEvaluationService
{
    private readonly ISettingsRepository _settingsRepo;
    private readonly IAuditLogRepository _auditRepo;
    private readonly PaymentDbContext _ctx;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<RiskEvaluationService> _logger;

    public RiskEvaluationService(ISettingsRepository settingsRepo, IAuditLogRepository auditRepo,
        Infrastructure.Persistence.PaymentDbContext ctx, IUnitOfWork uow, ILogger<RiskEvaluationService> logger)
    { _settingsRepo = settingsRepo; _auditRepo = auditRepo; _ctx = ctx; _uow = uow; _logger = logger; }

    public async Task<RiskEvaluationResult> EvaluateAsync(Guid tenantId, string operationType, string entityType, string entityId,
        string? ownerType, Guid? ownerId, decimal? amount, string? currency, CancellationToken ct)
    {
        var rules = await _settingsRepo.GetRiskRulesAsync(tenantId, ct);
        var activeRules = rules.Where(r => r.IsActive && (r.OperationType == operationType || r.OperationType == "*")).OrderBy(r => r.Priority).ToList();

        if (activeRules.Count == 0)
            return new RiskEvaluationResult("allow", 0, "low", null, null, new List<string> { "No active risk rules" });

        var reasons = new List<string>();
        int score = 0;
        string decision = "allow";

        foreach (var rule in activeRules)
        {
            bool ruleTriggered = false;
            if (rule.ThresholdAmount.HasValue && amount.HasValue && amount.Value >= rule.ThresholdAmount.Value)
                ruleTriggered = true;

            if (ruleTriggered)
            {
                score += 50;
                reasons.Add($"Rule '{rule.Name}': {rule.Action}");
                switch (rule.Action)
                {
                    case "block": decision = "block"; break;
                    case "require_review": if (decision != "block") decision = "require_review"; break;
                    case "flag": if (decision == "allow") decision = "flag"; break;
                }
            }
        }

        var riskLevel = score switch { >= 80 => "high", >= 40 => "medium", _ => "low" };

        var evaluation = new RiskEvaluation
        {
            TenantId = tenantId, OperationType = operationType,
            EntityType = entityType, EntityId = entityId,
            OwnerType = ownerType, OwnerId = ownerId,
            Amount = amount, Currency = currency,
            RiskScore = score, RiskLevel = riskLevel, Decision = decision,
            EvaluatedRules = JsonSerializer.Serialize(activeRules.Select(r => r.Name)),
            Reasons = JsonSerializer.Serialize(reasons)
        };

        // Persist evaluation to DB
        await _ctx.RiskEvaluations.AddAsync(evaluation, ct);

        // Create risk case for non-allow decisions
        Guid? riskCaseId = null;
        if (decision != "allow")
        {
            var riskCase = new RiskCase
            {
                TenantId = tenantId, CaseType = operationType + "_risk",
                Severity = riskLevel, EntityType = entityType, EntityId = entityId,
                OwnerType = ownerType, OwnerId = ownerId,
                Title = $"Risk detected: {operationType}",
                Description = string.Join("; ", reasons),
                Metadata = JsonSerializer.Serialize(new { decision, score })
            };
            await _ctx.RiskCases.AddAsync(riskCase, ct);
            riskCaseId = riskCase.Id;
            // Set FK after both are added (EF will fix up the relationship on save)
            evaluation.RiskCaseId = riskCase.Id;
            riskCase.RiskEvaluationId = evaluation.Id;
        }

        await _uow.SaveChangesAsync(ct);

        return new RiskEvaluationResult(decision, score, riskLevel, evaluation.Id, riskCaseId, reasons);
    }
}

public class StubUserComplianceService : IUserComplianceService
{
    public Task<UserComplianceStatus> GetComplianceStatusAsync(Guid tenantId, Guid userId, CancellationToken ct)
    {
        // TODO: Integrate with csharp-sdk Auth/User client when KYC endpoint is available
        return Task.FromResult(new UserComplianceStatus(true, "basic", false, false, new List<string>()));
    }
}
