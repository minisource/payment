using Domain.Enums;
using Domain.Events;
using Domain.ValueObjects;
using Minisource.Common.Domain;

namespace Domain.Entities;

/// <summary>
/// Payment aggregate root. Manages payment lifecycle and related attempts/logs.
/// </summary>
public class Payment : AggregateRoot<Guid>
{
    /// <summary>
    /// Unique tracking number for external reference.
    /// </summary>
    public long TrackingNumber { get; private set; }

    /// <summary>
    /// Original payment amount.
    /// </summary>
    public decimal Amount { get; private set; }

    /// <summary>
    /// Currency code (default IRR).
    /// </summary>
    public string Currency { get; private set; } = "IRR";

    /// <summary>
    /// Current payment status.
    /// </summary>
    public PaymentStatus Status { get; private set; }

    /// <summary>
    /// Payment gateway name.
    /// </summary>
    public string Gateway { get; private set; } = string.Empty;

    /// <summary>
    /// Gateway-specific transaction reference.
    /// </summary>
    public string? TransactionReference { get; private set; }

    /// <summary>
    /// Callback URL for payment gateway.
    /// </summary>
    public string CallbackUrl { get; private set; } = string.Empty;

    /// <summary>
    /// Return URL after payment completion.
    /// </summary>
    public string ReturnUrl { get; private set; } = string.Empty;

    /// <summary>
    /// Additional metadata as JSON.
    /// </summary>
    public string Metadata { get; private set; } = string.Empty;

    /// <summary>
    /// User ID who initiated the payment.
    /// </summary>
    public string? UserId { get; private set; }

    /// <summary>
    /// Idempotency key to prevent duplicate payments.
    /// </summary>
    public string? IdempotencyKey { get; private set; }

    /// <summary>
    /// Credit amount applied from wallet.
    /// </summary>
    public decimal CreditApplied { get; private set; }

    /// <summary>
    /// Amount due after credit application.
    /// </summary>
    public decimal AmountDue { get; private set; }

    /// <summary>
    /// Payment completion timestamp.
    /// </summary>
    public DateTime? CompletedAt { get; private set; }

    // Navigation properties
    private readonly List<PaymentAttempt> _attempts = [];
    public IReadOnlyCollection<PaymentAttempt> Attempts => _attempts.AsReadOnly();

    private readonly List<PaymentLog> _logs = [];
    public IReadOnlyCollection<PaymentLog> Logs => _logs.AsReadOnly();

    // EF Core constructor
    private Payment() { }

    /// <summary>
    /// Creates a new payment.
    /// </summary>
    public static Payment Create(
        long trackingNumber,
        decimal amount,
        string currency,
        string gateway,
        string callbackUrl,
        string returnUrl,
        string? userId = null,
        string? metadata = null,
        string? idempotencyKey = null)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive", nameof(amount));

        if (string.IsNullOrWhiteSpace(gateway))
            throw new ArgumentException("Gateway is required", nameof(gateway));

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            TrackingNumber = trackingNumber,
            Amount = amount,
            Currency = currency.ToUpperInvariant(),
            Status = PaymentStatus.Pending,
            Gateway = gateway,
            CallbackUrl = callbackUrl,
            ReturnUrl = returnUrl,
            UserId = userId,
            Metadata = metadata ?? string.Empty,
            IdempotencyKey = idempotencyKey,
            CreditApplied = 0,
            AmountDue = amount
        };

        payment.AddLog("Created", $"Payment created with tracking number {trackingNumber}");

        payment.RaiseDomainEvent(new PaymentCreatedEvent(
            payment.Id,
            trackingNumber,
            amount,
            currency,
            userId,
            gateway,
            payment.CreatedAt));

        return payment;
    }

    /// <summary>
    /// Applies credit from wallet to reduce amount due.
    /// </summary>
    public void ApplyCredit(decimal creditAmount, Guid walletId)
    {
        if (Status != PaymentStatus.Pending)
            throw new InvalidOperationException("Can only apply credit to pending payments");

        if (creditAmount <= 0)
            throw new ArgumentException("Credit amount must be positive", nameof(creditAmount));

        if (creditAmount > AmountDue)
            creditAmount = AmountDue;

        CreditApplied = creditAmount;
        AmountDue = Amount - creditAmount;
        UpdatedAt = DateTime.UtcNow;

        AddLog("CreditApplied", $"Applied credit of {creditAmount} {Currency}. Amount due: {AmountDue}");

        RaiseDomainEvent(new CreditAppliedToPaymentEvent(
            Id,
            walletId,
            creditAmount,
            Amount,
            AmountDue,
            DateTime.UtcNow));
    }

    /// <summary>
    /// Marks payment as processing (gateway request initiated).
    /// </summary>
    public void StartProcessing()
    {
        if (Status != PaymentStatus.Pending)
            throw new InvalidOperationException($"Cannot start processing payment in {Status} status");

        var oldStatus = Status;
        Status = PaymentStatus.Processing;
        UpdatedAt = DateTime.UtcNow;

        AddLog("Processing", "Payment processing started");
        RaiseDomainEvent(new PaymentStatusChangedEvent(Id, TrackingNumber, oldStatus, Status, null, UpdatedAt));
    }

    /// <summary>
    /// Completes the payment successfully.
    /// </summary>
    public void Complete(string transactionReference)
    {
        if (Status != PaymentStatus.Processing)
            throw new InvalidOperationException($"Cannot complete payment in {Status} status");

        var oldStatus = Status;
        Status = PaymentStatus.Completed;
        TransactionReference = transactionReference;
        CompletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddLog("Completed", $"Payment completed with reference: {transactionReference}");

        RaiseDomainEvent(new PaymentStatusChangedEvent(Id, TrackingNumber, oldStatus, Status, null, UpdatedAt));
        RaiseDomainEvent(new PaymentCompletedEvent(
            Id,
            TrackingNumber,
            Amount,
            Currency,
            UserId,
            transactionReference,
            CompletedAt.Value));
    }

    /// <summary>
    /// Marks payment as failed.
    /// </summary>
    public void Fail(string reason, string? gatewayErrorCode = null)
    {
        if (Status == PaymentStatus.Completed || Status == PaymentStatus.Refunded)
            throw new InvalidOperationException($"Cannot fail payment in {Status} status");

        var oldStatus = Status;
        Status = PaymentStatus.Failed;
        UpdatedAt = DateTime.UtcNow;

        AddLog("Failed", $"Payment failed: {reason}. Error code: {gatewayErrorCode}");

        RaiseDomainEvent(new PaymentStatusChangedEvent(Id, TrackingNumber, oldStatus, Status, reason, UpdatedAt));
        RaiseDomainEvent(new PaymentFailedEvent(Id, TrackingNumber, reason, gatewayErrorCode, UpdatedAt));
    }

    /// <summary>
    /// Cancels the payment.
    /// </summary>
    public void Cancel(string reason)
    {
        if (Status != PaymentStatus.Pending && Status != PaymentStatus.Processing)
            throw new InvalidOperationException($"Cannot cancel payment in {Status} status");

        var oldStatus = Status;
        Status = PaymentStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;

        AddLog("Cancelled", $"Payment cancelled: {reason}");
        RaiseDomainEvent(new PaymentStatusChangedEvent(Id, TrackingNumber, oldStatus, Status, reason, UpdatedAt));
    }

    /// <summary>
    /// Refunds the payment.
    /// </summary>
    public void Refund(decimal refundAmount, string reason)
    {
        if (Status != PaymentStatus.Completed)
            throw new InvalidOperationException("Can only refund completed payments");

        if (refundAmount > Amount)
            throw new ArgumentException("Refund amount cannot exceed payment amount", nameof(refundAmount));

        var oldStatus = Status;
        Status = PaymentStatus.Refunded;
        UpdatedAt = DateTime.UtcNow;

        AddLog("Refunded", $"Payment refunded: {refundAmount} {Currency}. Reason: {reason}");

        RaiseDomainEvent(new PaymentStatusChangedEvent(Id, TrackingNumber, oldStatus, Status, reason, UpdatedAt));
        RaiseDomainEvent(new PaymentRefundedEvent(Id, TrackingNumber, refundAmount, reason, UpdatedAt));
    }

    /// <summary>
    /// Records a payment attempt.
    /// </summary>
    public PaymentAttempt AddAttempt(PaymentStatus status, string providerResponse)
    {
        var attempt = PaymentAttempt.Create(Id, _attempts.Count + 1, status, providerResponse);
        _attempts.Add(attempt);
        UpdatedAt = DateTime.UtcNow;

        AddLog("AttemptAdded", $"Payment attempt #{attempt.AttemptNumber} - Status: {status}");

        return attempt;
    }

    /// <summary>
    /// Adds a log entry.
    /// </summary>
    public void AddLog(string action, string details)
    {
        var log = PaymentLog.Create(Id, action, details);
        _logs.Add(log);
    }

    /// <summary>
    /// Checks if payment can be retried.
    /// </summary>
    public bool CanRetry => Status == PaymentStatus.Failed && _attempts.Count < 3;

    /// <summary>
    /// Checks if payment requires gateway verification.
    /// </summary>
    public bool RequiresGatewayPayment => AmountDue > 0;

    /// <summary>
    /// Gets the effective amount to charge (after credit).
    /// </summary>
    public Money GetEffectiveAmount() => Money.Create(AmountDue, Currency);
}
