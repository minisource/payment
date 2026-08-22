using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class RefundRequestRepository : IRefundRequestRepository
{
    private readonly PaymentDbContext _ctx;
    public RefundRequestRepository(PaymentDbContext ctx) => _ctx = ctx;

    public async Task<RefundRequest?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _ctx.RefundRequests.Include(r => r.Attempts).FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<RefundRequest?> GetByIdForUpdateAsync(Guid id, CancellationToken ct = default)
        => await _ctx.RefundRequests
            .FromSqlRaw("SELECT * FROM [RefundRequests] WITH (UPDLOCK, ROWLOCK) WHERE [Id] = {0}", id)
            .FirstOrDefaultAsync(ct);

    public async Task<RefundRequest?> GetByIdempotencyKeyAsync(Guid tenantId, string key, CancellationToken ct = default)
        => await _ctx.RefundRequests.FirstOrDefaultAsync(r =>
            r.TenantId == tenantId && r.IdempotencyKey == key, ct);

    public async Task<List<RefundRequest>> GetByPaymentTransactionAsync(Guid transactionId, CancellationToken ct = default)
        => await _ctx.RefundRequests.Where(r => r.PaymentTransactionId == transactionId)
            .OrderByDescending(r => r.CreatedAt).ToListAsync(ct);

    public async Task<(List<RefundRequest> Items, int Total)> ListAsync(
        Guid? tenantId,
        RefundRequestStatus? status = null,
        Guid? paymentTransactionId = null,
        Guid? walletId = null,
        string? providerCode = null,
        string? currency = null,
        DateTime? from = null,
        DateTime? to = null,
        string? query = null,
        string? applicationCode = null,
        int skip = 0,
        int take = 50,
        CancellationToken ct = default)
    {
        var q = _ctx.RefundRequests.AsQueryable();
        if (tenantId.HasValue) q = q.Where(r => r.TenantId == tenantId.Value);
        if (status.HasValue) q = q.Where(r => r.Status == status.Value);
        if (paymentTransactionId.HasValue) q = q.Where(r => r.PaymentTransactionId == paymentTransactionId.Value);
        if (walletId.HasValue) q = q.Where(r => r.WalletId == walletId.Value);
        if (!string.IsNullOrWhiteSpace(providerCode)) q = q.Where(r => r.ProviderCode == providerCode);
        if (!string.IsNullOrWhiteSpace(currency)) q = q.Where(r => r.Currency == currency);
        if (!string.IsNullOrWhiteSpace(applicationCode)) q = q.Where(r => r.ApplicationCode == applicationCode);
        if (from.HasValue) q = q.Where(r => r.CreatedAt >= from.Value);
        if (to.HasValue) q = q.Where(r => r.CreatedAt <= to.Value);
        if (!string.IsNullOrWhiteSpace(query))
            q = q.Where(r => r.Reason.Contains(query) || r.GatewayName!.Contains(query));

        var total = await q.CountAsync(ct);
        var items = await q.OrderByDescending(r => r.CreatedAt).Skip(skip).Take(take).ToListAsync(ct);
        return (items, total);
    }

    public async Task AddAsync(RefundRequest refund, CancellationToken ct = default)
        => await _ctx.RefundRequests.AddAsync(refund, ct);

    public Task UpdateAsync(RefundRequest refund, CancellationToken ct = default)
    {
        _ctx.RefundRequests.Update(refund);
        return Task.CompletedTask;
    }

    public async Task<decimal> GetTotalRefundedAsync(Guid transactionId, CancellationToken ct = default)
        => await _ctx.RefundRequests
            .Where(r => r.PaymentTransactionId == transactionId
                && r.Status == RefundRequestStatus.Completed)
            .SumAsync(r => r.Amount, ct);
}
