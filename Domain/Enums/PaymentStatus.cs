namespace Domain.Enums;

/// <summary>
/// Payment status enum representing the lifecycle of a payment.
/// </summary>
public enum PaymentStatus
{
    /// <summary>
    /// Payment created, awaiting processing.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Payment is being processed by gateway.
    /// </summary>
    Processing = 1,

    /// <summary>
    /// Awaiting user action (redirect to gateway).
    /// </summary>
    AwaitingConfirmation = 2,

    /// <summary>
    /// Payment completed successfully.
    /// </summary>
    Completed = 3,

    /// <summary>
    /// Payment failed.
    /// </summary>
    Failed = 4,

    /// <summary>
    /// Payment cancelled by user or system.
    /// </summary>
    Cancelled = 5,

    /// <summary>
    /// Payment refunded.
    /// </summary>
    Refunded = 6,

    /// <summary>
    /// Payment partially refunded.
    /// </summary>
    PartiallyRefunded = 7,

    /// <summary>
    /// Payment expired.
    /// </summary>
    Expired = 8
}