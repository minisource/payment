using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class SettingsRepository : ISettingsRepository
{
    private readonly PaymentDbContext _ctx;
    public SettingsRepository(PaymentDbContext ctx) => _ctx = ctx;

    public async Task<PaymentSettings?> GetAsync(CancellationToken ct = default)
        => await _ctx.PaymentSettings.FirstOrDefaultAsync(ct);

    public async Task UpsertAsync(PaymentSettings settings, CancellationToken ct = default)
    {
        var existing = await _ctx.PaymentSettings.FirstOrDefaultAsync(ct);
        if (existing == null) await _ctx.PaymentSettings.AddAsync(settings, ct);
        else { _ctx.Entry(existing).CurrentValues.SetValues(settings); }
    }

    public async Task<TenantPaymentSettings?> GetTenantSettingsAsync(Guid tenantId, CancellationToken ct = default)
        => await _ctx.TenantPaymentSettings.FirstOrDefaultAsync(s => s.TenantId == tenantId, ct);

    public async Task UpsertTenantSettingsAsync(TenantPaymentSettings settings, CancellationToken ct = default)
    {
        var existing = await _ctx.TenantPaymentSettings.FirstOrDefaultAsync(s => s.TenantId == settings.TenantId, ct);
        if (existing == null) await _ctx.TenantPaymentSettings.AddAsync(settings, ct);
        else { _ctx.Entry(existing).CurrentValues.SetValues(settings); }
    }

    public Task<List<FeeRule>> GetFeeRulesAsync(Guid tenantId, CancellationToken ct = default)
        => _ctx.FeeRules.Where(r => r.TenantId == tenantId && r.IsActive).OrderBy(r => r.Priority).ToListAsync(ct);

    public async Task AddFeeRuleAsync(FeeRule rule, CancellationToken ct = default)
        => await _ctx.FeeRules.AddAsync(rule, ct);

    public async Task<FeeRule?> GetFeeRuleByIdAsync(Guid id, CancellationToken ct = default)
        => await _ctx.FeeRules.FirstOrDefaultAsync(r => r.Id == id, ct);

    public Task UpdateFeeRuleAsync(FeeRule rule, CancellationToken ct = default)
    { _ctx.FeeRules.Update(rule); return Task.CompletedTask; }

    public Task<List<LimitRule>> GetLimitRulesAsync(Guid tenantId, CancellationToken ct = default)
        => _ctx.LimitRules.Where(r => r.TenantId == tenantId && r.IsActive).OrderBy(r => r.Priority).ToListAsync(ct);

    public async Task AddLimitRuleAsync(LimitRule rule, CancellationToken ct = default)
        => await _ctx.LimitRules.AddAsync(rule, ct);

    public async Task<LimitRule?> GetLimitRuleByIdAsync(Guid id, CancellationToken ct = default)
        => await _ctx.LimitRules.FirstOrDefaultAsync(r => r.Id == id, ct);

    public Task UpdateLimitRuleAsync(LimitRule rule, CancellationToken ct = default)
    { _ctx.LimitRules.Update(rule); return Task.CompletedTask; }

    public Task<List<RiskRule>> GetRiskRulesAsync(Guid tenantId, CancellationToken ct = default)
        => _ctx.RiskRules.Where(r => r.TenantId == tenantId && r.IsActive).OrderBy(r => r.Priority).ToListAsync(ct);

    public async Task AddRiskRuleAsync(RiskRule rule, CancellationToken ct = default)
        => await _ctx.RiskRules.AddAsync(rule, ct);
}
