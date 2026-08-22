namespace Domain.Enums;

/// <summary>
/// Refund request lifecycle status.
/// </summary>
public enum RefundRequestStatus
{
    Requested = 1,
    PendingReview = 2,
    Approved = 3,
    Rejected = 4,
    Cancelled = 5,

    // Hold phase
    HoldPending = 6,
    HoldCreated = 7,

    // Gateway phase
    Processing = 8,
    GatewaySubmitted = 9,
    GatewaySucceeded = 10,
    GatewayFailed = 11,

    // Wallet debit phase
    WalletDebitPending = 12,
    WalletDebited = 13,

    // Terminal
    Completed = 14,
    Failed = 15,
    RequiresManualReview = 16
}

/// <summary>
/// Refund gateway attempt status.
/// </summary>
public enum RefundGatewayAttemptStatus
{
    Pending = 1,
    Submitted = 2,
    Succeeded = 3,
    Failed = 4,
    Timeout = 5,
    Unknown = 6
}

/// <summary>
/// Wallet hold lifecycle status.
/// </summary>
public enum WalletHoldStatus
{
    Active = 1,
    Captured = 2,
    Released = 3,
    Expired = 4,
    Failed = 5
}
