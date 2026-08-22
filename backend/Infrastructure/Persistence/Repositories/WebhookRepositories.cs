using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class WebhookSubscriptionRepository : IWebhookSubscriptionRepository
{
    private readonly PaymentDbContext _ctx;
    public WebhookSubscriptionRepository(PaymentDbContext ctx) => _ctx = ctx;

    public async Task<WebhookSubscription?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _ctx.WebhookSubscriptions.FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task<List<WebhookSubscription>> ListAsync(Guid? tenantId, string? applicationCode, string? status,
        int skip, int take, CancellationToken ct = default)
    {
        var q = _ctx.WebhookSubscriptions.AsQueryable();
        if (tenantId.HasValue) q = q.Where(s => s.TenantId == tenantId.Value);
        if (!string.IsNullOrEmpty(applicationCode)) q = q.Where(s => s.ApplicationCode == applicationCode);
        if (!string.IsNullOrEmpty(status)) q = q.Where(s => s.Status == status);
        return await q.OrderByDescending(s => s.CreatedAt).Skip(skip).Take(take).ToListAsync(ct);
    }

    public async Task<int> CountAsync(Guid? tenantId, string? applicationCode, string? status, CancellationToken ct = default)
    {
        var q = _ctx.WebhookSubscriptions.AsQueryable();
        if (tenantId.HasValue) q = q.Where(s => s.TenantId == tenantId.Value);
        if (!string.IsNullOrEmpty(applicationCode)) q = q.Where(s => s.ApplicationCode == applicationCode);
        if (!string.IsNullOrEmpty(status)) q = q.Where(s => s.Status == status);
        return await q.CountAsync(ct);
    }

    public async Task<int> CountActiveAsync(Guid? tenantId, CancellationToken ct = default)
    {
        var q = _ctx.WebhookSubscriptions.Where(s => s.Status == "active" && s.DeletedAt == null);
        if (tenantId.HasValue) q = q.Where(s => s.TenantId == tenantId.Value);
        return await q.CountAsync(ct);
    }

    public async Task AddAsync(WebhookSubscription sub, CancellationToken ct = default)
        => await _ctx.WebhookSubscriptions.AddAsync(sub, ct);
}

public class WebhookDeliveryRepository : IWebhookDeliveryRepository
{
    private readonly PaymentDbContext _ctx;
    public WebhookDeliveryRepository(PaymentDbContext ctx) => _ctx = ctx;

    public async Task<WebhookDelivery?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _ctx.WebhookDeliveries.FirstOrDefaultAsync(d => d.Id == id, ct);

    public async Task<List<WebhookDelivery>> ListAsync(Guid? tenantId, Guid? subscriptionId, string? status,
        int skip, int take, CancellationToken ct = default)
    {
        var q = _ctx.WebhookDeliveries.AsQueryable();
        if (tenantId.HasValue) q = q.Where(d => d.TenantId == tenantId.Value);
        if (subscriptionId.HasValue) q = q.Where(d => d.WebhookSubscriptionId == subscriptionId.Value);
        if (!string.IsNullOrEmpty(status)) q = q.Where(d => d.Status == status);
        return await q.OrderByDescending(d => d.CreatedAt).Skip(skip).Take(take).ToListAsync(ct);
    }

    public async Task<int> CountAsync(Guid? tenantId, Guid? subscriptionId, string? status, CancellationToken ct = default)
    {
        var q = _ctx.WebhookDeliveries.AsQueryable();
        if (tenantId.HasValue) q = q.Where(d => d.TenantId == tenantId.Value);
        if (subscriptionId.HasValue) q = q.Where(d => d.WebhookSubscriptionId == subscriptionId.Value);
        if (!string.IsNullOrEmpty(status)) q = q.Where(d => d.Status == status);
        return await q.CountAsync(ct);
    }
}
