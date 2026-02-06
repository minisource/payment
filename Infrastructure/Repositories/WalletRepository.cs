using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Wallet aggregate.
/// </summary>
public class WalletRepository : IWalletRepository
{
    private readonly PaymentDbContext _context;

    public WalletRepository(PaymentDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<Wallet?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Wallets
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<Wallet?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _context.Wallets
            .FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<Wallet?> GetByUserIdWithTransactionsAsync(
        string userId,
        int? transactionLimit = null,
        CancellationToken cancellationToken = default)
    {
        var wallet = await _context.Wallets
            .FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken);

        if (wallet != null)
        {
            // Load transactions separately with optional limit
            var transactionsQuery = _context.WalletTransactions
                .Where(t => t.WalletId == wallet.Id)
                .OrderByDescending(t => t.CreatedAt);

            if (transactionLimit.HasValue)
            {
                await transactionsQuery
                    .Take(transactionLimit.Value)
                    .LoadAsync(cancellationToken);
            }
            else
            {
                await transactionsQuery.LoadAsync(cancellationToken);
            }
        }

        return wallet;
    }

    /// <inheritdoc/>
    public async Task<Wallet?> GetByUserIdForUpdateAsync(string userId, CancellationToken cancellationToken = default)
    {
        // Use FOR UPDATE hint for pessimistic locking in PostgreSQL
        return await _context.Wallets
            .FromSqlRaw("SELECT * FROM \"Wallets\" WHERE \"UserId\" = {0} FOR UPDATE", userId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task AddAsync(Wallet wallet, CancellationToken cancellationToken = default)
    {
        await _context.Wallets.AddAsync(wallet, cancellationToken);
    }

    /// <inheritdoc/>
    public Task UpdateAsync(Wallet wallet, CancellationToken cancellationToken = default)
    {
        _context.Wallets.Update(wallet);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public async Task UpdateBalanceAsync(Guid walletId, decimal balance, CancellationToken cancellationToken = default)
    {
        await _context.Wallets
            .Where(w => w.Id == walletId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(w => w.Balance, balance)
                .SetProperty(w => w.UpdatedAt, DateTime.UtcNow),
                cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<bool> ExistsAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _context.Wallets
            .AnyAsync(w => w.UserId == userId, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<decimal> CalculateBalanceFromTransactionsAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var wallet = await _context.Wallets
            .FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken);

        if (wallet == null)
        {
            return 0;
        }

        var credits = await _context.WalletTransactions
            .Where(t => t.WalletId == wallet.Id && !t.IsReversed && t.Type == WalletTransactionType.Credit)
            .SumAsync(t => t.Amount, cancellationToken);

        var debits = await _context.WalletTransactions
            .Where(t => t.WalletId == wallet.Id && !t.IsReversed && t.Type == WalletTransactionType.Debit)
            .SumAsync(t => t.Amount, cancellationToken);

        return credits - debits;
    }
}
