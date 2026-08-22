namespace Domain.Enums;

    public enum LedgerEntryType
{
    AdminCredit = 1,
    AdminDebit = 2,
    Adjustment = 3,
    Reversal = 4,
    Fee = 5,
    WalletTransfer = 6,
    GatewayDeposit = 7,
    Withdrawal = 8,
    WithdrawalLock = 9,
    WithdrawalRelease = 10,
    WithdrawalCapture = 11,
    WithdrawalFee = 12,
    Refund = 13,
    RefundHold = 14,
    RefundHoldRelease = 15,
    RefundHoldCapture = 16
}

public enum LedgerDirection
{
    Credit = 1,
    Debit = 2,
    Lock = 3,
    Unlock = 4
}

public enum WalletTransactionStatus
{
    Pending = 1,
    Posted = 2,
    Failed = 3,
    Cancelled = 4,
    Reversed = 5
}

public enum FeeType
{
    None = 0,
    Fixed = 1,
    Percent = 2,
    FixedPlusPercent = 3
}

public enum FeePayer
{
    Platform = 1,
    Sender = 2,
    Receiver = 3
}

public enum RiskAction
{
    Allow = 1,
    Flag = 2,
    RequireReview = 3,
    Block = 4
}
