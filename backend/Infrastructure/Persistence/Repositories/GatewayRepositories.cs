using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class GatewayRepositories :
    IPaymentIntentRepository,
    IPaymentTransactionRepository,
    IGatewayProviderRepository,
    IGatewayConfigRepository,
    IGatewayRoutingPolicyRepository,
    IGatewayRoutingRuleRepository
{
    private readonly PaymentDbContext _ctx;
    public GatewayRepositories(PaymentDbContext ctx) => _ctx = ctx;

    // ═══ Payment Intents ═══════════════════════════

    async Task<PaymentIntent?> IPaymentIntentRepository.GetByIdAsync(Guid id, CancellationToken ct)
        => await _ctx.Set<PaymentIntent>().FirstOrDefaultAsync(p => p.Id == id, ct);

    async Task<List<PaymentIntent>> IPaymentIntentRepository.GetByTenantAsync(Guid tenantId, int skip, int take,
        Domain.Enums.PaymentIntentStatus? status, string? applicationCode, Guid? payerUserId, CancellationToken ct)
    {
        var q = _ctx.Set<PaymentIntent>().Where(p => p.TenantId == tenantId);
        if (status.HasValue) q = q.Where(p => p.Status == status);
        if (!string.IsNullOrEmpty(applicationCode)) q = q.Where(p => p.ApplicationCode == applicationCode);
        if (payerUserId.HasValue) q = q.Where(p => p.PayerUserId == payerUserId);
        return await q.OrderByDescending(p => p.CreatedAt).Skip(skip).Take(take).ToListAsync(ct);
    }

    async Task<int> IPaymentIntentRepository.CountByTenantAsync(Guid tenantId, Domain.Enums.PaymentIntentStatus? status,
        string? applicationCode, CancellationToken ct)
    {
        var q = _ctx.Set<PaymentIntent>().Where(p => p.TenantId == tenantId);
        if (status.HasValue) q = q.Where(p => p.Status == status);
        if (!string.IsNullOrEmpty(applicationCode)) q = q.Where(p => p.ApplicationCode == applicationCode);
        return await q.CountAsync(ct);
    }

    async Task<PaymentIntent?> IPaymentIntentRepository.GetByIdWithTransactionsAsync(Guid id, CancellationToken ct)
        => await _ctx.Set<PaymentIntent>().Include(p => p.Transactions).FirstOrDefaultAsync(p => p.Id == id, ct);

    Task<PaymentIntent?> IPaymentIntentRepository.GetByIdempotencyKeyAsync(Guid tenantId, string idempotencyKey, CancellationToken ct)
        => _ctx.Set<PaymentIntent>().FirstOrDefaultAsync(p =>
            p.TenantId == tenantId && p.IdempotencyKey == idempotencyKey, ct);

    async Task IPaymentIntentRepository.AddAsync(PaymentIntent intent, CancellationToken ct)
        => await _ctx.Set<PaymentIntent>().AddAsync(intent, ct);

    Task IPaymentIntentRepository.UpdateAsync(PaymentIntent intent, CancellationToken ct)
    { _ctx.Set<PaymentIntent>().Update(intent); return Task.CompletedTask; }

    Task<List<PaymentIntent>> IPaymentIntentRepository.GetByExternalReferenceAsync(Guid tenantId, string referenceType, string referenceId, CancellationToken ct)
        => _ctx.Set<PaymentIntent>().Where(p =>
            p.TenantId == tenantId && p.ExternalReferenceType == referenceType && p.ExternalReferenceId == referenceId).ToListAsync(ct);

    // ═══ Payment Transactions ══════════════════════

    async Task<PaymentTransaction?> IPaymentTransactionRepository.GetByIdAsync(Guid id, CancellationToken ct)
        => await _ctx.Set<PaymentTransaction>().Include(t => t.PaymentIntent).FirstOrDefaultAsync(t => t.Id == id, ct);

    Task<PaymentTransaction?> IPaymentTransactionRepository.GetByAuthorityAsync(string authority, CancellationToken ct)
        => _ctx.Set<PaymentTransaction>().FirstOrDefaultAsync(t => t.Authority == authority, ct);

    async Task IPaymentTransactionRepository.AddAsync(PaymentTransaction transaction, CancellationToken ct)
        => await _ctx.Set<PaymentTransaction>().AddAsync(transaction, ct);

    Task IPaymentTransactionRepository.UpdateAsync(PaymentTransaction transaction, CancellationToken ct)
    { _ctx.Set<PaymentTransaction>().Update(transaction); return Task.CompletedTask; }

    Task<List<PaymentTransaction>> IPaymentTransactionRepository.GetByIntentIdAsync(Guid paymentIntentId, CancellationToken ct)
        => _ctx.Set<PaymentTransaction>().Where(t => t.PaymentIntentId == paymentIntentId)
            .OrderByDescending(t => t.CreatedAt).ToListAsync(ct);

    async Task<List<PaymentTransaction>> IPaymentTransactionRepository.GetByTenantAsync(Guid tenantId, int skip, int take,
        Domain.Enums.PaymentTransactionStatus? status, string? providerCode, DateTime? from, DateTime? to, CancellationToken ct)
    {
        var q = _ctx.Set<PaymentTransaction>().Where(t => t.TenantId == tenantId);
        if (status.HasValue) q = q.Where(t => t.Status == status);
        if (!string.IsNullOrEmpty(providerCode)) q = q.Where(t => t.ProviderCode == providerCode);
        if (from.HasValue) q = q.Where(t => t.CreatedAt >= from);
        if (to.HasValue) q = q.Where(t => t.CreatedAt <= to);
        return await q.OrderByDescending(t => t.CreatedAt).Skip(skip).Take(take).ToListAsync(ct);
    }

    async Task<int> IPaymentTransactionRepository.CountByTenantAsync(Guid tenantId, Domain.Enums.PaymentTransactionStatus? status, CancellationToken ct)
    {
        var q = _ctx.Set<PaymentTransaction>().Where(t => t.TenantId == tenantId);
        if (status.HasValue) q = q.Where(t => t.Status == status);
        return await q.CountAsync(ct);
    }

    Task<PaymentTransaction?> IPaymentTransactionRepository.GetLatestForIntentAsync(Guid paymentIntentId, CancellationToken ct)
        => _ctx.Set<PaymentTransaction>().Where(t => t.PaymentIntentId == paymentIntentId)
            .OrderByDescending(t => t.CreatedAt).FirstOrDefaultAsync(ct);

    // ═══ Gateway Providers ═════════════════════════

    Task<GatewayProvider?> IGatewayProviderRepository.GetByCodeAsync(string code, CancellationToken ct)
        => _ctx.Set<GatewayProvider>().FirstOrDefaultAsync(p => p.Code == code, ct);

    Task<List<GatewayProvider>> IGatewayProviderRepository.GetAllAsync(CancellationToken ct)
        => _ctx.Set<GatewayProvider>().OrderBy(p => p.Code).ToListAsync(ct);

    async Task IGatewayProviderRepository.AddAsync(GatewayProvider provider, CancellationToken ct)
        => await _ctx.Set<GatewayProvider>().AddAsync(provider, ct);

    Task IGatewayProviderRepository.UpdateAsync(GatewayProvider provider, CancellationToken ct)
    { _ctx.Set<GatewayProvider>().Update(provider); return Task.CompletedTask; }

    // ═══ Gateway Configs ════════════════════════════

    Task<GatewayConfig?> IGatewayConfigRepository.GetByIdAsync(Guid id, CancellationToken ct)
        => _ctx.Set<GatewayConfig>().FirstOrDefaultAsync(c => c.Id == id, ct);

    async Task<List<GatewayConfig>> IGatewayConfigRepository.GetActiveAsync(Guid? tenantId, string? applicationCode, CancellationToken ct)
    {
        var q = _ctx.Set<GatewayConfig>().Where(c =>
            c.Status == Domain.Enums.GatewayConfigStatus.Active && c.DeletedAt == null);
        var configs = await q.ToListAsync(ct);
        return configs.Where(c =>
            (tenantId == null && applicationCode == null)
            || (tenantId.HasValue && c.TenantId == tenantId)
            || (applicationCode != null && c.ApplicationCode == applicationCode)
            || (c.TenantId == null && c.ApplicationCode == null)
        ).OrderBy(c => c.TenantId.HasValue ? 0 : 1).ThenBy(c => c.Priority).ToList();
    }

    async Task<List<GatewayConfig>> IGatewayConfigRepository.GetAllForAdminAsync(Guid? tenantId, string? applicationCode,
        int skip, int take, CancellationToken ct)
    {
        var q = _ctx.Set<GatewayConfig>().AsQueryable();
        if (tenantId.HasValue) q = q.Where(c => c.TenantId == tenantId || c.TenantId == null);
        if (!string.IsNullOrEmpty(applicationCode)) q = q.Where(c => c.ApplicationCode == applicationCode || c.ApplicationCode == null);
        return await q.Where(c => c.DeletedAt == null).OrderByDescending(c => c.CreatedAt).Skip(skip).Take(take).ToListAsync(ct);
    }

    async Task IGatewayConfigRepository.AddAsync(GatewayConfig config, CancellationToken ct)
        => await _ctx.Set<GatewayConfig>().AddAsync(config, ct);

    Task IGatewayConfigRepository.UpdateAsync(GatewayConfig config, CancellationToken ct)
    { _ctx.Set<GatewayConfig>().Update(config); return Task.CompletedTask; }

    async Task<int> IGatewayConfigRepository.CountAsync(Guid? tenantId, string? applicationCode, CancellationToken ct)
    {
        var q = _ctx.Set<GatewayConfig>().AsQueryable();
        if (tenantId.HasValue) q = q.Where(c => c.TenantId == tenantId);
        if (!string.IsNullOrEmpty(applicationCode)) q = q.Where(c => c.ApplicationCode == applicationCode);
        return await q.CountAsync(ct);
    }

    // ═══ Routing Policies ══════════════════════════

    async Task<GatewayRoutingPolicy?> IGatewayRoutingPolicyRepository.GetByIdAsync(Guid id, CancellationToken ct)
        => await _ctx.Set<GatewayRoutingPolicy>().Include(p => p.Rules).FirstOrDefaultAsync(p => p.Id == id, ct);

    async Task<GatewayRoutingPolicy?> IGatewayRoutingPolicyRepository.GetByIdWithRulesAsync(Guid id, CancellationToken ct)
        => await _ctx.Set<GatewayRoutingPolicy>().Include(p => p.Rules).FirstOrDefaultAsync(p => p.Id == id, ct);

    Task<GatewayRoutingPolicy?> IGatewayRoutingPolicyRepository.GetActiveForScopeAsync(Guid? tenantId, string? applicationCode, CancellationToken ct)
    {
        var q = _ctx.Set<GatewayRoutingPolicy>()
            .Include(p => p.Rules.Where(r => r.DeletedAt == null && r.Status == "active")).AsQueryable();
        if (tenantId.HasValue && !string.IsNullOrEmpty(applicationCode))
            return q.FirstOrDefaultAsync(p => p.TenantId == tenantId && p.ApplicationCode == applicationCode && p.IsActive, ct);
        if (tenantId.HasValue)
            return q.FirstOrDefaultAsync(p => p.TenantId == tenantId && p.ApplicationCode == null && p.IsActive, ct);
        if (!string.IsNullOrEmpty(applicationCode))
            return q.FirstOrDefaultAsync(p => p.TenantId == null && p.ApplicationCode == applicationCode && p.IsActive, ct);
        return q.FirstOrDefaultAsync(p => p.TenantId == null && p.ApplicationCode == null && p.IsActive, ct);
    }

    async Task<List<GatewayRoutingPolicy>> IGatewayRoutingPolicyRepository.GetAllAsync(Guid? tenantId, string? applicationCode, CancellationToken ct)
    {
        var q = _ctx.Set<GatewayRoutingPolicy>().AsQueryable();
        if (tenantId.HasValue) q = q.Where(p => p.TenantId == tenantId || p.TenantId == null);
        if (!string.IsNullOrEmpty(applicationCode)) q = q.Where(p => p.ApplicationCode == applicationCode || p.ApplicationCode == null);
        return await q.Where(p => p.DeletedAt == null).Include(p => p.Rules)
            .OrderByDescending(p => p.CreatedAt).ToListAsync(ct);
    }

    async Task IGatewayRoutingPolicyRepository.AddAsync(GatewayRoutingPolicy policy, CancellationToken ct)
        => await _ctx.Set<GatewayRoutingPolicy>().AddAsync(policy, ct);

    Task IGatewayRoutingPolicyRepository.UpdateAsync(GatewayRoutingPolicy policy, CancellationToken ct)
    { _ctx.Set<GatewayRoutingPolicy>().Update(policy); return Task.CompletedTask; }

    // ═══ Routing Rules ═════════════════════════════

    Task<GatewayRoutingRule?> IGatewayRoutingRuleRepository.GetByIdAsync(Guid id, CancellationToken ct)
        => _ctx.Set<GatewayRoutingRule>().FirstOrDefaultAsync(r => r.Id == id, ct);

    Task<List<GatewayRoutingRule>> IGatewayRoutingRuleRepository.GetByPolicyIdAsync(Guid policyId, CancellationToken ct)
        => _ctx.Set<GatewayRoutingRule>().Where(r => r.PolicyId == policyId && r.DeletedAt == null)
            .OrderBy(r => r.Priority).ToListAsync(ct);

    async Task IGatewayRoutingRuleRepository.AddAsync(GatewayRoutingRule rule, CancellationToken ct)
        => await _ctx.Set<GatewayRoutingRule>().AddAsync(rule, ct);

    Task IGatewayRoutingRuleRepository.UpdateAsync(GatewayRoutingRule rule, CancellationToken ct)
    { _ctx.Set<GatewayRoutingRule>().Update(rule); return Task.CompletedTask; }
}
