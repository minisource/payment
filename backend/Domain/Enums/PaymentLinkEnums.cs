namespace Domain.Enums;

public enum PaymentLinkStatus
{
    Active = 1,
    Paused = 2,
    Expired = 3,
    Disabled = 4,
    Deleted = 5
}

public enum PaymentLinkAmountType
{
    Fixed = 1,
    Open = 2,
    Suggested = 3
}
