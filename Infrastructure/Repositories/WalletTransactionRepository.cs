using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

/// <summary>
/// Repository implementation for WalletTransaction entity.
/// </summary>
public class WalletTransactionRepository : IWalletTransactionRepository
{
    private readonly PaymentDbContext _context;

    public WalletTransactionRepository(PaymentDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task AddAsync(WalletTransaction transaction, CancellationToken cancellationToken = default)
    {
        await _context.WalletTransactions.AddAsync(transaction, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<List<WalletTransaction>> GetByWalletIdAsync(
        Guid walletId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        return await _context.WalletTransactions
            .Where(t => t.WalletId == walletId)
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<List<WalletTransaction>> GetByWalletIdAndTypeAsync(
        Guid walletId,
        WalletTransactionType type,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        return await _context.WalletTransactions
            .Where(t => t.WalletId == walletId && t.Type == type)
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<List<WalletTransaction>> GetByReferenceAsync(
        string referenceId,
        string referenceType,
        CancellationToken cancellationToken = default)
    {
        return await _context.WalletTransactions
            .Where(t => t.ReferenceId == referenceId && t.ReferenceType == referenceType)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<int> GetTotalCountAsync(Guid walletId, CancellationToken cancellationToken = default)
    {
        return await _context.WalletTransactions
            .Where(t => t.WalletId == walletId)
            .CountAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<int> GetTotalCountByWalletIdAsync(Guid walletId, CancellationToken cancellationToken = default)
    {
        return await _context.WalletTransactions
            .Where(t => t.WalletId == walletId)
            .CountAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<decimal> GetTotalCreditsAsync(Guid walletId, CancellationToken cancellationToken = default)
    {
        return await _context.WalletTransactions
            .Where(t => t.WalletId == walletId && t.Type == WalletTransactionType.Credit && !t.IsReversed)
            .SumAsync(t => t.Amount, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<decimal> GetTotalDebitsAsync(Guid walletId, CancellationToken cancellationToken = default)
    {
        return await _context.WalletTransactions
            .Where(t => t.WalletId == walletId && t.Type == WalletTransactionType.Debit && !t.IsReversed)
            .SumAsync(t => t.Amount, cancellationToken);
    }
}
