using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Domain.Entities;
using Domain.Repositories;
using Domain.Services;
using Microsoft.Extensions.Logging;
using Minisource.Common.Domain;
using Minisource.Common.Exceptions;

namespace Application.Features.Ledger;

public class LedgerHashService : ILedgerHashService, ILedgerHashProvider
{
    private readonly IWalletLedgerRepository _ledgerRepo;
    private readonly IWalletAccountRepository _walletRepo;
    private readonly IAuditLogRepository _auditRepo;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<LedgerHashService> _logger;

    public LedgerHashService(IWalletLedgerRepository ledgerRepo, IWalletAccountRepository walletRepo,
        IAuditLogRepository auditRepo, IUnitOfWork uow, ILogger<LedgerHashService> logger)
    { _ledgerRepo = ledgerRepo; _walletRepo = walletRepo; _auditRepo = auditRepo; _uow = uow; _logger = logger; }

    public string ComputeEntryHash(WalletLedgerEntry entry, string? previousHash)
    {
        var canonical = string.Join("|", new[]
        {
            entry.TenantId.ToString(),
            entry.WalletAccountId.ToString(),
            entry.WalletTransactionId?.ToString() ?? "",
            ((int)entry.EntryType).ToString(),
            ((int)entry.Direction).ToString(),
            entry.Amount.ToString("F10", CultureInfo.InvariantCulture),
            entry.Currency,
            entry.BalanceAvailableBefore.ToString("F10", CultureInfo.InvariantCulture),
            entry.BalanceAvailableAfter.ToString("F10", CultureInfo.InvariantCulture),
            entry.BalanceLockedBefore.ToString("F10", CultureInfo.InvariantCulture),
            entry.BalanceLockedAfter.ToString("F10", CultureInfo.InvariantCulture),
            entry.BalancePendingBefore.ToString("F10", CultureInfo.InvariantCulture),
            entry.BalancePendingAfter.ToString("F10", CultureInfo.InvariantCulture),
            previousHash ?? "GENESIS",
            entry.CreatedAt.ToString("O"),
            entry.ReferenceType ?? "",
            entry.ReferenceId ?? ""
        });
        return Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)));
    }

    public bool VerifyEntryHash(WalletLedgerEntry entry)
    {
        var computed = ComputeEntryHash(entry, entry.PreviousEntryHash);
        return computed == entry.EntryHash;
    }

    public async Task<(bool valid, List<string> errors)> VerifyWalletHashChainAsync(Guid walletId, CancellationToken ct)
    {
        var errors = new List<string>();
        var entries = await _ledgerRepo.GetByWalletIdAsync(walletId, 0, int.MaxValue, ct);
        entries = entries.OrderBy(e => e.CreatedAt).ThenBy(e => e.Id).ToList();

        string? previousHash = null;
        foreach (var entry in entries)
        {
            if (string.IsNullOrEmpty(entry.EntryHash))
            {
                errors.Add($"Entry {entry.Id}: missing hash");
                previousHash = null; // Reset chain
                continue;
            }
            if (entry.PreviousEntryHash != previousHash)
            {
                errors.Add($"Entry {entry.Id}: previous hash mismatch (expected: {previousHash?[..16]}..., got: {entry.PreviousEntryHash?[..16]}...)");
            }
            var computed = ComputeEntryHash(entry, entry.PreviousEntryHash);
            if (computed != entry.EntryHash)
                errors.Add($"Entry {entry.Id}: hash tampered (computed: {computed[..16]}..., stored: {entry.EntryHash[..16]}...)");
            previousHash = entry.EntryHash;
        }

        return (errors.Count == 0, errors);
    }

    public async Task<int> BackfillWalletHashChainAsync(Guid walletId, CancellationToken ct)
    {
        var entries = await _ledgerRepo.GetByWalletIdAsync(walletId, 0, int.MaxValue, ct);
        entries = entries.OrderBy(e => e.CreatedAt).ThenBy(e => e.Id).ToList();

        int backfilled = 0;
        string? previousHash = null;

        foreach (var entry in entries)
        {
            if (!string.IsNullOrEmpty(entry.EntryHash))
            {
                previousHash = entry.EntryHash;
                continue; // Already hashed
            }
            entry.PreviousEntryHash = previousHash;
            entry.EntryHash = ComputeEntryHash(entry, previousHash);
            entry.HashAlgorithm = "SHA256";
            entry.HashVersion = 1;
            // Entities are modified in-place and saved via SaveChangesAsync below.
            // IMPORTANT: Requires IWalletLedgerRepository.GetByWalletIdAsync to use change tracking (NOT AsNoTracking).
            previousHash = entry.EntryHash;
            backfilled++;
        }

        if (backfilled > 0)
        {
            await _uow.SaveChangesAsync(ct);
            var wallet = await _walletRepo.GetByIdAsync(walletId, ct);
            await _auditRepo.AddAsync(new AuditLog
            {
                TenantId = wallet?.TenantId, Action = "ledger.hash_backfilled",
                EntityType = "WalletAccount", EntityId = walletId.ToString(),
                AfterSnapshot = System.Text.Json.JsonSerializer.Serialize(new { backfilled })
            }, ct);
            await _uow.SaveChangesAsync(ct);
        }

        _logger.LogInformation("Backfilled {Count} ledger entries for wallet {WalletId}", backfilled, walletId);
        return backfilled;
    }
}
