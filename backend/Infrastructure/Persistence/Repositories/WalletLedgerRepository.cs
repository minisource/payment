using Domain.Services;
using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class WalletLedgerRepository : IWalletLedgerRepository
{
    private readonly PaymentDbContext _ctx;
    private readonly ILedgerHashProvider _hashProvider;
    public WalletLedgerRepository(PaymentDbContext ctx, ILedgerHashProvider hashProvider)
    { _ctx = ctx; _hashProvider = hashProvider; }

    public async Task AddAsync(WalletLedgerEntry entry, CancellationToken ct = default)
    {
        // Auto-compute hash chain for tamper evidence
        if (string.IsNullOrEmpty(entry.EntryHash))
        {
            var previousEntry = await _ctx.WalletLedgerEntries
                .Where(e => e.WalletAccountId == entry.WalletAccountId && !string.IsNullOrEmpty(e.EntryHash))
                .OrderByDescending(e => e.CreatedAt)
                .Select(e => e.EntryHash)
                .FirstOrDefaultAsync(ct);
            entry.PreviousEntryHash = previousEntry;
            entry.EntryHash = _hashProvider.ComputeEntryHash(entry, previousEntry);
            entry.HashAlgorithm = "SHA256";
            entry.HashVersion = 1;
        }
        await _ctx.WalletLedgerEntries.AddAsync(entry, ct);
    }

    public async Task<List<WalletLedgerEntry>> GetByWalletIdAsync(Guid walletAccountId, int skip = 0, int take = 50, CancellationToken ct = default)
        => await _ctx.WalletLedgerEntries
            .Where(e => e.WalletAccountId == walletAccountId)
            .OrderByDescending(e => e.CreatedAt)
            .Skip(skip).Take(take).ToListAsync(ct);

    public async Task<int> CountByWalletIdAsync(Guid walletAccountId, CancellationToken ct = default)
        => await _ctx.WalletLedgerEntries.CountAsync(e => e.WalletAccountId == walletAccountId, ct);

    public async Task<decimal> ReplayBalanceAsync(Guid walletAccountId, CancellationToken ct = default)
    {
        var credits = await _ctx.WalletLedgerEntries
            .Where(e => e.WalletAccountId == walletAccountId && e.Direction == LedgerDirection.Credit)
            .SumAsync(e => e.Amount, ct);
        var debits = await _ctx.WalletLedgerEntries
            .Where(e => e.WalletAccountId == walletAccountId && e.Direction == LedgerDirection.Debit)
            .SumAsync(e => e.Amount, ct);
        return credits - debits;
    }

    public async Task<List<WalletLedgerEntry>> GetByTransactionIdAsync(Guid transactionId, CancellationToken ct = default)
        => await _ctx.WalletLedgerEntries
            .Where(e => e.WalletTransactionId == transactionId)
            .OrderBy(e => e.CreatedAt).ToListAsync(ct);

    public async Task<(List<WalletLedgerEntry> Items, int Total)> GetAllAsync(
        Guid? tenantId, Guid? walletId, string? entryType, string? direction, string? currency,
        decimal? amountMin, decimal? amountMax, string? referenceType, string? referenceId,
        string? dateFrom, string? dateTo, string? query, int skip, int take, CancellationToken ct = default)
    {
        var q = _ctx.WalletLedgerEntries.AsQueryable();

        if (tenantId.HasValue)
            q = q.Where(e => e.TenantId == tenantId.Value);

        if (walletId.HasValue)
            q = q.Where(e => e.WalletAccountId == walletId.Value);

        if (!string.IsNullOrEmpty(entryType) && Enum.TryParse<LedgerEntryType>(entryType, true, out var et))
            q = q.Where(e => e.EntryType == et);

        if (!string.IsNullOrEmpty(direction) && Enum.TryParse<LedgerDirection>(direction, true, out var dir))
            q = q.Where(e => e.Direction == dir);

        if (!string.IsNullOrEmpty(currency))
            q = q.Where(e => e.Currency == currency);

        if (amountMin.HasValue)
            q = q.Where(e => e.Amount >= amountMin.Value);

        if (amountMax.HasValue)
            q = q.Where(e => e.Amount <= amountMax.Value);

        if (!string.IsNullOrEmpty(referenceType))
            q = q.Where(e => e.ReferenceType == referenceType);

        if (!string.IsNullOrEmpty(referenceId))
            q = q.Where(e => e.ReferenceId == referenceId);

        if (!string.IsNullOrEmpty(dateFrom) && DateTime.TryParse(dateFrom, out var from))
            q = q.Where(e => e.CreatedAt >= from);

        if (!string.IsNullOrEmpty(dateTo) && DateTime.TryParse(dateTo, out var to))
            q = q.Where(e => e.CreatedAt < to);

        if (!string.IsNullOrEmpty(query))
            q = q.Where(e => e.Reason != null && e.Reason.Contains(query)
                || e.ReferenceType != null && e.ReferenceType.Contains(query)
                || e.ReferenceId != null && e.ReferenceId.Contains(query));

        var total = await q.CountAsync(ct);
        var items = await q.OrderByDescending(e => e.CreatedAt).Skip(skip).Take(take).ToListAsync(ct);
        return (items, total);
    }
}
