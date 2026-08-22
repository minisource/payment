using Domain.Entities;

namespace Domain.Services;

/// <summary>
/// Stateless hash provider for computing and verifying individual ledger entry hashes.
/// Separated from ILedgerHashService to break circular dependency:
/// WalletLedgerRepository depends only on this stateless provider,
/// while LedgerHashService (which depends on IWalletLedgerRepository) handles chain verification.
/// </summary>
public interface ILedgerHashProvider
{
    /// <summary>Computes the SHA256 hash for a ledger entry including the previous hash.</summary>
    string ComputeEntryHash(WalletLedgerEntry entry, string? previousHash);

    /// <summary>Verifies that an entry's stored hash matches its computed hash.</summary>
    bool VerifyEntryHash(WalletLedgerEntry entry);
}
