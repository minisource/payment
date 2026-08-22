namespace Application.Features.Risk;

/// <summary>
/// Service for evaluating risk of financial operations.
/// </summary>
public interface IRiskEvaluationService
{
    Task<RiskEvaluationResult> EvaluateAsync(Guid tenantId, string operationType, string entityType, string entityId,
        string? ownerType, Guid? ownerId, decimal? amount, string? currency, CancellationToken ct);
}

public record RiskEvaluationResult(string Decision, int RiskScore, string RiskLevel, Guid? RiskEvaluationId, Guid? RiskCaseId, List<string> Reasons);

/// <summary>
/// Service for checking user compliance status (KYC, sanctions, etc.).
/// </summary>
public interface IUserComplianceService
{
    Task<UserComplianceStatus> GetComplianceStatusAsync(Guid tenantId, Guid userId, CancellationToken ct);
}

public sealed record UserComplianceStatus(bool KycCompleted, string? KycLevel, bool IsBlocked, bool IsHighRisk, List<string> Flags);
