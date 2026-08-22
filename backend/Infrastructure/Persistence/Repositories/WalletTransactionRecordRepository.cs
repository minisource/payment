using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class WalletTransactionRecordRepository : IWalletTransactionRecordRepository
{
    private readonly PaymentDbContext _ctx;
    public WalletTransactionRecordRepository(PaymentDbContext ctx) => _ctx = ctx;

    public async Task<WalletTransactionRecord?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _ctx.WalletTransactionRecords.FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<WalletTransactionRecord?> GetByIdempotencyKeyAsync(Guid tenantId, string idempotencyKey, CancellationToken ct = default)
        => await _ctx.WalletTransactionRecords.FirstOrDefaultAsync(t => t.TenantId == tenantId && t.IdempotencyKey == idempotencyKey, ct);

    public async Task AddAsync(WalletTransactionRecord record, CancellationToken ct = default)
        => await _ctx.WalletTransactionRecords.AddAsync(record, ct);

    public Task UpdateAsync(WalletTransactionRecord record, CancellationToken ct = default)
    {
        _ctx.WalletTransactionRecords.Update(record);
        return Task.CompletedTask;
    }

    public async Task<List<WalletTransactionRecord>> GetByTenantAsync(Guid tenantId, int skip = 0, int take = 50, string? transactionType = null, Guid? walletId = null, CancellationToken ct = default)
    {
        var q = _ctx.WalletTransactionRecords.Where(t => t.TenantId == tenantId);
        if (transactionType != null) q = q.Where(t => t.TransactionType == transactionType);
        if (walletId.HasValue) q = q.Where(t => t.SourceWalletId == walletId || t.DestinationWalletId == walletId);
        return await q.OrderByDescending(t => t.CreatedAt).Skip(skip).Take(take).ToListAsync(ct);
    }

    public async Task<int> CountByTenantAsync(Guid tenantId, string? transactionType = null, Guid? walletId = null, CancellationToken ct = default)
    {
        var q = _ctx.WalletTransactionRecords.Where(t => t.TenantId == tenantId);
        if (transactionType != null) q = q.Where(t => t.TransactionType == transactionType);
        if (walletId.HasValue) q = q.Where(t => t.SourceWalletId == walletId || t.DestinationWalletId == walletId);
        return await q.CountAsync(ct);
    }
}
