using Domain.Entities;

namespace Domain.Repositories;

public interface IWalletLedgerRepository
{
    Task AddAsync(WalletLedgerEntry entry, CancellationToken ct = default);
    Task<List<WalletLedgerEntry>> GetByWalletIdAsync(Guid walletAccountId, int skip = 0, int take = 50, CancellationToken ct = default);
    Task<int> CountByWalletIdAsync(Guid walletAccountId, CancellationToken ct = default);
    Task<decimal> ReplayBalanceAsync(Guid walletAccountId, CancellationToken ct = default);
    Task<List<WalletLedgerEntry>> GetByTransactionIdAsync(Guid transactionId, CancellationToken ct = default);
    Task<(List<WalletLedgerEntry> Items, int Total)> GetAllAsync(Guid? tenantId, Guid? walletId, string? entryType, string? direction, string? currency, decimal? amountMin, decimal? amountMax, string? referenceType, string? referenceId, string? dateFrom, string? dateTo, string? query, int skip, int take, CancellationToken ct = default);
}
