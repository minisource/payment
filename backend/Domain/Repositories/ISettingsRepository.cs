using Domain.Entities;

namespace Domain.Repositories;

public interface ISettingsRepository
{
    Task<PaymentSettings?> GetAsync(CancellationToken ct = default);
    Task UpsertAsync(PaymentSettings settings, CancellationToken ct = default);
    Task<TenantPaymentSettings?> GetTenantSettingsAsync(Guid tenantId, CancellationToken ct = default);
    Task UpsertTenantSettingsAsync(TenantPaymentSettings settings, CancellationToken ct = default);
    Task<List<FeeRule>> GetFeeRulesAsync(Guid tenantId, CancellationToken ct = default);
    Task AddFeeRuleAsync(FeeRule rule, CancellationToken ct = default);
    Task<FeeRule?> GetFeeRuleByIdAsync(Guid id, CancellationToken ct = default);
    Task UpdateFeeRuleAsync(FeeRule rule, CancellationToken ct = default);
    Task<List<LimitRule>> GetLimitRulesAsync(Guid tenantId, CancellationToken ct = default);
    Task AddLimitRuleAsync(LimitRule rule, CancellationToken ct = default);
    Task<LimitRule?> GetLimitRuleByIdAsync(Guid id, CancellationToken ct = default);
    Task UpdateLimitRuleAsync(LimitRule rule, CancellationToken ct = default);
    Task<List<RiskRule>> GetRiskRulesAsync(Guid tenantId, CancellationToken ct = default);
    Task AddRiskRuleAsync(RiskRule rule, CancellationToken ct = default);
}
