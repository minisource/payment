namespace Application.DTOs;

// ─── Refund Request DTOs ────────────────────────────────────

public class RefundRequestDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string? ApplicationCode { get; set; }
    public Guid? PaymentIntentId { get; set; }
    public Guid PaymentTransactionId { get; set; }
    public Guid? WalletId { get; set; }
    public string RequestedByUserId { get; set; } = string.Empty;
    public string? RequestedByAdminId { get; set; }
    public string RequestSource { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string? AdminNote { get; set; }
    public string? RejectReason { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid GatewayConfigId { get; set; }
    public string ProviderCode { get; set; } = string.Empty;
    public string GatewayName { get; set; } = string.Empty;
    public string? GatewayRefundId { get; set; }
    public string? GatewayTrackingCode { get; set; }
    public string? GatewayReferenceId { get; set; }
    public Guid? WalletHoldId { get; set; }
    public string? ApprovedByUserId { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? ProcessedByUserId { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? FailedAt { get; set; }
    public string? FailureCode { get; set; }
    public string? FailureMessage { get; set; }
    public string? IdempotencyKey { get; set; }
    public string? CorrelationId { get; set; }
    public string? RequestId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<RefundGatewayAttemptDto>? Attempts { get; set; }
}

public class RefundGatewayAttemptDto
{
    public Guid Id { get; set; }
    public int AttemptNo { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? GatewayRefundId { get; set; }
    public string? GatewayTrackingCode { get; set; }
    public string? GatewayStatus { get; set; }
    public int? HttpStatusCode { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
}

// ─── Request DTOs ───────────────────────────────────────────

public class CreateRefundRequest
{
    public Guid PaymentTransactionId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "IRT";
    public string Reason { get; set; } = string.Empty;
    public string? IdempotencyKey { get; set; }
}

public class ApproveRefundRequest
{
    public string? AdminNote { get; set; }
}

public class RejectRefundRequest
{
    public string Reason { get; set; } = string.Empty;
}

public class ProcessRefundRequest
{
    public string? Note { get; set; }
}

// ─── Wallet Hold DTOs ───────────────────────────────────────

public class WalletHoldDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid WalletAccountId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string ReferenceType { get; set; } = string.Empty;
    public Guid ReferenceId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime? CapturedAt { get; set; }
    public DateTime? ReleasedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

// ─── List Response ────────────────────────────────────────

public class RefundListResponse
{
    public List<RefundRequestDto> Items { get; set; } = [];
    public int Total { get; set; }
    public int Skip { get; set; }
    public int Take { get; set; }
}
