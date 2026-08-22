using Minisource.Common.Domain;

namespace Domain.Entities;

/// <summary>
/// Global payment service settings.
/// </summary>
public class PaymentSettings : Entity<Guid>
{
    public string Environment { get; set; } = "production";
    public List<string> SupportedCurrencies { get; set; } = ["IRT"];
    public string DefaultCurrency { get; set; } = "IRT";
    public int MoneyMaxPrecision { get; set; } = 30;
    public int MoneyMaxScale { get; set; } = 10;
    public bool DefaultWalletEnabled { get; set; } = true;
    public string? Metadata { get; set; }

    public PaymentSettings()
    {
        Id = Guid.NewGuid();
    }
}

/// <summary>
/// Tenant-specific payment settings.
/// </summary>
public class TenantPaymentSettings : Entity<Guid>
{
    public Guid TenantId { get; set; }
    public bool WalletEnabled { get; set; } = true;
    public bool GatewayEnabled { get; set; }
    public bool WithdrawalsEnabled { get; set; }
    public bool ManualAdminAdjustmentsEnabled { get; set; } = true;
    public string DefaultCurrency { get; set; } = "IRT";
    public List<string> SupportedCurrencies { get; set; } = ["IRT"];
    public decimal? MinWalletAdjustmentAmount { get; set; }
    public decimal? MaxWalletAdjustmentAmount { get; set; }
    public decimal? DailyWalletCreditLimit { get; set; }
    public decimal? DailyWalletDebitLimit { get; set; }
    public bool KycRequiredForWithdrawal { get; set; } = true;
    public bool AmlChecksEnabled { get; set; }
    public decimal? MinWithdrawalAmount { get; set; }
    public decimal? MaxWithdrawalAmount { get; set; }
    public bool WithdrawalRequiresVerifiedPayoutAccount { get; set; } = true;
    public bool WithdrawalRequiresAdminApproval { get; set; } = true;
    public int? WithdrawalAutoExpireHours { get; set; }
    public int? WithdrawalDailyCountLimit { get; set; }
    public int? WithdrawalMonthlyCountLimit { get; set; }
    public decimal? WithdrawalDailyAmountLimit { get; set; }
    public decimal? WithdrawalMonthlyAmountLimit { get; set; }
    public string? Metadata { get; set; }

    public TenantPaymentSettings()
    {
        Id = Guid.NewGuid();
    }

    public static TenantPaymentSettings CreateDefault(Guid tenantId, string currency = "IRT")
    {
        return new TenantPaymentSettings
        {
            TenantId = tenantId,
            DefaultCurrency = currency,
            SupportedCurrencies = [currency]
        };
    }
}

/// <summary>
/// Fee rule for payment operations.
/// </summary>
public class FeeRule : Entity<Guid>
{
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string OperationType { get; set; } = string.Empty;
    public string Currency { get; set; } = "IRT";
    public string FeeType { get; set; } = "none";
    public decimal? FixedAmount { get; set; }
    public decimal? PercentRate { get; set; }
    public decimal? MinFee { get; set; }
    public decimal? MaxFee { get; set; }
    public string Payer { get; set; } = "platform";
    public int Priority { get; set; } = 100;
    public string Status { get; set; } = "active";
    public string? Metadata { get; set; }
    public DateTime? DeletedAt { get; set; }

    public FeeRule() { Id = Guid.NewGuid(); }
    public bool IsActive => Status == "active" && DeletedAt == null;
}

/// <summary>
/// Limit rule for payment operations.
/// </summary>
public class LimitRule : Entity<Guid>
{
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string OperationType { get; set; } = string.Empty;
    public string Currency { get; set; } = "IRT";
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public decimal? DailyAmountLimit { get; set; }
    public decimal? MonthlyAmountLimit { get; set; }
    public int? DailyCountLimit { get; set; }
    public int? MonthlyCountLimit { get; set; }
    public bool RequiresKyc { get; set; }
    public int Priority { get; set; } = 100;
    public string Status { get; set; } = "active";
    public string? Metadata { get; set; }
    public DateTime? DeletedAt { get; set; }

    public LimitRule() { Id = Guid.NewGuid(); }
    public bool IsActive => Status == "active" && DeletedAt == null;
}

/// <summary>
/// Risk rule for payment operations.
/// </summary>
public class RiskRule : Entity<Guid>
{
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string OperationType { get; set; } = string.Empty;
    public string RuleType { get; set; } = string.Empty;
    public string Action { get; set; } = "flag";
    public decimal? ThresholdAmount { get; set; }
    public string? Currency { get; set; }
    public string? Metadata { get; set; }
    public int Priority { get; set; } = 100;
    public string Status { get; set; } = "active";
    public DateTime? DeletedAt { get; set; }

    public RiskRule() { Id = Guid.NewGuid(); }
    public bool IsActive => Status == "active" && DeletedAt == null;
}
