using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class PayoutRepositories :
    IPayoutAccountRepository,
    IWithdrawalRequestRepository,
    IPayoutRecordRepository
{
    private readonly PaymentDbContext _ctx;
    public PayoutRepositories(PaymentDbContext ctx) => _ctx = ctx;

    // ═══ PayoutAccountRepository ═══
    Task<PayoutAccount?> IPayoutAccountRepository.GetByIdAsync(Guid id, CancellationToken ct)
        => _ctx.Set<PayoutAccount>().FirstOrDefaultAsync(a => a.Id == id, ct);

    async Task<List<PayoutAccount>> IPayoutAccountRepository.GetByOwnerAsync(Guid tenantId, string ownerType, Guid ownerId, int skip, int take, CancellationToken ct)
        => await _ctx.Set<PayoutAccount>()
            .Where(a => a.TenantId == tenantId && a.OwnerType == ownerType && a.OwnerId == ownerId)
            .OrderByDescending(a => a.CreatedAt).Skip(skip).Take(take).ToListAsync(ct);

    async Task<List<PayoutAccount>> IPayoutAccountRepository.GetByTenantAsync(Guid tenantId, PayoutAccountStatus? status, int skip, int take, CancellationToken ct)
    {
        var q = _ctx.Set<PayoutAccount>().Where(a => a.TenantId == tenantId);
        if (status.HasValue) q = q.Where(a => a.Status == status.Value);
        return await q.OrderByDescending(a => a.CreatedAt).Skip(skip).Take(take).ToListAsync(ct);
    }

    Task<PayoutAccount?> IPayoutAccountRepository.GetByIbanHashAsync(Guid tenantId, string ibanHash, CancellationToken ct)
        => _ctx.Set<PayoutAccount>().FirstOrDefaultAsync(a => a.TenantId == tenantId && a.IbanHash == ibanHash && a.DeletedAt == null, ct);

    Task<PayoutAccount?> IPayoutAccountRepository.GetByCardHashAsync(Guid tenantId, string cardHash, CancellationToken ct)
        => _ctx.Set<PayoutAccount>().FirstOrDefaultAsync(a => a.TenantId == tenantId && a.CardNumberHash == cardHash && a.DeletedAt == null, ct);

    async Task IPayoutAccountRepository.AddAsync(PayoutAccount account, CancellationToken ct)
        => await _ctx.Set<PayoutAccount>().AddAsync(account, ct);

    Task IPayoutAccountRepository.UpdateAsync(PayoutAccount account, CancellationToken ct)
    { _ctx.Set<PayoutAccount>().Update(account); return Task.CompletedTask; }

    // ═══ WithdrawalRequestRepository ═══
    Task<WithdrawalRequest?> IWithdrawalRequestRepository.GetByIdAsync(Guid id, CancellationToken ct)
        => _ctx.Set<WithdrawalRequest>().Include(w => w.PayoutAccount).FirstOrDefaultAsync(w => w.Id == id, ct);

    Task<WithdrawalRequest?> IWithdrawalRequestRepository.GetByIdempotencyKeyAsync(Guid tenantId, string idempotencyKey, CancellationToken ct)
        => _ctx.Set<WithdrawalRequest>().FirstOrDefaultAsync(w => w.TenantId == tenantId && w.IdempotencyKey == idempotencyKey, ct);

    async Task<List<WithdrawalRequest>> IWithdrawalRequestRepository.GetByOwnerAsync(Guid tenantId, string ownerType, Guid ownerId, int skip, int take, CancellationToken ct)
        => await _ctx.Set<WithdrawalRequest>()
            .Where(w => w.TenantId == tenantId && w.OwnerType == ownerType && w.OwnerId == ownerId)
            .OrderByDescending(w => w.CreatedAt).Skip(skip).Take(take).ToListAsync(ct);

    async Task<List<WithdrawalRequest>> IWithdrawalRequestRepository.GetByTenantAsync(Guid tenantId, WithdrawalStatus? status, int skip, int take, CancellationToken ct)
    {
        var q = _ctx.Set<WithdrawalRequest>().Where(w => w.TenantId == tenantId);
        if (status.HasValue) q = q.Where(w => w.Status == status.Value);
        return await q.OrderByDescending(w => w.CreatedAt).Skip(skip).Take(take).ToListAsync(ct);
    }

    async Task<List<WithdrawalRequest>> IWithdrawalRequestRepository.GetByWalletAsync(Guid walletId, int skip, int take, CancellationToken ct)
        => await _ctx.Set<WithdrawalRequest>().Where(w => w.WalletId == walletId)
            .OrderByDescending(w => w.CreatedAt).Skip(skip).Take(take).ToListAsync(ct);

    async Task<int> IWithdrawalRequestRepository.CountByTenantAsync(Guid tenantId, WithdrawalStatus? status, CancellationToken ct)
    {
        var q = _ctx.Set<WithdrawalRequest>().Where(w => w.TenantId == tenantId);
        if (status.HasValue) q = q.Where(w => w.Status == status.Value);
        return await q.CountAsync(ct);
    }

    async Task IWithdrawalRequestRepository.AddAsync(WithdrawalRequest request, CancellationToken ct)
        => await _ctx.Set<WithdrawalRequest>().AddAsync(request, ct);

    Task IWithdrawalRequestRepository.UpdateAsync(WithdrawalRequest request, CancellationToken ct)
    { _ctx.Set<WithdrawalRequest>().Update(request); return Task.CompletedTask; }

    // ═══ PayoutRecordRepository ═══
    Task<PayoutRecord?> IPayoutRecordRepository.GetByIdAsync(Guid id, CancellationToken ct)
        => _ctx.Set<PayoutRecord>().FirstOrDefaultAsync(p => p.Id == id, ct);

    Task<PayoutRecord?> IPayoutRecordRepository.GetByWithdrawalIdAsync(Guid withdrawalRequestId, CancellationToken ct)
        => _ctx.Set<PayoutRecord>().FirstOrDefaultAsync(p => p.WithdrawalRequestId == withdrawalRequestId, ct);

    async Task<List<PayoutRecord>> IPayoutRecordRepository.GetByTenantAsync(Guid tenantId, PayoutRecordStatus? status, int skip, int take, CancellationToken ct)
    {
        var q = _ctx.Set<PayoutRecord>().Where(p => p.TenantId == tenantId);
        if (status.HasValue) q = q.Where(p => p.Status == status.Value);
        return await q.OrderByDescending(p => p.CreatedAt).Skip(skip).Take(take).ToListAsync(ct);
    }

    async Task IPayoutRecordRepository.AddAsync(PayoutRecord record, CancellationToken ct)
        => await _ctx.Set<PayoutRecord>().AddAsync(record, ct);

    Task IPayoutRecordRepository.UpdateAsync(PayoutRecord record, CancellationToken ct)
    { _ctx.Set<PayoutRecord>().Update(record); return Task.CompletedTask; }
}
