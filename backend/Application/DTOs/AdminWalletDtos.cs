namespace Application.DTOs;

public class AdminCreditWalletRequest
{
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "IRT";
    public string Reason { get; set; } = string.Empty;
    public string? ReferenceType { get; set; }
    public string? ReferenceId { get; set; }
    public string? Metadata { get; set; }
}

public class AdminDebitWalletRequest
{
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "IRT";
    public string Reason { get; set; } = string.Empty;
    public bool AllowNegative { get; set; }
    public string? OverrideReason { get; set; }
    public string? ReferenceType { get; set; }
    public string? ReferenceId { get; set; }
    public string? Metadata { get; set; }
}

public class AdminAdjustmentResponse
{
    public Guid WalletTransactionId { get; set; }
    public Guid WalletId { get; set; }
    public Guid EntryId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public decimal AvailableBalanceAfter { get; set; }
    public decimal LockedBalanceAfter { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class WalletAccountDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string OwnerType { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
    public string Currency { get; set; } = string.Empty;
    public decimal AvailableBalance { get; set; }
    public decimal LockedBalance { get; set; }
    public decimal PendingBalance { get; set; }
    public string Status { get; set; } = string.Empty;
    public long Version { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class LedgerEntryDto
{
    public Guid Id { get; set; }
    public Guid WalletAccountId { get; set; }
    public Guid? WalletTransactionId { get; set; }
    public string EntryType { get; set; } = string.Empty;
    public string Direction { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public decimal BalanceAvailableBefore { get; set; }
    public decimal BalanceAvailableAfter { get; set; }
    public string? Reason { get; set; }
    public string? ReferenceType { get; set; }
    public string? ReferenceId { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class WalletTransactionRecordDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public Guid? SourceWalletId { get; set; }
    public Guid? DestinationWalletId { get; set; }
    public string? ReferenceType { get; set; }
    public string? ReferenceId { get; set; }
    public string? IdempotencyKey { get; set; }
    public string? Description { get; set; }
    public string? Reason { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PostedAt { get; set; }
    public DateTime? ReversedAt { get; set; }
}
