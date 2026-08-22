using Domain.Enums;
using Domain.Events;
using Minisource.Common.Domain;

namespace Domain.Entities;

/// <summary>
/// Refund request aggregate root. Manages the full refund lifecycle:
/// requested → approved → hold → gateway → wallet debit → completed.
/// Uses strict pre-hold policy: gateway is called only after wallet hold succeeds.
/// </summary>
public sealed class RefundRequest : AggregateRoot<Guid>
{
    public Guid TenantId { get; private set; }
    public string? ApplicationCode { get; private set; }

    public Guid? PaymentIntentId { get; private set; }
    public Guid PaymentTransactionId { get; private set; }
    public Guid? WalletId { get; private set; }

    public string RequestedByUserId { get; private set; } = string.Empty;
    public string? RequestedByAdminId { get; private set; }
    public string RequestSource { get; private set; } = "admin"; // user, admin, system

    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = string.Empty;

    public string Reason { get; private set; } = string.Empty;
    public string? AdminNote { get; private set; }
    public string? RejectReason { get; private set; }

    public RefundRequestStatus Status { get; private set; } = RefundRequestStatus.Requested;

    // Gateway config — must use the original gateway from the payment transaction
    public Guid GatewayConfigId { get; private set; }
    public string ProviderCode { get; private set; } = string.Empty;
    public string GatewayName { get; private set; } = string.Empty;

    // Gateway refund result
    public string? GatewayRefundId { get; private set; }
    public string? GatewayTrackingCode { get; private set; }
    public string? GatewayReferenceId { get; private set; }

    // Wallet hold
    public Guid? WalletHoldId { get; private set; }

    // Approval tracking
    public string? ApprovedByUserId { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public string? ProcessedByUserId { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public DateTime? FailedAt { get; private set; }

    // Failure details
    public string? FailureCode { get; private set; }
    public string? FailureMessage { get; private set; }
    public string? SafeGatewayResponseJson { get; private set; }

    // Idempotency
    public string? IdempotencyKey { get; private set; }
    public string? CorrelationId { get; private set; }
    public string? RequestId { get; private set; }

    // Children
    private readonly List<RefundGatewayAttempt> _attempts = [];
    public IReadOnlyCollection<RefundGatewayAttempt> Attempts => _attempts.AsReadOnly();

    private RefundRequest() { }

    public static RefundRequest Create(
        Guid tenantId,
        Guid paymentTransactionId,
        Guid? paymentIntentId,
        Guid gatewayConfigId,
        string providerCode,
        string gatewayName,
        decimal amount,
        string currency,
        string reason,
        string requestedByUserId,
        string requestSource = "admin",
        string? applicationCode = null,
        string? idempotencyKey = null,
        string? correlationId = null,
        string? requestId = null,
        Guid? walletId = null)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("TenantId is required", nameof(tenantId));
        if (paymentTransactionId == Guid.Empty)
            throw new ArgumentException("PaymentTransactionId is required", nameof(paymentTransactionId));
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive", nameof(amount));
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency is required", nameof(currency));
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Reason is required", nameof(reason));
        if (string.IsNullOrWhiteSpace(providerCode))
            throw new ArgumentException("ProviderCode is required", nameof(providerCode));

        var refund = new RefundRequest
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ApplicationCode = applicationCode,
            PaymentIntentId = paymentIntentId,
            PaymentTransactionId = paymentTransactionId,
            GatewayConfigId = gatewayConfigId,
            ProviderCode = providerCode,
            GatewayName = gatewayName,
            Amount = amount,
            Currency = currency.ToUpperInvariant(),
            Reason = reason,
            RequestedByUserId = requestedByUserId,
            RequestedByAdminId = requestSource == "admin" ? requestedByUserId : null,
            RequestSource = requestSource,
            WalletId = walletId,
            IdempotencyKey = idempotencyKey,
            CorrelationId = correlationId,
            RequestId = requestId,
            Status = RefundRequestStatus.Requested
        };

        refund.RaiseDomainEvent(new RefundRequestedEvent(
            refund.Id, tenantId, paymentTransactionId, amount, currency, reason, refund.CreatedAt));

        return refund;
    }

    // ─── Status Transitions ────────────────────────────────────

    public void MarkPendingReview()
    {
        EnsureStatus(RefundRequestStatus.Requested);
        Status = RefundRequestStatus.PendingReview;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new RefundStatusChangedEvent(Id, TenantId,
            RefundRequestStatus.Requested, RefundRequestStatus.PendingReview, null, UpdatedAt));
    }

    public void Approve(string approvedByUserId, string? adminNote = null)
    {
        EnsureStatus(RefundRequestStatus.PendingReview);
        Status = RefundRequestStatus.Approved;
        ApprovedByUserId = approvedByUserId;
        ApprovedAt = DateTime.UtcNow;
        AdminNote = adminNote;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new RefundStatusChangedEvent(Id, TenantId,
            RefundRequestStatus.PendingReview, RefundRequestStatus.Approved, adminNote, UpdatedAt));
    }

    public void Reject(string rejectReason, string? rejectedByUserId = null)
    {
        EnsureStatus(RefundRequestStatus.PendingReview);
        Status = RefundRequestStatus.Rejected;
        RejectReason = rejectReason;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new RefundRejectedEvent(Id, TenantId, rejectReason, UpdatedAt));
    }

    public void Cancel()
    {
        if (Status is RefundRequestStatus.Completed or RefundRequestStatus.GatewaySucceeded
            or RefundRequestStatus.WalletDebited)
            throw new InvalidOperationException($"Cannot cancel refund in {Status} status");

        var oldStatus = Status;
        Status = RefundRequestStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new RefundStatusChangedEvent(Id, TenantId, oldStatus, RefundRequestStatus.Cancelled, null, UpdatedAt));
    }

    public void MarkHoldPending()
    {
        EnsureStatus(RefundRequestStatus.Approved);
        Status = RefundRequestStatus.HoldPending;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkHoldCreated(Guid walletHoldId)
    {
        EnsureStatus(RefundRequestStatus.HoldPending);
        WalletHoldId = walletHoldId;
        Status = RefundRequestStatus.HoldCreated;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new RefundHoldCreatedEvent(Id, TenantId, walletHoldId, UpdatedAt));
    }

    public void MarkHoldFailed(string failureCode, string failureMessage, bool requiresManualReview = false)
    {
        EnsureStatus(RefundRequestStatus.HoldPending);

        if (requiresManualReview)
        {
            Status = RefundRequestStatus.RequiresManualReview;
        }
        else
        {
            Status = RefundRequestStatus.Failed;
            FailedAt = DateTime.UtcNow;
        }

        FailureCode = failureCode;
        FailureMessage = failureMessage;
        UpdatedAt = DateTime.UtcNow;

        if (requiresManualReview)
            RaiseDomainEvent(new RefundManualReviewRequiredEvent(Id, TenantId, failureCode, failureMessage, UpdatedAt));
        else
            RaiseDomainEvent(new RefundFailedEvent(Id, TenantId, failureCode, failureMessage, UpdatedAt));
    }

    public void StartProcessing(string processedByUserId)
    {
        EnsureStatus(RefundRequestStatus.HoldCreated);
        Status = RefundRequestStatus.Processing;
        ProcessedByUserId = processedByUserId;
        ProcessedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new RefundStatusChangedEvent(Id, TenantId,
            RefundRequestStatus.HoldCreated, RefundRequestStatus.Processing, null, UpdatedAt));
    }

    public void MarkGatewaySubmitted()
    {
        EnsureStatus(RefundRequestStatus.Processing);
        Status = RefundRequestStatus.GatewaySubmitted;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkGatewaySucceeded(
        string? gatewayRefundId = null,
        string? gatewayTrackingCode = null,
        string? safeResponse = null)
    {
        EnsureStatus(RefundRequestStatus.GatewaySubmitted);
        Status = RefundRequestStatus.GatewaySucceeded;
        GatewayRefundId = gatewayRefundId;
        GatewayTrackingCode = gatewayTrackingCode;
        SafeGatewayResponseJson = safeResponse;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new RefundGatewaySucceededEvent(Id, TenantId, gatewayRefundId, UpdatedAt));
    }

    public void MarkGatewayFailed(
        string errorCode,
        string errorMessage,
        string? safeResponse = null,
        bool requiresManualReview = false)
    {
        if (Status is not (RefundRequestStatus.Processing or RefundRequestStatus.GatewaySubmitted))
            throw new InvalidOperationException($"Cannot mark gateway failed in {Status} status");

        Status = requiresManualReview
            ? RefundRequestStatus.RequiresManualReview
            : RefundRequestStatus.GatewayFailed;

        FailureCode = errorCode;
        FailureMessage = errorMessage;
        SafeGatewayResponseJson = safeResponse;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new RefundGatewayFailedEvent(Id, TenantId, errorCode, errorMessage, requiresManualReview, UpdatedAt));
    }

    public void MarkWalletDebitPending()
    {
        EnsureStatus(RefundRequestStatus.GatewaySucceeded);
        Status = RefundRequestStatus.WalletDebitPending;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkWalletDebited()
    {
        EnsureStatus(RefundRequestStatus.WalletDebitPending);
        Status = RefundRequestStatus.WalletDebited;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkCompleted()
    {
        if (Status is not (RefundRequestStatus.WalletDebited or RefundRequestStatus.GatewaySucceeded))
            throw new InvalidOperationException($"Cannot complete refund in {Status} status");

        Status = RefundRequestStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new RefundCompletedEvent(Id, TenantId, Amount, Currency, CompletedAt.Value));
    }

    public void MarkRequiresManualReview(string failureCode, string failureMessage, Guid? walletHoldId = null)
    {
        Status = RefundRequestStatus.RequiresManualReview;
        FailureCode = failureCode;
        FailureMessage = failureMessage;
        WalletHoldId = walletHoldId ?? WalletHoldId;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new RefundManualReviewRequiredEvent(Id, TenantId, failureCode, failureMessage, UpdatedAt));
    }

    // ─── Attempts ──────────────────────────────────────────────

    public RefundGatewayAttempt AddAttempt(
        int attemptNo,
        string? requestPayloadSafe = null)
    {
        var attempt = RefundGatewayAttempt.Create(Id, attemptNo, ProviderCode, GatewayName, requestPayloadSafe);
        _attempts.Add(attempt);
        UpdatedAt = DateTime.UtcNow;
        return attempt;
    }

    // ─── Validation ────────────────────────────────────────────

    public bool CanProcess => Status is RefundRequestStatus.HoldCreated or RefundRequestStatus.Approved;
    public bool CanRetry => Status is RefundRequestStatus.GatewayFailed or RefundRequestStatus.Failed;
    public bool IsTerminal => Status is RefundRequestStatus.Completed or RefundRequestStatus.Rejected
        or RefundRequestStatus.Cancelled;
    public bool HasActiveHold => WalletHoldId.HasValue
        && Status is RefundRequestStatus.HoldCreated or RefundRequestStatus.Processing
            or RefundRequestStatus.GatewaySubmitted;

    private void EnsureStatus(RefundRequestStatus expected)
    {
        if (Status != expected)
            throw new InvalidOperationException(
                $"Refund {Id} has status {Status}, expected {expected}");
    }
}


