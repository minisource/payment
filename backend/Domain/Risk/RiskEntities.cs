using Minisource.Common.Domain;

namespace Domain.Entities;

/// <summary>Risk evaluation result for a financial operation.</summary>
public class RiskEvaluation : Entity<Guid>
{
    public Guid TenantId { get; set; }
    public Guid? RiskCaseId { get; set; }
    public string OperationType { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string? OwnerType { get; set; }
    public Guid? OwnerId { get; set; }
    public decimal? Amount { get; set; }
    public string? Currency { get; set; }
    public int RiskScore { get; set; }
    public string RiskLevel { get; set; } = "low";
    public string Decision { get; set; } = "allow";
    public string? EvaluatedRules { get; set; }
    public string? Reasons { get; set; }

    public RiskEvaluation() { Id = Guid.NewGuid(); }
}

/// <summary>A risk/security case opened for review by admins.</summary>
public class RiskCase : Entity<Guid>
{
    public Guid TenantId { get; set; }
    public Guid? RiskEvaluationId { get; set; }
    public string CaseType { get; set; } = string.Empty;
    public string Severity { get; set; } = "medium";
    public string Status { get; set; } = "open";
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string? OwnerType { get; set; }
    public Guid? OwnerId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public DateTime OpenedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }
    public string? ResolutionNote { get; set; }
    public Guid? ResolvedByUserId { get; set; }
    public string? Metadata { get; set; }

    public RiskCase() { Id = Guid.NewGuid(); }
    public void Acknowledge() { Status = "acknowledged"; UpdatedAt = DateTime.UtcNow; }
    public void Resolve(Guid userId, string? note) { Status = "resolved"; ResolvedAt = DateTime.UtcNow; ResolvedByUserId = userId; ResolutionNote = note; UpdatedAt = DateTime.UtcNow; }
    public void MarkFalsePositive(Guid userId) { Status = "false_positive"; ResolvedAt = DateTime.UtcNow; ResolvedByUserId = userId; UpdatedAt = DateTime.UtcNow; }
}
