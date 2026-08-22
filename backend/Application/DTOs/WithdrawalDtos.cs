namespace Application.DTOs;

// ═══ Payout Account DTOs ═══════════════════════════════

public class CreatePayoutAccountRequest
{
    public string AccountType { get; set; } = "iban";
    public string? BankName { get; set; }
    public string? BankCode { get; set; }
    public string? CardNumber { get; set; }
    public string? Iban { get; set; }
    public string? AccountNumber { get; set; }
    public string HolderName { get; set; } = string.Empty;
    public string Currency { get; set; } = "IRT";
    public string? Metadata { get; set; }
}

public class UpdatePayoutAccountRequest
{
    public string? HolderName { get; set; }
    public string? BankName { get; set; }
    public string? BankCode { get; set; }
    public string? Metadata { get; set; }
}

public class PayoutAccountDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string OwnerType { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
    public string AccountType { get; set; } = string.Empty;
    public string? BankName { get; set; }
    public string? BankCode { get; set; }
    public string? CardNumberMasked { get; set; }
    public string? IbanMasked { get; set; }
    public string? AccountNumberMasked { get; set; }
    public string HolderName { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class AdminVerifyPayoutAccountRequest
{
    public string? Note { get; set; }
}

public class AdminRejectPayoutAccountRequest
{
    public string Reason { get; set; } = string.Empty;
}

// ═══ Withdrawal Request DTOs ═══════════════════════════

public class CreateWithdrawalRequest
{
    public Guid WalletId { get; set; }
    public Guid PayoutAccountId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "IRT";
    public string? Description { get; set; }
    public string? Metadata { get; set; }
}

public class CancelWithdrawalRequest
{
    public string Reason { get; set; } = string.Empty;
}

public class WithdrawalRequestDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string? ApplicationCode { get; set; }
    public Guid WalletId { get; set; }
    public Guid PayoutAccountId { get; set; }
    public string OwnerType { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
    public decimal Amount { get; set; }
    public decimal FeeAmount { get; set; }
    public decimal NetAmount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? LockedWalletTransactionId { get; set; }
    public Guid? ReleaseWalletTransactionId { get; set; }
    public Guid? CaptureWalletTransactionId { get; set; }
    public Guid? ApprovedByUserId { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovalNote { get; set; }
    public string? RejectionReason { get; set; }
    public string? CancellationReason { get; set; }
    public string? BankTrackingNumber { get; set; }
    public string? BankReferenceId { get; set; }
    public string? PaidNote { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

// ═══ Admin Withdrawal Action DTOs ══════════════════════

public class AdminWithdrawalApproveRequest
{
    public string? Note { get; set; }
}

public class AdminWithdrawalRejectRequest
{
    public string Reason { get; set; } = string.Empty;
}

public class AdminWithdrawalMoreInfoRequest
{
    public string Reason { get; set; } = string.Empty;
}

public class AdminWithdrawalMarkPaidRequest
{
    public string? BankTrackingNumber { get; set; }
    public string? BankReferenceId { get; set; }
    public string? Note { get; set; }
}

public class AdminWithdrawalMarkFailedRequest
{
    public string FailureReason { get; set; } = string.Empty;
    public bool ReleaseFunds { get; set; } = true;
}

public class AdminWithdrawalMarkProcessingRequest
{
    public string? Note { get; set; }
}

// ═══ Payout Record DTOs ═════════════════════════════════

public class PayoutRecordDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid WithdrawalRequestId { get; set; }
    public Guid PayoutAccountId { get; set; }
    public Guid WalletId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public decimal FeeAmount { get; set; }
    public decimal NetAmount { get; set; }
    public string PayoutMethod { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? BankTrackingNumber { get; set; }
    public string? BankReferenceId { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? FailedAt { get; set; }
    public string? FailureReason { get; set; }
    public DateTime CreatedAt { get; set; }
}

// ═══ Reports DTOs ═══════════════════════════════════════

public class WithdrawalSummaryDto
{
    public Guid? TenantId { get; set; }
    public List<WithdrawalSummaryItem> Items { get; set; } = [];
}

public class WithdrawalSummaryItem
{
    public string Currency { get; set; } = string.Empty;
    public int PendingCount { get; set; }
    public decimal PendingAmount { get; set; }
    public int ApprovedCount { get; set; }
    public decimal ApprovedAmount { get; set; }
    public int PaidCount { get; set; }
    public decimal PaidAmount { get; set; }
    public int RejectedCount { get; set; }
    public decimal RejectedAmount { get; set; }
}
