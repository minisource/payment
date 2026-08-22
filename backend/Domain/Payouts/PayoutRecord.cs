using Minisource.Common.Domain;

namespace Domain.Entities;

/// <summary>
/// Payout record tracking the actual bank payout for a withdrawal request.
/// Only manual_bank_transfer method is supported currently.
/// </summary>
public class PayoutRecord : Entity<Guid>
{
    public Guid TenantId { get; private set; }
    public string? ApplicationCode { get; private set; }
    public Guid WithdrawalRequestId { get; private set; }
    public Guid PayoutAccountId { get; private set; }
    public Guid WalletId { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "IRT";
    public decimal FeeAmount { get; private set; }
    public decimal NetAmount { get; private set; }
    public string PayoutMethod { get; private set; } = "manual_bank_transfer";
    public PayoutRecordStatus Status { get; private set; } = PayoutRecordStatus.Pending;
    public string? BankTrackingNumber { get; private set; }
    public string? BankReferenceId { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public DateTime? FailedAt { get; private set; }
    public string? FailureReason { get; private set; }
    public Guid? CreatedByUserId { get; private set; }
    public string? Metadata { get; private set; }

    private PayoutRecord() { }

    public static PayoutRecord Create(
        Guid tenantId, Guid withdrawalRequestId, Guid payoutAccountId, Guid walletId,
        decimal amount, string currency, decimal feeAmount, decimal netAmount,
        string payoutMethod = "manual_bank_transfer",
        string? applicationCode = null, Guid? createdByUserId = null, string? metadata = null)
    {
        return new PayoutRecord
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId, ApplicationCode = applicationCode,
            WithdrawalRequestId = withdrawalRequestId,
            PayoutAccountId = payoutAccountId,
            WalletId = walletId,
            Amount = amount, Currency = currency.ToUpperInvariant(),
            FeeAmount = feeAmount, NetAmount = netAmount,
            PayoutMethod = payoutMethod,
            CreatedByUserId = createdByUserId, Metadata = metadata
        };
    }

    public void MarkSucceeded(string? bankTrackingNumber = null, string? bankReferenceId = null)
    {
        if (Status == PayoutRecordStatus.Succeeded)
            return; // Idempotent
        Status = PayoutRecordStatus.Succeeded;
        PaidAt = DateTime.UtcNow;
        BankTrackingNumber = bankTrackingNumber;
        BankReferenceId = bankReferenceId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkFailed(string reason)
    {
        Status = PayoutRecordStatus.Failed;
        FailedAt = DateTime.UtcNow;
        FailureReason = reason;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkProcessing()
    {
        Status = PayoutRecordStatus.Processing;
        UpdatedAt = DateTime.UtcNow;
    }
}

public enum PayoutRecordStatus
{
    Pending = 1,
    Processing = 2,
    Succeeded = 3,
    Failed = 4,
    Cancelled = 5
}
