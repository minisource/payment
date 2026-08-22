using Minisource.Common.Domain;

namespace Domain.Entities;

/// <summary>
/// Withdrawal request aggregate root. Tracks the full lifecycle from create to paid/failed.
/// Funds are locked on create and captured on mark-paid.
/// </summary>
public class WithdrawalRequest : AggregateRoot<Guid>
{
    public Guid TenantId { get; private set; }
    public string? ApplicationCode { get; private set; }
    public Guid WalletId { get; private set; }
    public Guid PayoutAccountId { get; private set; }
    public string OwnerType { get; private set; } = "user";
    public Guid OwnerId { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "IRT";
    public decimal FeeAmount { get; private set; }
    public decimal NetAmount { get; private set; }
    public WithdrawalStatus Status { get; private set; } = WithdrawalStatus.PendingReview;
    public string? Description { get; private set; }
    public string? UserNote { get; private set; }
    public string? IdempotencyKey { get; private set; }

    // Wallet transaction tracking
    public Guid? LockedWalletTransactionId { get; private set; }
    public Guid? ReleaseWalletTransactionId { get; private set; }
    public Guid? CaptureWalletTransactionId { get; private set; }

    // Admin approval
    public Guid? ApprovedByUserId { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public string? ApprovalNote { get; private set; }

    // Rejection
    public Guid? RejectedByUserId { get; private set; }
    public DateTime? RejectedAt { get; private set; }
    public string? RejectionReason { get; private set; }

    // Cancellation
    public Guid? CancelledByUserId { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public string? CancellationReason { get; private set; }

    // More info
    public Guid? MoreInfoRequestedByUserId { get; private set; }
    public DateTime? MoreInfoRequestedAt { get; private set; }
    public string? MoreInfoReason { get; private set; }

    // Payment
    public Guid? PaidByUserId { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public string? PaidNote { get; private set; }
    public string? BankTrackingNumber { get; private set; }
    public string? BankReferenceId { get; private set; }

    public string? Metadata { get; private set; }
    public Guid? CreatedByUserId { get; private set; }
    public string? CreatedByClientId { get; private set; }

    public PayoutAccount? PayoutAccount { get; private set; }

    private WithdrawalRequest() { }

    public static WithdrawalRequest Create(
        Guid tenantId, Guid walletId, Guid payoutAccountId,
        string ownerType, Guid ownerId,
        decimal amount, string currency,
        decimal feeAmount, decimal netAmount,
        string? applicationCode = null,
        string? description = null,
        string? idempotencyKey = null,
        string? metadata = null,
        Guid? createdByUserId = null)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId required", nameof(tenantId));
        if (walletId == Guid.Empty) throw new ArgumentException("WalletId required", nameof(walletId));
        if (payoutAccountId == Guid.Empty) throw new ArgumentException("PayoutAccountId required", nameof(payoutAccountId));
        if (amount <= 0) throw new ArgumentException("Amount must be positive", nameof(amount));

        return new WithdrawalRequest
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ApplicationCode = applicationCode,
            WalletId = walletId, PayoutAccountId = payoutAccountId,
            OwnerType = ownerType.ToLowerInvariant(), OwnerId = ownerId,
            Amount = amount, Currency = currency.ToUpperInvariant(),
            FeeAmount = feeAmount, NetAmount = netAmount,
            Description = description,
            IdempotencyKey = idempotencyKey,
            Metadata = metadata,
            CreatedByUserId = createdByUserId
        };
    }

    public void SetLockedTransaction(Guid walletTransactionId)
    {
        LockedWalletTransactionId = walletTransactionId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Approve(Guid approvedByUserId, string? note = null)
    {
        if (Status != WithdrawalStatus.PendingReview && Status != WithdrawalStatus.MoreInfoRequired)
            throw new InvalidOperationException($"Cannot approve in {Status} status");
        Status = WithdrawalStatus.Approved;
        ApprovedByUserId = approvedByUserId;
        ApprovedAt = DateTime.UtcNow;
        ApprovalNote = note;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reject(Guid rejectedByUserId, string reason)
    {
        if (Status != WithdrawalStatus.PendingReview && Status != WithdrawalStatus.MoreInfoRequired)
            throw new InvalidOperationException($"Cannot reject in {Status} status");
        Status = WithdrawalStatus.Rejected;
        RejectedByUserId = rejectedByUserId;
        RejectedAt = DateTime.UtcNow;
        RejectionReason = reason;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel(Guid cancelledByUserId, string reason)
    {
        if (Status == WithdrawalStatus.Paid)
            throw new InvalidOperationException("Cannot cancel paid withdrawal");
        Status = WithdrawalStatus.Cancelled;
        CancelledByUserId = cancelledByUserId;
        CancelledAt = DateTime.UtcNow;
        CancellationReason = reason;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RequestMoreInfo(Guid requestedByUserId, string reason)
    {
        if (Status != WithdrawalStatus.PendingReview && Status != WithdrawalStatus.Approved)
            throw new InvalidOperationException($"Cannot request more info in {Status} status");
        Status = WithdrawalStatus.MoreInfoRequired;
        MoreInfoRequestedByUserId = requestedByUserId;
        MoreInfoRequestedAt = DateTime.UtcNow;
        MoreInfoReason = reason;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkProcessing(Guid processedByUserId, string? note = null)
    {
        if (Status != WithdrawalStatus.Approved)
            throw new InvalidOperationException($"Cannot mark processing in {Status} status");
        Status = WithdrawalStatus.ProcessingPayout;
        PaidNote = note;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkPaid(Guid paidByUserId, string? bankTrackingNumber = null,
        string? bankReferenceId = null, string? note = null)
    {
        if (Status == WithdrawalStatus.Paid)
            return; // Idempotent
        if (Status != WithdrawalStatus.Approved && Status != WithdrawalStatus.ProcessingPayout)
            throw new InvalidOperationException($"Cannot mark paid in {Status} status");
        Status = WithdrawalStatus.Paid;
        PaidByUserId = paidByUserId;
        PaidAt = DateTime.UtcNow;
        PaidNote = note;
        BankTrackingNumber = bankTrackingNumber;
        BankReferenceId = bankReferenceId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkFailed(string failureReason, Guid? failedByUserId = null)
    {
        if (Status == WithdrawalStatus.Paid)
            throw new InvalidOperationException("Cannot mark paid withdrawal as failed");
        Status = WithdrawalStatus.Failed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetReleaseTransaction(Guid walletTransactionId)
    {
        ReleaseWalletTransactionId = walletTransactionId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetCaptureTransaction(Guid walletTransactionId)
    {
        CaptureWalletTransactionId = walletTransactionId;
        UpdatedAt = DateTime.UtcNow;
    }
}

public enum WithdrawalStatus
{
    PendingReview = 1,
    Approved = 2,
    Rejected = 3,
    Cancelled = 4,
    MoreInfoRequired = 5,
    ProcessingPayout = 6,
    Paid = 7,
    Failed = 8,
    Expired = 9
}
