namespace Domain.Enums;

public enum PaymentIntentStatus
{
    Created = 1,
    GatewaySelected = 2,
    RedirectRequired = 3,
    Processing = 4,
    Succeeded = 5,
    Failed = 6,
    Cancelled = 7,
    Expired = 8
}

public enum WalletBehavior
{
    None = 0,
    CreditRecipientWallet = 1,
    CreditPayerWallet = 2,
    CreditPayerThenTransferToRecipient = 3
}

public enum PaymentIntentPurpose
{
    WalletTopup = 1,
    PublicPayment = 2,
    SettlementPayment = 3,
    InvoicePayment = 4,
    OrderPayment = 5,
    AdminCreated = 6
}

public enum GatewayProviderStatus
{
    Active = 1,
    Disabled = 2,
    Maintenance = 3,
    Deprecated = 4
}

public enum GatewayConfigStatus
{
    Active = 1,
    Disabled = 2,
    Maintenance = 3,
    Test = 4,
    Deleted = 5
}

public enum GatewayConfigScope
{
    Global = 1,
    Tenant = 2,
    Application = 3,
    TenantApplication = 4
}

public enum GatewayHealthStatus
{
    Healthy = 1,
    Degraded = 2,
    Unhealthy = 3,
    Unknown = 4
}

public enum GatewayRoutingStrategy
{
    Priority = 1,
    Random = 2,
    WeightedRandom = 3,
    RoundRobin = 4,
    LeastFailed = 5,
    ManualProvider = 6
}

public enum PaymentTransactionStatus
{
    Created = 1,
    SentToGateway = 2,
    CallbackReceived = 3,
    VerifyPending = 4,
    Verified = 5,
    Failed = 6,
    Cancelled = 7,
    Expired = 8,
    Unknown = 9
}

public enum WalletPostingStatus
{
    NotRequired = 1,
    Pending = 2,
    Posted = 3,
    Failed = 4
}
