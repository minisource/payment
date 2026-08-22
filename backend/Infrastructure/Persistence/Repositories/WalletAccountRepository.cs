using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class WalletAccountRepository : IWalletAccountRepository
{
    private readonly PaymentDbContext _ctx;
    public WalletAccountRepository(PaymentDbContext ctx) => _ctx = ctx;

    public async Task<WalletAccount?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _ctx.WalletAccounts.FirstOrDefaultAsync(w => w.Id == id, ct);

    public async Task<WalletAccount?> GetByOwnerAsync(Guid tenantId, string ownerType, Guid ownerId, string currency, CancellationToken ct = default)
        => await _ctx.WalletAccounts.FirstOrDefaultAsync(w =>
            w.TenantId == tenantId && w.OwnerType == ownerType && w.OwnerId == ownerId && w.Currency == currency, ct);

    public async Task<WalletAccount?> GetByIdForUpdateAsync(Guid id, CancellationToken ct = default)
        => await _ctx.WalletAccounts
            .FromSqlRaw("SELECT * FROM [WalletAccounts] WITH (UPDLOCK, ROWLOCK) WHERE [Id] = {0}", id)
            .FirstOrDefaultAsync(ct);

    public async Task<List<WalletAccount>> GetByTenantAsync(Guid tenantId, WalletAccountStatus? status = null, int skip = 0, int take = 50, CancellationToken ct = default)
    {
        var q = _ctx.WalletAccounts.Where(w => w.TenantId == tenantId);
        if (status.HasValue) q = q.Where(w => w.Status == status.Value);
        return await q.OrderByDescending(w => w.UpdatedAt).Skip(skip).Take(take).ToListAsync(ct);
    }

    public async Task<List<WalletAccount>> GetByOwnerAsync(Guid tenantId, string ownerType, Guid ownerId, CancellationToken ct = default)
        => await _ctx.WalletAccounts.Where(w =>
            w.TenantId == tenantId && w.OwnerType == ownerType && w.OwnerId == ownerId).ToListAsync(ct);

    public async Task AddAsync(WalletAccount account, CancellationToken ct = default)
        => await _ctx.WalletAccounts.AddAsync(account, ct);

    public Task UpdateAsync(WalletAccount account, CancellationToken ct = default)
    {
        _ctx.WalletAccounts.Update(account);
        return Task.CompletedTask;
    }

    public async Task<int> CountByTenantAsync(Guid tenantId, CancellationToken ct = default)
        => await _ctx.WalletAccounts.CountAsync(w => w.TenantId == tenantId, ct);

    public async Task<Dictionary<string, (decimal available, decimal locked, decimal pending, int count)>> GetTenantLiabilityAsync(Guid tenantId, CancellationToken ct = default)
    {
        var groups = await _ctx.WalletAccounts
            .Where(w => w.TenantId == tenantId && w.Status == WalletAccountStatus.Active)
            .GroupBy(w => w.Currency)
            .Select(g => new { Currency = g.Key, Available = g.Sum(w => w.AvailableBalance), Locked = g.Sum(w => w.LockedBalance), Pending = g.Sum(w => w.PendingBalance), Count = g.Count() })
            .ToListAsync(ct);

        return groups.ToDictionary(g => g.Currency, g => (g.Available, g.Locked, g.Pending, g.Count));
    }
}
