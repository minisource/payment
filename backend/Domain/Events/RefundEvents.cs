using Domain.Enums;
using Minisource.Common.Domain;

namespace Domain.Events;

// ─── Refund Domain Events ─────────────────────────────────────

public sealed record RefundRequestedEvent(
    Guid RefundRequestId,
    Guid TenantId,
    Guid PaymentTransactionId,
    decimal Amount,
    string Currency,
    string Reason,
    DateTime OccurredAt) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
}

public sealed record RefundStatusChangedEvent(
    Guid RefundRequestId,
    Guid TenantId,
    RefundRequestStatus OldStatus,
    RefundRequestStatus NewStatus,
    string? Note,
    DateTime OccurredAt) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
}

public sealed record RefundHoldCreatedEvent(
    Guid RefundRequestId,
    Guid TenantId,
    Guid WalletHoldId,
    DateTime OccurredAt) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
}

public sealed record RefundGatewaySucceededEvent(
    Guid RefundRequestId,
    Guid TenantId,
    string? GatewayRefundId,
    DateTime OccurredAt) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
}

public sealed record RefundGatewayFailedEvent(
    Guid RefundRequestId,
    Guid TenantId,
    string ErrorCode,
    string ErrorMessage,
    bool RequiresManualReview,
    DateTime OccurredAt) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
}

public sealed record RefundRejectedEvent(
    Guid RefundRequestId,
    Guid TenantId,
    string RejectReason,
    DateTime OccurredAt) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
}

public sealed record RefundFailedEvent(
    Guid RefundRequestId,
    Guid TenantId,
    string FailureCode,
    string FailureMessage,
    DateTime OccurredAt) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
}

public sealed record RefundCompletedEvent(
    Guid RefundRequestId,
    Guid TenantId,
    decimal Amount,
    string Currency,
    DateTime OccurredAt) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
}

public sealed record RefundManualReviewRequiredEvent(
    Guid RefundRequestId,
    Guid TenantId,
    string FailureCode,
    string FailureMessage,
    DateTime OccurredAt) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
}
