using System.Text.Json;
using Application.DTOs;
using Domain.Entities;
using Domain.Repositories;
using Microsoft.Extensions.Logging;
using Minisource.Common.Domain;
using Minisource.Common.Exceptions;

namespace Application.Features.Settings;

public class SettingsService : ISettingsService
{
    private readonly ISettingsRepository _repo;
    private readonly IAuditLogRepository _auditRepo;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<SettingsService> _logger;

    public SettingsService(ISettingsRepository repo, IAuditLogRepository auditRepo, IUnitOfWork uow, ILogger<SettingsService> logger)
    {
        _repo = repo; _auditRepo = auditRepo; _uow = uow; _logger = logger;
    }

    public async Task<PaymentSettingsDto> GetPaymentSettingsAsync(CancellationToken ct = default)
    {
        var settings = await _repo.GetAsync(ct);
        if (settings == null)
        {
            settings = new PaymentSettings();
            await _repo.UpsertAsync(settings, ct);
            await _uow.SaveChangesAsync(ct);
        }
        return MapGlobalToDto(settings);
    }

    public async Task<PaymentSettingsDto> UpdatePaymentSettingsAsync(UpdatePaymentSettingsRequest request, CancellationToken ct = default)
    {
        var settings = await _repo.GetAsync(ct) ?? new PaymentSettings();
        if (request.SupportedCurrencies != null) settings.SupportedCurrencies = request.SupportedCurrencies;
        if (request.DefaultCurrency != null) settings.DefaultCurrency = request.DefaultCurrency;
        if (request.MoneyMaxPrecision.HasValue) settings.MoneyMaxPrecision = request.MoneyMaxPrecision.Value;
        if (request.MoneyMaxScale.HasValue) settings.MoneyMaxScale = request.MoneyMaxScale.Value;
        if (request.DefaultWalletEnabled.HasValue) settings.DefaultWalletEnabled = request.DefaultWalletEnabled.Value;

        await _repo.UpsertAsync(settings, ct);
        await _uow.SaveChangesAsync(ct);
        return MapGlobalToDto(settings);
    }

    public async Task<TenantPaymentSettingsDto> GetTenantPaymentSettingsAsync(Guid tenantId, CancellationToken ct = default)
    {
        var settings = await _repo.GetTenantSettingsAsync(tenantId, ct);
        if (settings == null)
        {
            settings = TenantPaymentSettings.CreateDefault(tenantId);
            await _repo.UpsertTenantSettingsAsync(settings, ct);
            await _uow.SaveChangesAsync(ct);
        }
        return MapTenantToDto(settings);
    }

    public async Task<TenantPaymentSettingsDto> UpdateTenantPaymentSettingsAsync(Guid tenantId, UpdateTenantPaymentSettingsRequest request, CancellationToken ct = default)
    {
        var settings = await _repo.GetTenantSettingsAsync(tenantId, ct)
            ?? TenantPaymentSettings.CreateDefault(tenantId);

        if (request.WalletEnabled.HasValue) settings.WalletEnabled = request.WalletEnabled.Value;
        if (request.ManualAdminAdjustmentsEnabled.HasValue) settings.ManualAdminAdjustmentsEnabled = request.ManualAdminAdjustmentsEnabled.Value;
        if (request.SupportedCurrencies != null) settings.SupportedCurrencies = request.SupportedCurrencies;
        if (request.DefaultCurrency != null) settings.DefaultCurrency = request.DefaultCurrency;
        if (request.MinWalletAdjustmentAmount.HasValue) settings.MinWalletAdjustmentAmount = request.MinWalletAdjustmentAmount.Value;
        if (request.MaxWalletAdjustmentAmount.HasValue) settings.MaxWalletAdjustmentAmount = request.MaxWalletAdjustmentAmount.Value;
        if (request.DailyWalletCreditLimit.HasValue) settings.DailyWalletCreditLimit = request.DailyWalletCreditLimit.Value;
        if (request.DailyWalletDebitLimit.HasValue) settings.DailyWalletDebitLimit = request.DailyWalletDebitLimit.Value;
        if (request.MinWithdrawalAmount.HasValue) settings.MinWithdrawalAmount = request.MinWithdrawalAmount.Value;
        if (request.MaxWithdrawalAmount.HasValue) settings.MaxWithdrawalAmount = request.MaxWithdrawalAmount.Value;
        if (request.WithdrawalRequiresVerifiedPayoutAccount.HasValue) settings.WithdrawalRequiresVerifiedPayoutAccount = request.WithdrawalRequiresVerifiedPayoutAccount.Value;
        if (request.WithdrawalRequiresAdminApproval.HasValue) settings.WithdrawalRequiresAdminApproval = request.WithdrawalRequiresAdminApproval.Value;
        if (request.WithdrawalAutoExpireHours.HasValue) settings.WithdrawalAutoExpireHours = request.WithdrawalAutoExpireHours.Value;
        if (request.WithdrawalDailyCountLimit.HasValue) settings.WithdrawalDailyCountLimit = request.WithdrawalDailyCountLimit.Value;
        if (request.WithdrawalMonthlyCountLimit.HasValue) settings.WithdrawalMonthlyCountLimit = request.WithdrawalMonthlyCountLimit.Value;
        if (request.WithdrawalDailyAmountLimit.HasValue) settings.WithdrawalDailyAmountLimit = request.WithdrawalDailyAmountLimit.Value;
        if (request.WithdrawalMonthlyAmountLimit.HasValue) settings.WithdrawalMonthlyAmountLimit = request.WithdrawalMonthlyAmountLimit.Value;

        await _repo.UpsertTenantSettingsAsync(settings, ct);
        await _uow.SaveChangesAsync(ct);
        return MapTenantToDto(settings);
    }

    public async Task<List<FeeRule>> GetFeeRulesAsync(Guid tenantId, CancellationToken ct = default)
        => await _repo.GetFeeRulesAsync(tenantId, ct);

    public async Task<FeeRule> CreateFeeRuleAsync(Guid tenantId, FeeRule rule, CancellationToken ct = default)
    {
        rule.TenantId = tenantId;
        await _repo.AddFeeRuleAsync(rule, ct);
        await _uow.SaveChangesAsync(ct);
        return rule;
    }

    public async Task<List<LimitRule>> GetLimitRulesAsync(Guid tenantId, CancellationToken ct = default)
        => await _repo.GetLimitRulesAsync(tenantId, ct);

    public async Task<LimitRule> CreateLimitRuleAsync(Guid tenantId, LimitRule rule, CancellationToken ct = default)
    {
        rule.TenantId = tenantId;
        await _repo.AddLimitRuleAsync(rule, ct);
        await _uow.SaveChangesAsync(ct);
        return rule;
    }

    public async Task<List<RiskRule>> GetRiskRulesAsync(Guid tenantId, CancellationToken ct = default)
        => await _repo.GetRiskRulesAsync(tenantId, ct);

    public async Task<RiskRule> CreateRiskRuleAsync(Guid tenantId, RiskRule rule, CancellationToken ct = default)
    {
        rule.TenantId = tenantId;
        await _repo.AddRiskRuleAsync(rule, ct);
        await _uow.SaveChangesAsync(ct);
        return rule;
    }

    private static PaymentSettingsDto MapGlobalToDto(PaymentSettings s) => new()
    {
        Id = s.Id, Environment = s.Environment, SupportedCurrencies = s.SupportedCurrencies,
        DefaultCurrency = s.DefaultCurrency, MoneyMaxPrecision = s.MoneyMaxPrecision,
        MoneyMaxScale = s.MoneyMaxScale, DefaultWalletEnabled = s.DefaultWalletEnabled
    };

    private static TenantPaymentSettingsDto MapTenantToDto(TenantPaymentSettings s) => new()
    {
        Id = s.Id, TenantId = s.TenantId, WalletEnabled = s.WalletEnabled,
        GatewayEnabled = s.GatewayEnabled, WithdrawalsEnabled = s.WithdrawalsEnabled,
        ManualAdminAdjustmentsEnabled = s.ManualAdminAdjustmentsEnabled,
        DefaultCurrency = s.DefaultCurrency, SupportedCurrencies = s.SupportedCurrencies,
        MinWalletAdjustmentAmount = s.MinWalletAdjustmentAmount,
        MaxWalletAdjustmentAmount = s.MaxWalletAdjustmentAmount,
        DailyWalletCreditLimit = s.DailyWalletCreditLimit,
        DailyWalletDebitLimit = s.DailyWalletDebitLimit,
        KycRequiredForWithdrawal = s.KycRequiredForWithdrawal,
        AmlChecksEnabled = s.AmlChecksEnabled,
        MinWithdrawalAmount = s.MinWithdrawalAmount,
        MaxWithdrawalAmount = s.MaxWithdrawalAmount,
        WithdrawalRequiresVerifiedPayoutAccount = s.WithdrawalRequiresVerifiedPayoutAccount,
        WithdrawalRequiresAdminApproval = s.WithdrawalRequiresAdminApproval,
        WithdrawalAutoExpireHours = s.WithdrawalAutoExpireHours,
        WithdrawalDailyCountLimit = s.WithdrawalDailyCountLimit,
        WithdrawalMonthlyCountLimit = s.WithdrawalMonthlyCountLimit,
        WithdrawalDailyAmountLimit = s.WithdrawalDailyAmountLimit,
        WithdrawalMonthlyAmountLimit = s.WithdrawalMonthlyAmountLimit
    };
}
