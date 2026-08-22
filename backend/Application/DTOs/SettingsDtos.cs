namespace Application.DTOs;

public class PaymentSettingsDto
{
    public Guid Id { get; set; }
    public string Environment { get; set; } = string.Empty;
    public List<string> SupportedCurrencies { get; set; } = [];
    public string DefaultCurrency { get; set; } = string.Empty;
    public int MoneyMaxPrecision { get; set; }
    public int MoneyMaxScale { get; set; }
    public bool DefaultWalletEnabled { get; set; }
}

public class UpdatePaymentSettingsRequest
{
    public List<string>? SupportedCurrencies { get; set; }
    public string? DefaultCurrency { get; set; }
    public int? MoneyMaxPrecision { get; set; }
    public int? MoneyMaxScale { get; set; }
    public bool? DefaultWalletEnabled { get; set; }
}

public class TenantPaymentSettingsDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public bool WalletEnabled { get; set; }
    public bool GatewayEnabled { get; set; }
    public bool WithdrawalsEnabled { get; set; }
    public bool ManualAdminAdjustmentsEnabled { get; set; }
    public string DefaultCurrency { get; set; } = string.Empty;
    public List<string> SupportedCurrencies { get; set; } = [];
    public decimal? MinWalletAdjustmentAmount { get; set; }
    public decimal? MaxWalletAdjustmentAmount { get; set; }
    public decimal? DailyWalletCreditLimit { get; set; }
    public decimal? DailyWalletDebitLimit { get; set; }
    public bool KycRequiredForWithdrawal { get; set; }
    public bool AmlChecksEnabled { get; set; }
    public decimal? MinWithdrawalAmount { get; set; }
    public decimal? MaxWithdrawalAmount { get; set; }
    public bool WithdrawalRequiresVerifiedPayoutAccount { get; set; }
    public bool WithdrawalRequiresAdminApproval { get; set; }
    public int? WithdrawalAutoExpireHours { get; set; }
    public int? WithdrawalDailyCountLimit { get; set; }
    public int? WithdrawalMonthlyCountLimit { get; set; }
    public decimal? WithdrawalDailyAmountLimit { get; set; }
    public decimal? WithdrawalMonthlyAmountLimit { get; set; }
}

public class UpdateTenantPaymentSettingsRequest
{
    public bool? WalletEnabled { get; set; }
    public bool? ManualAdminAdjustmentsEnabled { get; set; }
    public List<string>? SupportedCurrencies { get; set; }
    public string? DefaultCurrency { get; set; }
    public decimal? MinWalletAdjustmentAmount { get; set; }
    public decimal? MaxWalletAdjustmentAmount { get; set; }
    public decimal? DailyWalletCreditLimit { get; set; }
    public decimal? DailyWalletDebitLimit { get; set; }
    public decimal? MinWithdrawalAmount { get; set; }
    public decimal? MaxWithdrawalAmount { get; set; }
    public bool? WithdrawalRequiresVerifiedPayoutAccount { get; set; }
    public bool? WithdrawalRequiresAdminApproval { get; set; }
    public int? WithdrawalAutoExpireHours { get; set; }
    public int? WithdrawalDailyCountLimit { get; set; }
    public int? WithdrawalMonthlyCountLimit { get; set; }
    public decimal? WithdrawalDailyAmountLimit { get; set; }
    public decimal? WithdrawalMonthlyAmountLimit { get; set; }
}

public class TenantLiabilityDto
{
    public Guid TenantId { get; set; }
    public List<TenantLiabilityItem> Items { get; set; } = [];
}

public class TenantLiabilityItem
{
    public string Currency { get; set; } = string.Empty;
    public decimal AvailableTotal { get; set; }
    public decimal LockedTotal { get; set; }
    public decimal PendingTotal { get; set; }
    public decimal TotalLiability { get; set; }
    public int WalletCount { get; set; }
}

public class WalletActivityReportDto
{
    public Guid TenantId { get; set; }
    public string Currency { get; set; } = string.Empty;
    public decimal TotalCredits { get; set; }
    public decimal TotalDebits { get; set; }
    public int TransactionCount { get; set; }
    public DateTime From { get; set; }
    public DateTime To { get; set; }
}

public class WalletConsistencyResult
{
    public bool Ok { get; set; }
    public DateTime CheckedAt { get; set; }
    public string Scope { get; set; } = string.Empty;
    public string ScopeId { get; set; } = string.Empty;
    public List<WalletConsistencyError> Errors { get; set; } = [];
    public List<string> Warnings { get; set; } = [];
}

public class WalletConsistencyError
{
    public string Code { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
