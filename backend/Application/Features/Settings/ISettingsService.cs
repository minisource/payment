using Application.DTOs;
using Domain.Entities;

namespace Application.Features.Settings;

/// <summary>
/// Service for managing payment settings (global, tenant, fee rules, limit rules, risk rules).
/// </summary>
public interface ISettingsService
{
    Task<PaymentSettingsDto> GetPaymentSettingsAsync(CancellationToken ct);
    Task<PaymentSettingsDto> UpdatePaymentSettingsAsync(UpdatePaymentSettingsRequest request, CancellationToken ct);
    Task<TenantPaymentSettingsDto> GetTenantPaymentSettingsAsync(Guid tenantId, CancellationToken ct);
    Task<TenantPaymentSettingsDto> UpdateTenantPaymentSettingsAsync(Guid tenantId, UpdateTenantPaymentSettingsRequest request, CancellationToken ct);
    Task<List<FeeRule>> GetFeeRulesAsync(Guid tenantId, CancellationToken ct);
    Task<List<LimitRule>> GetLimitRulesAsync(Guid tenantId, CancellationToken ct);
    Task<List<RiskRule>> GetRiskRulesAsync(Guid tenantId, CancellationToken ct);
}
