using Application.DTOs;
using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using Microsoft.Extensions.Logging;
using Minisource.Common.Domain;
using Minisource.Common.Exceptions;
using Minisource.Common.Locking;
using Application.Features.Approvals;
using Minisource.Common.Response;

namespace Application.Features.Wallets;

public class AdminWalletService : IAdminWalletService
{
    private readonly IWalletAccountRepository _walletRepo;
    private readonly IWalletLedgerRepository _ledgerRepo;
    private readonly IWalletTransactionRecordRepository _txnRepo;
    private readonly IIdempotencyRecordRepository _idemRepo;
    private readonly IAuditLogRepository _auditRepo;
    private readonly ISettingsRepository _settingsRepo;
    private readonly IDistributedLockService _lockService;
    private readonly IUnitOfWork _uow;
    private readonly IAdminApprovalService _approvalSvc;
    private readonly ILogger<AdminWalletService> _logger;

    public AdminWalletService(
        IWalletAccountRepository walletRepo,
        IWalletLedgerRepository ledgerRepo,
        IWalletTransactionRecordRepository txnRepo,
        IIdempotencyRecordRepository idemRepo,
        IAuditLogRepository auditRepo,
        ISettingsRepository settingsRepo,
        IDistributedLockService lockService,
        IUnitOfWork uow,
        IAdminApprovalService approvalSvc,
        ILogger<AdminWalletService> logger)
    {
        _walletRepo = walletRepo;
        _ledgerRepo = ledgerRepo;
        _txnRepo = txnRepo;
        _idemRepo = idemRepo;
        _auditRepo = auditRepo;
        _settingsRepo = settingsRepo;
        _lockService = lockService;
        _uow = uow;
        _approvalSvc = approvalSvc;
        _logger = logger;
    }

    public async Task<AdminAdjustmentResponse> CreditWalletAsync(
        Guid tenantId, Guid walletId, Guid actorUserId,
        AdminCreditWalletRequest request, string? idempotencyKey, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
            throw new ValidationException("IdempotencyKey", "X-Idempotency-Key header is required for wallet mutations");

        if (request.Amount <= 0)
            throw new ValidationException("Amount", "Amount must be positive");

        // Acquire Redis distributed lock
        var lockKey = $"payment:tenant:{tenantId}:wallet:{walletId}:currency:{request.Currency}";
        await using var walletLock = await _lockService.AcquireAsync(lockKey, TimeSpan.FromSeconds(30), ct);
        if (!walletLock.IsAcquired)
            throw new ConflictException("Unable to acquire lock for wallet operation");

        // Idempotency check + pre-insert
        var existingIdem = await _idemRepo.GetByKeyAsync(tenantId, idempotencyKey, ct);
        if (existingIdem is { Status: "completed" })
            throw new ConflictException("Request already processed with this idempotency key");

        if (existingIdem == null)
        {
            await _idemRepo.AddAsync(new IdempotencyRecord
            {
                TenantId = tenantId, IdempotencyKey = idempotencyKey,
                Operation = "admin_credit", Status = "processing",
                CreatedByUserId = actorUserId
            }, ct);
            await _uow.SaveChangesAsync(ct);
        }

        // Tenant settings check
        var tenantSettings = await _settingsRepo.GetTenantSettingsAsync(tenantId, ct);
        if (tenantSettings is { ManualAdminAdjustmentsEnabled: false })
            throw new BusinessException("Manual admin adjustments are disabled for this tenant", "admin_adjustments_disabled");

        await using var txn = await _uow.BeginTransactionAsync(ct);

        try
        {
            var wallet = await _walletRepo.GetByIdForUpdateAsync(walletId, ct)
                ?? throw new NotFoundException("WalletAccount", walletId.ToString());

            if (wallet.TenantId != tenantId)
                throw new ForbiddenException($"Wallet does not belong to tenant {tenantId}");

            if (wallet.Currency != request.Currency)
                throw new ValidationException("Currency", $"Currency mismatch: wallet is {wallet.Currency}, requested {request.Currency}");

            ValidateLimits(tenantSettings, request.Amount);

            // Maker-checker: require approval for high-amount admin credit
            if (tenantSettings?.WithdrawalRequiresAdminApproval == true && request.Amount >= (tenantSettings?.MaxWalletAdjustmentAmount ?? 1000000))
            {
                await _approvalSvc.RequestApprovalAsync(tenantId, actorUserId, "admin_wallet_credit",
                    "WalletAccount", walletId.ToString(), new { request.Amount, request.Currency, request.Reason }, ct);
            }

            // Create transaction record
            var record = WalletTransactionRecord.Create(
                tenantId, "admin_credit", request.Amount, wallet.Currency,
                idempotencyKey, $"Admin credit: {request.Reason}", request.Reason,
                actorUserId, request.ReferenceType, request.ReferenceId);

            // Post credit with walletTransactionId
            var entry = wallet.PostCredit(request.Amount, LedgerEntryType.AdminCredit, record.Id,
                request.Reason, request.ReferenceType, request.ReferenceId, actorUserId);
            record.AddLedgerEntry(entry);
            record.Post();

            await _walletRepo.UpdateAsync(wallet, ct);
            await _txnRepo.AddAsync(record, ct);

            // Update idempotency record to completed
            existingIdem = await _idemRepo.GetByKeyAsync(tenantId, idempotencyKey, ct);
            if (existingIdem != null)
            {
                existingIdem.Status = "completed";
                existingIdem.ResourceType = "wallet_transaction";
                existingIdem.ResourceId = record.Id.ToString();
                existingIdem.CompletedAt = DateTime.UtcNow;
                await _idemRepo.UpdateAsync(existingIdem, ct);
            }

            // Audit
            await _auditRepo.AddAsync(new AuditLog
            {
                TenantId = tenantId, ActorUserId = actorUserId,
                Action = "wallet.admin_credited", EntityType = "WalletAccount", EntityId = walletId.ToString(),
                Reason = request.Reason
            }, ct);

            await _uow.SaveChangesAsync(ct);
            await txn.CommitAsync(ct);

            return new AdminAdjustmentResponse
            {
                WalletTransactionId = record.Id, WalletId = wallet.Id,
                EntryId = entry.Id, Amount = request.Amount, Currency = wallet.Currency,
                AvailableBalanceAfter = wallet.AvailableBalance,
                LockedBalanceAfter = wallet.LockedBalance,
                Status = "posted"
            };
        }
        catch
        {
            await txn.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<AdminAdjustmentResponse> DebitWalletAsync(
        Guid tenantId, Guid walletId, Guid actorUserId,
        AdminDebitWalletRequest request, string? idempotencyKey, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
            throw new ValidationException("IdempotencyKey", "X-Idempotency-Key header is required");

        if (request.Amount <= 0)
            throw new ValidationException("Amount", "Amount must be positive");

        // Acquire Redis distributed lock
        var lockKey = $"payment:tenant:{tenantId}:wallet:{walletId}:currency:{request.Currency}";
        await using var walletLock = await _lockService.AcquireAsync(lockKey, TimeSpan.FromSeconds(30), ct);
        if (!walletLock.IsAcquired)
            throw new ConflictException("Unable to acquire lock for wallet operation");

        // Idempotency check + pre-insert
        var existingIdem = await _idemRepo.GetByKeyAsync(tenantId, idempotencyKey, ct);
        if (existingIdem is { Status: "completed" })
            throw new ConflictException("Request already processed");

        if (existingIdem == null)
        {
            await _idemRepo.AddAsync(new IdempotencyRecord
            {
                TenantId = tenantId, IdempotencyKey = idempotencyKey,
                Operation = "admin_debit", Status = "processing",
                CreatedByUserId = actorUserId
            }, ct);
            await _uow.SaveChangesAsync(ct);
        }

        var tenantSettings = await _settingsRepo.GetTenantSettingsAsync(tenantId, ct);
        if (tenantSettings is { ManualAdminAdjustmentsEnabled: false })
            throw new BusinessException("Manual admin adjustments are disabled", "admin_adjustments_disabled");

        await using var txn = await _uow.BeginTransactionAsync(ct);

        try
        {
            var wallet = await _walletRepo.GetByIdForUpdateAsync(walletId, ct)
                ?? throw new NotFoundException("WalletAccount", walletId.ToString());

            if (wallet.TenantId != tenantId)
                throw new ForbiddenException($"Wallet does not belong to tenant {tenantId}");

            if (wallet.Currency != request.Currency)
                throw new ValidationException("Currency", "Currency mismatch");

            ValidateLimits(tenantSettings, request.Amount);

            // Maker-checker: require approval for high-amount admin debit
            if (tenantSettings?.WithdrawalRequiresAdminApproval == true && request.Amount >= (tenantSettings?.MaxWalletAdjustmentAmount ?? 1000000))
            {
                await _approvalSvc.RequestApprovalAsync(tenantId, actorUserId, "admin_wallet_debit",
                    "WalletAccount", walletId.ToString(), new { request.Amount, request.Currency, request.Reason }, ct);
            }

            if (!request.AllowNegative && request.Amount > wallet.AvailableBalance)
                throw new BusinessException(
                    $"Insufficient balance. Available: {wallet.AvailableBalance}, Requested: {request.Amount}",
                    "wallet_insufficient_balance");

            var record = WalletTransactionRecord.Create(
                tenantId, "admin_debit", request.Amount, wallet.Currency,
                idempotencyKey, $"Admin debit: {request.Reason}", request.Reason,
                actorUserId, request.ReferenceType, request.ReferenceId);

            var entry = wallet.PostDebit(request.Amount, LedgerEntryType.AdminDebit,
                request.AllowNegative, record.Id, request.Reason,
                request.ReferenceType, request.ReferenceId, actorUserId);
            record.AddLedgerEntry(entry);
            record.Post();

            await _walletRepo.UpdateAsync(wallet, ct);
            await _txnRepo.AddAsync(record, ct);

            // Update idempotency record to completed
            existingIdem = await _idemRepo.GetByKeyAsync(tenantId, idempotencyKey, ct);
            if (existingIdem != null)
            {
                existingIdem.Status = "completed";
                existingIdem.ResourceType = "wallet_transaction";
                existingIdem.ResourceId = record.Id.ToString();
                existingIdem.CompletedAt = DateTime.UtcNow;
                await _idemRepo.UpdateAsync(existingIdem, ct);
            }

            // Audit
            await _auditRepo.AddAsync(new AuditLog
            {
                TenantId = tenantId, ActorUserId = actorUserId,
                Action = "wallet.admin_debited", EntityType = "WalletAccount", EntityId = walletId.ToString(),
                Reason = request.Reason
            }, ct);

            await _uow.SaveChangesAsync(ct);
            await txn.CommitAsync(ct);

            return new AdminAdjustmentResponse
            {
                WalletTransactionId = record.Id, WalletId = wallet.Id,
                EntryId = entry.Id, Amount = request.Amount, Currency = wallet.Currency,
                AvailableBalanceAfter = wallet.AvailableBalance,
                LockedBalanceAfter = wallet.LockedBalance,
                Status = "posted"
            };
        }
        catch
        {
            await txn.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<WalletAccountDto> GetWalletAsync(Guid walletId, CancellationToken ct = default)
    {
        var wallet = await _walletRepo.GetByIdAsync(walletId, ct)
            ?? throw new NotFoundException("WalletAccount", walletId.ToString());
        return MapWalletToDto(wallet);
    }

    public async Task<List<WalletAccountDto>> GetUserWalletsAsync(Guid tenantId, string ownerType, Guid ownerId, CancellationToken ct = default)
    {
        var wallets = await _walletRepo.GetByOwnerAsync(tenantId, ownerType, ownerId, ct);
        return wallets.Select(MapWalletToDto).ToList();
    }

    public async Task<List<WalletAccountDto>> GetAdminWalletsAsync(Guid tenantId, string? ownerType, Guid? ownerId, string? currency, string? status, int skip, int take, CancellationToken ct = default)
    {
        var statusEnum = status != null ? Enum.Parse<WalletAccountStatus>(status, true) : (WalletAccountStatus?)null;
        var wallets = await _walletRepo.GetByTenantAsync(tenantId, statusEnum, skip, take, ct);
        return wallets.Select(MapWalletToDto).ToList();
    }

    public async Task<PagedResponse<LedgerEntryDto>> GetLedgerAsync(Guid walletId, int skip, int take, CancellationToken ct = default)
    {
        var entries = await _ledgerRepo.GetByWalletIdAsync(walletId, skip, take, ct);
        var count = await _ledgerRepo.CountByWalletIdAsync(walletId, ct);
        return PagedResponse<LedgerEntryDto>.Ok(
            entries.Select(MapLedgerToDto).ToList().AsReadOnly(),
            skip / take + 1, take, count);
    }

    public async Task<PagedResponse<WalletTransactionRecordDto>> GetTransactionsAsync(Guid tenantId, string? transactionType, Guid? walletId, int skip, int take, CancellationToken ct = default)
    {
        var items = await _txnRepo.GetByTenantAsync(tenantId, skip, take, transactionType, walletId, ct);
        var count = await _txnRepo.CountByTenantAsync(tenantId, transactionType, walletId, ct);
        return PagedResponse<WalletTransactionRecordDto>.Ok(
            items.Select(MapTxnToDto).ToList().AsReadOnly(),
            skip / take + 1, take, count);
    }

    public async Task<WalletTransactionRecordDto> GetTransactionAsync(Guid transactionId, CancellationToken ct = default)
    {
        var txn = await _txnRepo.GetByIdAsync(transactionId, ct)
            ?? throw new NotFoundException("WalletTransactionRecord", transactionId.ToString());
        return MapTxnToDto(txn);
    }

    public async Task<PagedResponse<LedgerEntryDto>> ListAllLedgerAsync(
        Guid? tenantId, Guid? walletId, string? entryType, string? direction, string? currency,
        decimal? amountMin, decimal? amountMax, string? referenceType, string? referenceId,
        string? dateFrom, string? dateTo, string? query, int skip, int take, CancellationToken ct = default)
    {
        var (entries, count) = await _ledgerRepo.GetAllAsync(
            tenantId, walletId, entryType, direction, currency,
            amountMin, amountMax, referenceType, referenceId,
            dateFrom, dateTo, query, skip, take, ct);
        return PagedResponse<LedgerEntryDto>.Ok(
            entries.Select(MapLedgerToDto).ToList().AsReadOnly(),
            skip / take + 1, take, count);
    }

    private static void ValidateLimits(TenantPaymentSettings? settings, decimal amount)
    {
        if (settings == null) return;
        if (settings.MinWalletAdjustmentAmount.HasValue && amount < settings.MinWalletAdjustmentAmount.Value)
            throw new ValidationException("Amount", $"Amount below minimum: {settings.MinWalletAdjustmentAmount}");
        if (settings.MaxWalletAdjustmentAmount.HasValue && amount > settings.MaxWalletAdjustmentAmount.Value)
            throw new ValidationException("Amount", $"Amount exceeds maximum: {settings.MaxWalletAdjustmentAmount}");
    }

    private static WalletAccountDto MapWalletToDto(WalletAccount w) => new()
    {
        Id = w.Id, TenantId = w.TenantId, OwnerType = w.OwnerType, OwnerId = w.OwnerId,
        Currency = w.Currency, AvailableBalance = w.AvailableBalance,
        LockedBalance = w.LockedBalance, PendingBalance = w.PendingBalance,
        Status = w.Status.ToString(), Version = w.Version,
        CreatedAt = w.CreatedAt, UpdatedAt = w.UpdatedAt
    };

    private static LedgerEntryDto MapLedgerToDto(WalletLedgerEntry e) => new()
    {
        Id = e.Id, WalletAccountId = e.WalletAccountId,
        WalletTransactionId = e.WalletTransactionId,
        EntryType = e.EntryType.ToString(), Direction = e.Direction.ToString(),
        Amount = e.Amount, Currency = e.Currency,
        BalanceAvailableBefore = e.BalanceAvailableBefore,
        BalanceAvailableAfter = e.BalanceAvailableAfter,
        Reason = e.Reason, ReferenceType = e.ReferenceType, ReferenceId = e.ReferenceId,
        CreatedByUserId = e.CreatedByUserId, CreatedAt = e.CreatedAt
    };

    private static WalletTransactionRecordDto MapTxnToDto(WalletTransactionRecord r) => new()
    {
        Id = r.Id, TenantId = r.TenantId, TransactionType = r.TransactionType,
        Status = r.Status.ToString(), Amount = r.Amount, Currency = r.Currency,
        SourceWalletId = r.SourceWalletId, DestinationWalletId = r.DestinationWalletId,
        ReferenceType = r.ReferenceType, ReferenceId = r.ReferenceId,
        IdempotencyKey = r.IdempotencyKey, Description = r.Description, Reason = r.Reason,
        CreatedByUserId = r.CreatedByUserId, CreatedAt = r.CreatedAt,
        PostedAt = r.PostedAt, ReversedAt = r.ReversedAt
    };
}
