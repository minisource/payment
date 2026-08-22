using Domain.Enums;
using Minisource.Common.Domain;

namespace Domain.Events;

/// <summary>
/// Event raised when a new payment is created.
/// </summary>
public record PaymentCreatedEvent(
    Guid PaymentId,
    long TrackingNumber,
    decimal Amount,
    string Currency,
    string? UserId,
    string Gateway,
    DateTime CreatedAt
) : DomainEvent;

/// <summary>
/// Event raised when payment status changes.
/// </summary>
public record PaymentStatusChangedEvent(
    Guid PaymentId,
    long TrackingNumber,
    PaymentStatus OldStatus,
    PaymentStatus NewStatus,
    string? Reason,
    DateTime ChangedAt
) : DomainEvent;

/// <summary>
/// Event raised when payment is completed successfully.
/// </summary>
public record PaymentCompletedEvent(
    Guid PaymentId,
    long TrackingNumber,
    decimal Amount,
    string Currency,
    string? UserId,
    string TransactionReference,
    DateTime CompletedAt
) : DomainEvent;

/// <summary>
/// Event raised when payment fails.
/// </summary>
public record PaymentFailedEvent(
    Guid PaymentId,
    long TrackingNumber,
    string Reason,
    string? GatewayErrorCode,
    DateTime FailedAt
) : DomainEvent;

/// <summary>
/// Event raised when payment is refunded.
/// </summary>
public record PaymentRefundedEvent(
    Guid PaymentId,
    long TrackingNumber,
    decimal RefundedAmount,
    string Reason,
    DateTime RefundedAt
) : DomainEvent;

/// <summary>
/// Event raised when credit is applied to payment.
/// </summary>
public record CreditAppliedToPaymentEvent(
    Guid PaymentId,
    Guid WalletId,
    decimal CreditAmount,
    decimal OriginalAmount,
    decimal AmountDue,
    DateTime AppliedAt
) : DomainEvent;
