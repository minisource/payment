using Minisource.Common.Domain;

namespace Domain.Entities;

public class AdminApprovalRequest : Entity<Guid>
{
    public Guid TenantId { get; set; }
    public string OperationType { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string Status { get; set; } = "pending";
    public Guid RequestedByUserId { get; set; }
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public Guid? ApprovedByUserId { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public Guid? RejectedByUserId { get; set; }
    public DateTime? RejectedAt { get; set; }
    public string? RejectionReason { get; set; }
    public int RequiredApprovals { get; set; } = 1;
    public int ApprovalCount { get; set; }
    public string RequestPayload { get; set; } = "{}";
    public Guid? RiskEvaluationId { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string? Metadata { get; set; }

    public AdminApprovalRequest() { Id = Guid.NewGuid(); }
    public void Approve(Guid userId) { ApprovedByUserId = userId; ApprovedAt = DateTime.UtcNow; Status = "approved"; UpdatedAt = DateTime.UtcNow; }
    public void Reject(Guid userId, string? reason) { RejectedByUserId = userId; RejectedAt = DateTime.UtcNow; RejectionReason = reason; Status = "rejected"; UpdatedAt = DateTime.UtcNow; }
    public void Execute(Guid userId) { Status = "executed"; UpdatedAt = DateTime.UtcNow; }
    public void Cancel() { Status = "cancelled"; UpdatedAt = DateTime.UtcNow; }
}

public class AdminApprovalDecision : Entity<Guid>
{
    public Guid TenantId { get; set; }
    public Guid ApprovalRequestId { get; set; }
    public string Decision { get; set; } = string.Empty;
    public Guid DecidedByUserId { get; set; }
    public DateTime DecidedAt { get; set; } = DateTime.UtcNow;
    public string? Reason { get; set; }
    public string? Metadata { get; set; }
    public AdminApprovalDecision() { Id = Guid.NewGuid(); }
}

public class AdminApprovalPolicy : Entity<Guid>
{
    public Guid? TenantId { get; set; }
    public string OperationType { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
    public decimal? AmountThreshold { get; set; }
    public string? Currency { get; set; }
    public int RequiredApprovals { get; set; } = 1;
    public bool RequireDifferentUser { get; set; } = true;
    public string Status { get; set; } = "active";
    public string? Metadata { get; set; }
    public DateTime? DeletedAt { get; set; }
    public AdminApprovalPolicy() { Id = Guid.NewGuid(); }
}
