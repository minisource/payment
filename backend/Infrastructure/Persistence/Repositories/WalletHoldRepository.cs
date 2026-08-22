using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class WalletHoldRepository : IWalletHoldRepository
{
    private readonly PaymentDbContext _ctx;
    public WalletHoldRepository(PaymentDbContext ctx) => _ctx = ctx;

    public async Task<WalletHold?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _ctx.WalletHolds.FirstOrDefaultAsync(h => h.Id == id, ct);

    public async Task<WalletHold?> GetByIdForUpdateAsync(Guid id, CancellationToken ct = default)
        => await _ctx.WalletHolds
            .FromSqlRaw("SELECT * FROM [WalletHolds] WITH (UPDLOCK, ROWLOCK) WHERE [Id] = {0}", id)
            .FirstOrDefaultAsync(ct);

    public async Task<WalletHold?> GetActiveByReferenceAsync(string referenceType, Guid referenceId, CancellationToken ct = default)
        => await _ctx.WalletHolds.FirstOrDefaultAsync(h =>
            h.ReferenceType == referenceType && h.ReferenceId == referenceId
            && h.Status == WalletHoldStatus.Active && h.DeletedAt == null, ct);

    public async Task<List<WalletHold>> GetByWalletAccountAsync(Guid walletAccountId, WalletHoldStatus? status = null, CancellationToken ct = default)
    {
        var q = _ctx.WalletHolds.Where(h => h.WalletAccountId == walletAccountId && h.DeletedAt == null);
        if (status.HasValue) q = q.Where(h => h.Status == status.Value);
        return await q.OrderByDescending(h => h.CreatedAt).ToListAsync(ct);
    }

    public async Task<decimal> GetActiveHoldSumAsync(Guid walletAccountId, CancellationToken ct = default)
        => await _ctx.WalletHolds
            .Where(h => h.WalletAccountId == walletAccountId
                && h.Status == WalletHoldStatus.Active && h.DeletedAt == null)
            .SumAsync(h => h.Amount, ct);

    public async Task AddAsync(WalletHold hold, CancellationToken ct = default)
        => await _ctx.WalletHolds.AddAsync(hold, ct);

    public Task UpdateAsync(WalletHold hold, CancellationToken ct = default)
    {
        _ctx.WalletHolds.Update(hold);
        return Task.CompletedTask;
    }
}
