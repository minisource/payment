using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class PaymentLinkRepository : IPaymentLinkRepository
{
    private readonly PaymentDbContext _ctx;
    public PaymentLinkRepository(PaymentDbContext ctx) => _ctx = ctx;

    public Task<PaymentLink?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _ctx.Set<PaymentLink>().FirstOrDefaultAsync(l => l.Id == id, ct);

    public Task<PaymentLink?> GetByTokenHashAsync(string tokenHash, CancellationToken ct = default)
        => _ctx.Set<PaymentLink>().FirstOrDefaultAsync(l => l.TokenHash == tokenHash, ct);

    public async Task AddAsync(PaymentLink link, CancellationToken ct = default)
        => await _ctx.Set<PaymentLink>().AddAsync(link, ct);

    public Task UpdateAsync(PaymentLink link, CancellationToken ct = default)
    { _ctx.Set<PaymentLink>().Update(link); return Task.CompletedTask; }

    public async Task<List<PaymentLink>> GetByOwnerAsync(Guid tenantId, string ownerType, Guid ownerId,
        int skip = 0, int take = 50, CancellationToken ct = default)
        => await _ctx.Set<PaymentLink>()
            .Where(l => l.TenantId == tenantId && l.OwnerType == ownerType && l.OwnerId == ownerId && l.DeletedAt == null)
            .OrderByDescending(l => l.CreatedAt)
            .Skip(skip).Take(take)
            .ToListAsync(ct);

    public async Task<List<PaymentLink>> GetByTenantAsync(Guid tenantId, int skip = 0, int take = 50,
        PaymentLinkStatus? status = null, string? applicationCode = null, CancellationToken ct = default)
    {
        var q = _ctx.Set<PaymentLink>().Where(l => l.TenantId == tenantId && l.DeletedAt == null);
        if (status.HasValue) q = q.Where(l => l.Status == status.Value);
        if (!string.IsNullOrEmpty(applicationCode)) q = q.Where(l => l.ApplicationCode == applicationCode);
        return await q.OrderByDescending(l => l.CreatedAt).Skip(skip).Take(take).ToListAsync(ct);
    }

    public async Task<int> CountByTenantAsync(Guid tenantId, PaymentLinkStatus? status = null, CancellationToken ct = default)
    {
        var q = _ctx.Set<PaymentLink>().Where(l => l.TenantId == tenantId && l.DeletedAt == null);
        if (status.HasValue) q = q.Where(l => l.Status == status.Value);
        return await q.CountAsync(ct);
    }

    public async Task<List<PaymentLink>> GetByRecipientWalletAsync(Guid walletId, int skip = 0, int take = 50, CancellationToken ct = default)
        => await _ctx.Set<PaymentLink>()
            .Where(l => l.RecipientWalletId == walletId && l.DeletedAt == null)
            .OrderByDescending(l => l.CreatedAt)
            .Skip(skip).Take(take)
            .ToListAsync(ct);
}
