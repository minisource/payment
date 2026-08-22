using Minisource.Common.Domain;

namespace Domain.Entities;

/// <summary>Wallet consistency check batch result.</summary>
public class WalletConsistencyCheck : Entity<Guid>
{
    public Guid TenantId { get; private set; }
    public Guid? WalletId { get; private set; }
    public string Scope { get; set; } = "wallet";
    public string Status { get; set; } = "running";
    public int CheckedWalletCount { get; set; }
    public int IssueCount { get; set; }
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public Guid? TriggeredByUserId { get; set; }
    public string? Summary { get; set; }
    public string? Metadata { get; set; }

    public WalletConsistencyCheck() { Id = Guid.NewGuid(); }
    public void Complete(int issueCount) { Status = issueCount > 0 ? "completed_with_issues" : "passed"; IssueCount = issueCount; CompletedAt = DateTime.UtcNow; }
}

/// <summary>A detected wallet consistency issue.</summary>
public class WalletConsistencyIssue : Entity<Guid>
{
    public Guid TenantId { get; private set; }
    public Guid CheckId { get; private set; }
    public Guid? WalletId { get; private set; }
    public string Severity { get; set; } = "medium";
    public string IssueCode { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
    public string? Details { get; set; }
    public string Status { get; set; } = "open";
    public DateTime? ResolvedAt { get; set; }
    public Guid? ResolvedByUserId { get; set; }
    public string? ResolutionNote { get; set; }

    public WalletConsistencyIssue() { Id = Guid.NewGuid(); }
    public void Resolve(Guid userId, string? note) { Status = "resolved"; ResolvedAt = DateTime.UtcNow; ResolvedByUserId = userId; ResolutionNote = note; }
    public void Acknowledge() { Status = "acknowledged"; }
}

/// <summary>Financial reconciliation batch.</summary>
public class FinancialReconciliationBatch : Entity<Guid>
{
    public Guid TenantId { get; init; }
    public string ReconciliationType { get; set; } = string.Empty;
    public string Status { get; set; } = "running";
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public int TotalChecked { get; set; }
    public int MatchedCount { get; set; }
    public int MismatchCount { get; set; }
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public Guid? TriggeredByUserId { get; set; }
    public string? Summary { get; set; }
    public string? Metadata { get; set; }

    public FinancialReconciliationBatch() { Id = Guid.NewGuid(); }
    public void Complete(int matched, int mismatched) { Status = "completed"; MatchedCount = matched; MismatchCount = mismatched; TotalChecked = matched + mismatched; CompletedAt = DateTime.UtcNow; }
}

/// <summary>A single reconciliation mismatch item.</summary>
public class FinancialReconciliationItem : Entity<Guid>
{
    public Guid TenantId { get; init; }
    public Guid BatchId { get; init; }
    public string ItemType { get; set; } = string.Empty;
    public string Status { get; set; } = "mismatched";
    public string Severity { get; set; } = "medium";
    public string PrimaryEntityType { get; set; } = string.Empty;
    public string PrimaryEntityId { get; set; } = string.Empty;
    public string? RelatedEntityType { get; set; }
    public string? RelatedEntityId { get; set; }
    public decimal? ExpectedAmount { get; set; }
    public decimal? ActualAmount { get; set; }
    public string? Currency { get; set; }
    public string? IssueCode { get; set; }
    public string? Message { get; set; }
    public string? Details { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public Guid? ResolvedByUserId { get; set; }
    public string? ResolutionNote { get; set; }

    public FinancialReconciliationItem() { Id = Guid.NewGuid(); }
    public void Resolve(Guid userId, string? note) { Status = "resolved"; ResolvedAt = DateTime.UtcNow; ResolvedByUserId = userId; ResolutionNote = note; }
    public void MarkFalsePositive() { Status = "false_positive"; ResolvedAt = DateTime.UtcNow; }
}

/// <summary>Limit usage tracking for daily/monthly count/amount enforcement.</summary>
public class PaymentLimitUsage : Entity<Guid>
{
    public Guid TenantId { get; set; }
    public string? OwnerType { get; set; }
    public Guid? OwnerId { get; set; }
    public string OperationType { get; set; } = string.Empty;
    public string? Currency { get; set; }
    public string WindowType { get; set; } = "daily";
    public DateTime WindowStart { get; set; }
    public DateTime WindowEnd { get; set; }
    public decimal AmountTotal { get; set; }
    public int CountTotal { get; set; }

    public PaymentLimitUsage() { Id = Guid.NewGuid(); }
}
