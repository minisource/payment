using Application.DTOs;
using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using Domain.Services;
using Microsoft.Extensions.Logging;
using Minisource.Common.Exceptions;

namespace Application.Features.Wallets;

public class WalletConsistencyService : IWalletConsistencyService
{
    private readonly IWalletAccountRepository _walletRepo;
    private readonly IWalletLedgerRepository _ledgerRepo;
    private readonly ILedgerHashService _hashService;
    private readonly ILogger<WalletConsistencyService> _logger;

    public WalletConsistencyService(
        IWalletAccountRepository walletRepo,
        IWalletLedgerRepository ledgerRepo,
        ILedgerHashService hashService,
        ILogger<WalletConsistencyService> logger)
    {
        _walletRepo = walletRepo;
        _ledgerRepo = ledgerRepo;
        _hashService = hashService;
        _logger = logger;
    }

    public async Task<WalletConsistencyResult> CheckWalletAsync(Guid walletId, CancellationToken ct = default)
    {
        var wallet = await _walletRepo.GetByIdAsync(walletId, ct);
        if (wallet == null)
            throw new NotFoundException("WalletAccount", walletId.ToString());

        var result = new WalletConsistencyResult
        {
            CheckedAt = DateTime.UtcNow, Scope = "wallet", ScopeId = walletId.ToString(), Ok = true
        };

        // 1. Balance replay check
        var replayedBalance = await _ledgerRepo.ReplayBalanceAsync(walletId, ct);
        if (replayedBalance != wallet.AvailableBalance)
        {
            result.Ok = false;
            result.Errors.Add(new WalletConsistencyError
            {
                Code = "wallet_balance_mismatch",
                EntityType = "WalletAccount", EntityId = walletId.ToString(),
                Message = $"Balance mismatch: stored={wallet.AvailableBalance}, replayed={replayedBalance}"
            });
        }

        // 2. Hash chain validation
        var (hashValid, hashErrors) = await _hashService.VerifyWalletHashChainAsync(walletId, ct);
        if (!hashValid)
        {
            result.Ok = false;
            result.Errors.AddRange(hashErrors.Select(e => new WalletConsistencyError
            {
                Code = "ledger_hash_chain_broken",
                EntityType = "WalletLedgerEntry", EntityId = walletId.ToString(),
                Message = e
            }));
        }

        // 3. Ordering & gap detection
        var entries = await _ledgerRepo.GetByWalletIdAsync(walletId, 0, int.MaxValue, ct);
        entries = entries.OrderBy(e => e.CreatedAt).ThenBy(e => e.Id).ToList();
        CheckOrderingAndGaps(entries, result);

        // 4. Balance continuity (balance_before of next == balance_after of previous)
        CheckBalanceContinuity(entries, result);

        // 5. Duplicate ledger detection
        CheckDuplicateLedgerEntries(entries, result);

        // 6. Lock/release/capture invariants for withdrawals
        CheckWithdrawalInvariants(entries, result);

        return result;
    }

    public async Task<WalletConsistencyResult> CheckTenantWalletsAsync(Guid tenantId, CancellationToken ct = default)
    {
        var wallets = await _walletRepo.GetByTenantAsync(tenantId, ct: ct);
        var result = new WalletConsistencyResult
        {
            CheckedAt = DateTime.UtcNow, Scope = "tenant", ScopeId = tenantId.ToString(), Ok = true
        };

        foreach (var wallet in wallets)
        {
            var walletResult = await CheckWalletAsync(wallet.Id, ct);
            if (!walletResult.Ok)
            {
                result.Ok = false;
                result.Errors.AddRange(walletResult.Errors);
            }
        }

        return result;
    }

    // ─── Private check methods ──────────────────────────────

    private static void CheckOrderingAndGaps(List<WalletLedgerEntry> entries, WalletConsistencyResult result)
    {
        DateTime? previousCreatedAt = null;

        for (int i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];

            // Check for chronological ordering violations
            if (previousCreatedAt.HasValue && entry.CreatedAt < previousCreatedAt.Value)
            {
                result.Ok = false;
                result.Errors.Add(new WalletConsistencyError
                {
                    Code = "ledger_ordering_violation",
                    EntityType = "WalletLedgerEntry", EntityId = entry.Id.ToString(),
                    Message = $"Entry {entry.Id} created at {entry.CreatedAt:O} is before previous entry at {previousCreatedAt:O}"
                });
            }

            previousCreatedAt = entry.CreatedAt;
        }
    }

    private static void CheckBalanceContinuity(List<WalletLedgerEntry> entries, WalletConsistencyResult result)
    {
        for (int i = 1; i < entries.Count; i++)
        {
            var prev = entries[i - 1];
            var curr = entries[i];

            if (prev.BalanceAvailableAfter != curr.BalanceAvailableBefore)
            {
                result.Ok = false;
                result.Errors.Add(new WalletConsistencyError
                {
                    Code = "ledger_balance_continuity_broken",
                    EntityType = "WalletLedgerEntry", EntityId = curr.Id.ToString(),
                    Message = $"Balance continuity broken at {curr.Id}: " +
                        $"prev.after={prev.BalanceAvailableAfter}, curr.before={curr.BalanceAvailableBefore}"
                });
            }

            if (prev.BalanceLockedAfter != curr.BalanceLockedBefore)
            {
                result.Ok = false;
                result.Errors.Add(new WalletConsistencyError
                {
                    Code = "ledger_locked_continuity_broken",
                    EntityType = "WalletLedgerEntry", EntityId = curr.Id.ToString(),
                    Message = $"Locked balance continuity broken at {curr.Id}"
                });
            }

            if (prev.BalancePendingAfter != curr.BalancePendingBefore)
            {
                result.Ok = false;
                result.Errors.Add(new WalletConsistencyError
                {
                    Code = "ledger_pending_continuity_broken",
                    EntityType = "WalletLedgerEntry", EntityId = curr.Id.ToString(),
                    Message = $"Pending balance continuity broken at {curr.Id}"
                });
            }
        }
    }

    private static void CheckDuplicateLedgerEntries(List<WalletLedgerEntry> entries, WalletConsistencyResult result)
    {
        var seen = new HashSet<string>();
        foreach (var entry in entries)
        {
            // Use reference tuple as duplicate key (same txn, same type, same amount, same timestamp)
            var key = $"{entry.WalletTransactionId}|{(int)entry.EntryType}|{(int)entry.Direction}|{entry.Amount}|{entry.CreatedAt:O}";
            if (!seen.Add(key))
            {
                result.Ok = false;
                result.Errors.Add(new WalletConsistencyError
                {
                    Code = "ledger_duplicate_entry",
                    EntityType = "WalletLedgerEntry", EntityId = entry.Id.ToString(),
                    Message = $"Duplicate ledger entry: txn={entry.WalletTransactionId}, type={entry.EntryType}, amount={entry.Amount}"
                });
            }
        }
    }

    private static void CheckWithdrawalInvariants(List<WalletLedgerEntry> entries, WalletConsistencyResult result)
    {
        // Track lock/release/capture pairs for withdrawal entries
        var lockedAmounts = new Dictionary<Guid, decimal>(); // txnId → locked amount

        foreach (var entry in entries)
        {
            if (entry.WalletTransactionId == null) continue;
            var txnId = entry.WalletTransactionId.Value;

            switch (entry.EntryType)
            {
                case LedgerEntryType.WithdrawalLock:
                    // Available should decrease, locked should increase
                    if (entry.BalanceAvailableAfter >= entry.BalanceAvailableBefore && entry.Amount > 0)
                    {
                        result.Ok = false;
                        result.Errors.Add(new WalletConsistencyError
                        {
                            Code = "withdrawal_lock_invariant",
                            EntityType = "WalletLedgerEntry", EntityId = entry.Id.ToString(),
                            Message = $"WithdrawalLock at {entry.Id}: available balance did not decrease"
                        });
                    }
                    lockedAmounts[txnId] = entry.Amount;
                    break;

                case LedgerEntryType.WithdrawalRelease:
                    // Available should increase, locked should decrease
                    if (entry.BalanceAvailableAfter <= entry.BalanceAvailableBefore && entry.Amount > 0)
                    {
                        result.Ok = false;
                        result.Errors.Add(new WalletConsistencyError
                        {
                            Code = "withdrawal_release_invariant",
                            EntityType = "WalletLedgerEntry", EntityId = entry.Id.ToString(),
                            Message = $"WithdrawalRelease at {entry.Id}: available balance did not increase"
                        });
                    }
                    // Verify release amount matches lock amount
                    if (lockedAmounts.TryGetValue(txnId, out var lockedAmt) && entry.Amount != lockedAmt)
                    {
                        result.Ok = false;
                        result.Errors.Add(new WalletConsistencyError
                        {
                            Code = "withdrawal_release_amount_mismatch",
                            EntityType = "WalletLedgerEntry", EntityId = entry.Id.ToString(),
                            Message = $"WithdrawalRelease at {entry.Id}: release amount {entry.Amount} != lock amount {lockedAmt}"
                        });
                    }
                    lockedAmounts.Remove(txnId);
                    break;

                case LedgerEntryType.WithdrawalCapture:
                    // Available should NOT change (already locked), locked should decrease
                    if (entry.BalanceAvailableAfter != entry.BalanceAvailableBefore)
                    {
                        result.Ok = false;
                        result.Errors.Add(new WalletConsistencyError
                        {
                            Code = "withdrawal_capture_invariant",
                            EntityType = "WalletLedgerEntry", EntityId = entry.Id.ToString(),
                            Message = $"WithdrawalCapture at {entry.Id}: available balance should not change during capture"
                        });
                    }
                    // Verify capture amount matches lock amount
                    if (lockedAmounts.TryGetValue(txnId, out var capLockedAmt) && entry.Amount != capLockedAmt)
                    {
                        result.Ok = false;
                        result.Errors.Add(new WalletConsistencyError
                        {
                            Code = "withdrawal_capture_amount_mismatch",
                            EntityType = "WalletLedgerEntry", EntityId = entry.Id.ToString(),
                            Message = $"WithdrawalCapture at {entry.Id}: capture amount {entry.Amount} != lock amount {capLockedAmt}"
                        });
                    }
                    lockedAmounts.Remove(txnId);
                    break;
            }
        }
    }
}
