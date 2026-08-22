namespace Application.Features.Notifications;

/// <summary>
/// Central registry of all Payment notification keys.
/// Exposed to frontend via API for discovery.
/// </summary>
public static class PaymentNotificationKeys
{
    // ─── User Notifications ──────────────────────────
    public const string PaymentSucceeded = "payment_succeeded";
    public const string PaymentFailed = "payment_failed";
    public const string WalletCharged = "wallet_charged";
    public const string WalletDebited = "wallet_debited";

    public const string RefundRequested = "refund_requested";
    public const string RefundApproved = "refund_approved";
    public const string RefundRejected = "refund_rejected";
    public const string RefundProcessing = "refund_processing";
    public const string RefundCompleted = "refund_completed";
    public const string RefundFailed = "refund_failed";

    public const string WithdrawalRequested = "withdrawal_requested";
    public const string WithdrawalApproved = "withdrawal_approved";
    public const string WithdrawalRejected = "withdrawal_rejected";
    public const string WithdrawalPaid = "withdrawal_paid";
    public const string WithdrawalFailed = "withdrawal_failed";

    public const string PaymentLinkPaid = "payment_link_paid";

    // ─── Admin Notifications ─────────────────────────
    public const string PaymentFailedAdmin = "payment_failed_admin";
    public const string GatewayCallbackFailedAdmin = "gateway_callback_failed_admin";
    public const string GatewayConfigMissingAdmin = "gateway_config_missing_admin";

    public const string RefundRequestedAdmin = "refund_requested_admin";
    public const string RefundRequiresManualReviewAdmin = "refund_requires_manual_review_admin";
    public const string RefundCompletedAdmin = "refund_completed_admin";
    public const string RefundHoldCaptureFailedAdmin = "refund_hold_capture_failed_admin";
    public const string RefundHoldReleaseFailedAdmin = "refund_hold_release_failed_admin";

    public const string WithdrawalRequestedAdmin = "withdrawal_requested_admin";
    public const string WithdrawalRequiresApprovalAdmin = "withdrawal_requires_approval_admin";
    public const string WithdrawalPaidAdmin = "withdrawal_paid_admin";

    public const string ReconciliationMismatchAdmin = "reconciliation_mismatch_admin";
    public const string RiskCaseCreatedAdmin = "risk_case_created_admin";

    // ─── Registry for frontend discovery ─────────────

    public static IReadOnlyList<NotificationKeyInfo> All => new List<NotificationKeyInfo>
    {
        new(PaymentSucceeded, "user", "Payment succeeded notification sent to user"),
        new(PaymentFailed, "user", "Payment failed notification sent to user"),
        new(WalletCharged, "user", "Wallet credited notification"),
        new(WalletDebited, "user", "Wallet debited notification"),
        new(RefundRequested, "user", "Refund request confirmation to user"),
        new(RefundApproved, "user", "Refund approved notification to user"),
        new(RefundRejected, "user", "Refund rejected notification to user"),
        new(RefundProcessing, "user", "Refund processing started notification"),
        new(RefundCompleted, "user", "Refund completed notification to user"),
        new(RefundFailed, "user", "Refund failed notification to user"),
        new(WithdrawalRequested, "user", "Withdrawal request confirmation to user"),
        new(WithdrawalApproved, "user", "Withdrawal approved notification to user"),
        new(WithdrawalRejected, "user", "Withdrawal rejected notification to user"),
        new(WithdrawalPaid, "user", "Withdrawal paid notification to user"),
        new(WithdrawalFailed, "user", "Withdrawal failed notification to user"),
        new(PaymentLinkPaid, "user", "Payment link paid notification"),

        new(PaymentFailedAdmin, "admin", "Payment failed — admin alert"),
        new(GatewayCallbackFailedAdmin, "admin", "Gateway callback failed — admin alert"),
        new(GatewayConfigMissingAdmin, "admin", "Gateway config missing — admin alert"),
        new(RefundRequestedAdmin, "admin", "New refund request — admin alert"),
        new(RefundRequiresManualReviewAdmin, "admin", "Refund requires manual review — admin alert"),
        new(RefundCompletedAdmin, "admin", "Refund completed — admin info"),
        new(RefundHoldCaptureFailedAdmin, "admin", "Refund hold capture failed — admin alert"),
        new(RefundHoldReleaseFailedAdmin, "admin", "Refund hold release failed — admin alert"),
        new(WithdrawalRequestedAdmin, "admin", "New withdrawal request — admin alert"),
        new(WithdrawalRequiresApprovalAdmin, "admin", "Withdrawal requires approval — admin alert"),
        new(WithdrawalPaidAdmin, "admin", "Withdrawal paid — admin info"),
        new(ReconciliationMismatchAdmin, "admin", "Reconciliation mismatch detected — admin alert"),
        new(RiskCaseCreatedAdmin, "admin", "Risk case created — admin alert"),
    };
}

public sealed record NotificationKeyInfo(
    string Key,
    string RecipientType,
    string Description);
