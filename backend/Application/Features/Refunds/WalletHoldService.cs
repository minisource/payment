using Domain.Entities;
using Domain.Enums;
using Domain.Gateways;
using Domain.Repositories;
using Microsoft.Extensions.Logging;
using Minisource.Common.Domain;
using Minisource.Common.Exceptions;
using Minisource.Common.Locking;

namespace Application.Features.Refunds;

/// <summary>
/// Wallet hold service implementing strict pre-hold policy for refunds.
/// Uses WalletAccount.LockFunds/UnlockFunds/CaptureLockedFunds for atomic balance operations,
/// with a separate WalletHold entity for business-level tracking.
/// </summary>
public class WalletHoldService : IWalletHoldService
{
    private readonly IWalletAccountRepository _walletRepo;
    private readonly IWalletHoldRepository _holdRepo;
    private readonly IWalletLedgerRepository _ledgerRepo;
    private readonly IWalletTransactionRecordRepository _txnRepo;
    private readonly IDistributedLockService _lockService;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<WalletHoldService> _logger;

    public WalletHoldService(
        IWalletAccountRepository walletRepo,
        IWalletHoldRepository holdRepo,
        IWalletLedgerRepository ledgerRepo,
        IWalletTransactionRecordRepository txnRepo,
        IDistributedLockService lockService,
        IUnitOfWork uow,
        ILogger<WalletHoldService> logger)
    {
        _walletRepo = walletRepo;
        _holdRepo = holdRepo;
        _ledgerRepo = ledgerRepo;
        _txnRepo = txnRepo;
        _lockService = lockService;
        _uow = uow;
        _logger = logger;
    }

    public async Task<CreateWalletHoldResult> CreateHoldAsync(
        CreateWalletHoldRequest request,
        CancellationToken ct = default)
    {
        var lockKey = $"payment:hold:{request.WalletAccountId}";
        await using var holdLock = await _lockService.AcquireAsync(lockKey, TimeSpan.FromSeconds(30), ct);
        if (!holdLock.IsAcquired)
            return new CreateWalletHoldResult(false, ErrorCode: "hold_lock_failed",
                ErrorMessage: "Unable to acquire lock for wallet hold");

        if (!string.IsNullOrWhiteSpace(request.IdempotencyKey))
        {
            var existingHold = await _holdRepo.GetActiveByReferenceAsync(
                request.ReferenceType, request.ReferenceId, ct);
            if (existingHold is not null)
                return new CreateWalletHoldResult(true, existingHold.Id, existingHold,
                    ErrorCode: "hold_already_exists");
        }

        await using var txn = await _uow.BeginTransactionAsync(ct);

        try
        {
            var wallet = await _walletRepo.GetByIdForUpdateAsync(request.WalletAccountId, ct);
            if (wallet is null)
                return new CreateWalletHoldResult(false, ErrorCode: "wallet_not_found",
                    ErrorMessage: $"Wallet {request.WalletAccountId} not found");

            if (wallet.TenantId != request.TenantId)
                return new CreateWalletHoldResult(false, ErrorCode: "wallet_tenant_mismatch",
                    ErrorMessage: $"Wallet does not belong to tenant {request.TenantId}");

            // Lock funds: moves from AvailableBalance to LockedBalance
            // Throws if insufficient available balance
            try
            {
                var parsedUserId = TryParseGuid(request.CreatedByUserId);
                wallet.LockFunds(request.Amount, reason: request.Reason,
                    referenceType: request.ReferenceType,
                    referenceId: request.ReferenceId.ToString(),
                    createdByUserId: parsedUserId);
            }
            catch (InvalidOperationException ex)
            {
                await txn.RollbackAsync(ct);
                _logger.LogWarning("Insufficient available balance for hold: {Message}", ex.Message);
                return new CreateWalletHoldResult(false,
                    ErrorCode: "refund_wallet_balance_insufficient",
                    ErrorMessage: $"Wallet balance insufficient. Available: {wallet.AvailableBalance}, Requested: {request.Amount}");
            }

            var hold = WalletHold.Create(
                request.TenantId, request.WalletAccountId, request.Amount, request.Currency,
                request.ReferenceType, request.ReferenceId, request.Reason,
                request.ApplicationCode, request.CreatedByUserId);

            await _walletRepo.UpdateAsync(wallet, ct);
            await _holdRepo.AddAsync(hold, ct);

            var parsedRecordUserId = TryParseGuid(request.CreatedByUserId);
            var txnRecord = WalletTransactionRecord.Create(
                request.TenantId, "refund_hold", request.Amount, request.Currency,
                request.IdempotencyKey, $"Refund hold for {request.ReferenceType}:{request.ReferenceId}",
                request.Reason, createdByUserId: parsedRecordUserId,
                referenceType: request.ReferenceType,
                referenceId: request.ReferenceId.ToString());
            txnRecord.Post();
            await _txnRepo.AddAsync(txnRecord, ct);

            await _uow.SaveChangesAsync(ct);
            await txn.CommitAsync(ct);

            _logger.LogInformation("Hold created: {HoldId} for wallet {WalletId}, amount {Amount}",
                hold.Id, request.WalletAccountId, request.Amount);

            return new CreateWalletHoldResult(true, hold.Id, hold);
        }
        catch (Exception ex) when (ex is not AppException)
        {
            await txn.RollbackAsync(ct);
            _logger.LogError(ex, "Failed to create wallet hold");
            return new CreateWalletHoldResult(false,
                ErrorCode: "hold_creation_failed", ErrorMessage: ex.Message);
        }
    }

    public async Task<WalletHoldActionResult> CaptureHoldAsync(
        CaptureWalletHoldRequest request,
        CancellationToken ct = default)
    {
        var lockKey = $"payment:hold:capture:{request.HoldId}";
        await using var holdLock = await _lockService.AcquireAsync(lockKey, TimeSpan.FromSeconds(30), ct);
        if (!holdLock.IsAcquired)
            return new WalletHoldActionResult(false, ErrorCode: "hold_lock_failed",
                ErrorMessage: "Unable to acquire lock for hold capture");

        await using var txn = await _uow.BeginTransactionAsync(ct);

        try
        {
            var hold = await _holdRepo.GetByIdForUpdateAsync(request.HoldId, ct);
            if (hold is null)
                return new WalletHoldActionResult(false, ErrorCode: "hold_not_found",
                    ErrorMessage: $"Wallet hold {request.HoldId} not found");

            if (!hold.IsActive)
                return new WalletHoldActionResult(false, hold,
                    ErrorCode: "hold_not_active", ErrorMessage: $"Hold {request.HoldId} is not active");

            var wallet = await _walletRepo.GetByIdForUpdateAsync(hold.WalletAccountId, ct);
            if (wallet is null)
                return new WalletHoldActionResult(false, ErrorCode: "wallet_not_found",
                    ErrorMessage: $"Wallet {hold.WalletAccountId} not found");

            var parsedUserId = TryParseGuid(request.CapturedByUserId);
            wallet.CaptureLockedFunds(hold.Amount, reason: request.Reason,
                referenceType: hold.ReferenceType,
                referenceId: hold.ReferenceId.ToString(),
                createdByUserId: parsedUserId);

            hold.Capture();

            await _walletRepo.UpdateAsync(wallet, ct);
            await _holdRepo.UpdateAsync(hold, ct);

            var txnRecord = WalletTransactionRecord.Create(
                hold.TenantId, "refund_hold_capture", hold.Amount, hold.Currency,
                reason: request.Reason,
                referenceType: hold.ReferenceType,
                referenceId: hold.ReferenceId.ToString());
            txnRecord.Post();
            await _txnRepo.AddAsync(txnRecord, ct);

            await _uow.SaveChangesAsync(ct);
            await txn.CommitAsync(ct);

            _logger.LogInformation("Hold captured: {HoldId}", request.HoldId);
            return new WalletHoldActionResult(true, hold);
        }
        catch (Exception ex) when (ex is not AppException)
        {
            await txn.RollbackAsync(ct);
            _logger.LogError(ex, "Failed to capture hold {HoldId}", request.HoldId);
            return new WalletHoldActionResult(false, ErrorCode: "hold_capture_failed", ErrorMessage: ex.Message);
        }
    }

    public async Task<WalletHoldActionResult> ReleaseHoldAsync(
        ReleaseWalletHoldRequest request,
        CancellationToken ct = default)
    {
        var lockKey = $"payment:hold:release:{request.HoldId}";
        await using var holdLock = await _lockService.AcquireAsync(lockKey, TimeSpan.FromSeconds(30), ct);
        if (!holdLock.IsAcquired)
            return new WalletHoldActionResult(false, ErrorCode: "hold_lock_failed",
                ErrorMessage: "Unable to acquire lock for hold release");

        await using var txn = await _uow.BeginTransactionAsync(ct);

        try
        {
            var hold = await _holdRepo.GetByIdForUpdateAsync(request.HoldId, ct);
            if (hold is null)
                return new WalletHoldActionResult(false, ErrorCode: "hold_not_found",
                    ErrorMessage: $"Wallet hold {request.HoldId} not found");

            if (!hold.IsActive)
                return new WalletHoldActionResult(false, hold,
                    ErrorCode: "hold_not_active", ErrorMessage: $"Hold {request.HoldId} is not active");

            var wallet = await _walletRepo.GetByIdForUpdateAsync(hold.WalletAccountId, ct);
            if (wallet is null)
                return new WalletHoldActionResult(false, ErrorCode: "wallet_not_found",
                    ErrorMessage: $"Wallet {hold.WalletAccountId} not found");

            var parsedUserId = TryParseGuid(request.ReleasedByUserId);
            wallet.UnlockFunds(hold.Amount, reason: request.Reason,
                referenceType: hold.ReferenceType,
                referenceId: hold.ReferenceId.ToString(),
                createdByUserId: parsedUserId);

            hold.Release();

            await _walletRepo.UpdateAsync(wallet, ct);
            await _holdRepo.UpdateAsync(hold, ct);

            var txnRecord = WalletTransactionRecord.Create(
                hold.TenantId, "refund_hold_release", hold.Amount, hold.Currency,
                reason: request.Reason,
                referenceType: hold.ReferenceType,
                referenceId: hold.ReferenceId.ToString());
            txnRecord.Post();
            await _txnRepo.AddAsync(txnRecord, ct);

            await _uow.SaveChangesAsync(ct);
            await txn.CommitAsync(ct);

            _logger.LogInformation("Hold released: {HoldId}", request.HoldId);
            return new WalletHoldActionResult(true, hold);
        }
        catch (Exception ex) when (ex is not AppException)
        {
            await txn.RollbackAsync(ct);
            _logger.LogError(ex, "Failed to release hold {HoldId}", request.HoldId);
            return new WalletHoldActionResult(false, ErrorCode: "hold_release_failed", ErrorMessage: ex.Message);
        }
    }

    private static Guid? TryParseGuid(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return Guid.TryParse(value, out var g) ? g : null;
    }
}
