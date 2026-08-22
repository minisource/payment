using Domain.Entities;

namespace Domain.Services;

/// <summary>
/// Full ledger hash service for chain verification and backfilling.
/// Extends ILedgerHashProvider with stateful chain operations.
/// The stateless ILedgerHashProvider is used by repositories to avoid circular dependencies.
/// </summary>
public interface ILedgerHashService : ILedgerHashProvider
{
    /// <summary>Verifies the entire hash chain for a wallet's ledger entries.</summary>
    Task<(bool valid, List<string> errors)> VerifyWalletHashChainAsync(Guid walletId, CancellationToken ct);

    /// <summary>Backfills missing hashes for a wallet's ledger entries.</summary>
    Task<int> BackfillWalletHashChainAsync(Guid walletId, CancellationToken ct);
}
