using System.Text.Json;
using Application.DTOs;
using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using Microsoft.Extensions.Logging;
using Minisource.Common.Domain;
using Minisource.Common.Exceptions;
using Application.Features.Risk;
using Application.Features.Limits;
using Minisource.Common.Locking;

namespace Application.Features.Withdrawals;

public class WithdrawalService : IWithdrawalService
{
    private readonly IWithdrawalRequestRepository _reqRepo;
    private readonly IPayoutAccountRepository _payoutRepo;
    private readonly IWalletAccountRepository _walletRepo;
    private readonly IWalletLedgerRepository _ledgerRepo;
    private readonly IPayoutRecordRepository _payoutRecordRepo;
    private readonly ISettingsRepository _settingsRepo;
    private readonly IIdempotencyRecordRepository _idempotencyRepo;
    private readonly IAuditLogRepository _auditRepo;
    private readonly IDistributedLockService _lockService;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<WithdrawalService> _logger;

    private readonly IUserComplianceService _complianceSvc;
    private readonly IRiskEvaluationService _riskSvc;
    private readonly ILimitUsageService _limitSvc;

    public WithdrawalService(
        IWithdrawalRequestRepository reqRepo, IPayoutAccountRepository payoutRepo,
        IWalletAccountRepository walletRepo, IWalletLedgerRepository ledgerRepo,
        IPayoutRecordRepository payoutRecordRepo, ISettingsRepository settingsRepo,
        IIdempotencyRecordRepository idempotencyRepo, IAuditLogRepository auditRepo,
        IDistributedLockService lockService, IUnitOfWork uow, IUserComplianceService complianceSvc,
        IRiskEvaluationService riskSvc, ILimitUsageService limitSvc, ILogger<WithdrawalService> logger)
    {
        _reqRepo = reqRepo; _payoutRepo = payoutRepo; _walletRepo = walletRepo;
        _ledgerRepo = ledgerRepo; _payoutRecordRepo = payoutRecordRepo;
        _settingsRepo = settingsRepo; _idempotencyRepo = idempotencyRepo;
        _auditRepo = auditRepo; _lockService = lockService; _uow = uow;
        _complianceSvc = complianceSvc; _riskSvc = riskSvc; _limitSvc = limitSvc; _logger = logger;
    }

    public async Task<WithdrawalRequestDto> CreateAsync(Guid tenantId, Guid ownerId, string ownerType,
        CreateWithdrawalRequest request, string? applicationCode, string? idempotencyKey, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            var existing = await _reqRepo.GetByIdempotencyKeyAsync(tenantId, idempotencyKey, ct);
            if (existing != null) return MapToDto(existing);
        }

        var payoutAccount = await _payoutRepo.GetByIdAsync(request.PayoutAccountId, ct)
            ?? throw new BusinessException("Payout account not found", "payout_account_not_found");
        if (payoutAccount.OwnerId != ownerId || payoutAccount.TenantId != tenantId)
            throw new BusinessException("Payout account does not belong to you", "payout_account_not_owned");
        if (!payoutAccount.CanBeUsed)
            throw new BusinessException("Payout account cannot be used", "payout_account_not_verified");
        if (!payoutAccount.Currency.Equals(request.Currency, StringComparison.OrdinalIgnoreCase))
            throw new BusinessException("Currency mismatch", "payout_account_currency_mismatch");

        var settings = await _settingsRepo.GetTenantSettingsAsync(tenantId, ct);
        if (settings != null)
        {
            if (!settings.WithdrawalsEnabled)
                throw new BusinessException("Withdrawals disabled", "withdrawal_disabled");
            if (settings.WithdrawalRequiresVerifiedPayoutAccount && payoutAccount.Status != PayoutAccountStatus.Verified)
                throw new BusinessException("Verified payout account required", "payout_account_verification_required");
            if (settings.MinWithdrawalAmount.HasValue && request.Amount < settings.MinWithdrawalAmount.Value)
                throw new BusinessException("Amount below minimum", "withdrawal_amount_below_min");
            if (settings.MaxWithdrawalAmount.HasValue && request.Amount > settings.MaxWithdrawalAmount.Value)
                throw new BusinessException("Amount above maximum", "withdrawal_amount_above_max");
        }

        if (request.Amount <= 0)
            throw new BusinessException("Amount must be positive", "withdrawal_amount_invalid");

        // KYC/Compliance check
        var compliance = await _complianceSvc.GetComplianceStatusAsync(tenantId, ownerId, ct);
        if (compliance.IsBlocked)
            throw new BusinessException("User is blocked from financial operations", "user_compliance_blocked");
        if (compliance.IsHighRisk)
            _logger.LogWarning("High-risk user {UserId} initiating withdrawal, flagged for review", ownerId);
        if (!compliance.KycCompleted && (settings?.KycRequiredForWithdrawal ?? true))
            throw new BusinessException("KYC verification required for withdrawals", "kyc_required");

        // Risk evaluation
        var riskResult = await _riskSvc.EvaluateAsync(tenantId, "withdrawal_create", "WithdrawalRequest",
            string.Empty, ownerType, ownerId, request.Amount, request.Currency, ct);
        if (riskResult.Decision == "block")
            throw new BusinessException("Withdrawal blocked by risk rules", "risk_rule_blocked_operation");

        // Limit usage check
        var (limitAllowed, limitReason) = await _limitSvc.CheckLimitsAsync(tenantId, "user", ownerId,
            "withdrawal", request.Currency, request.Amount, ct);
        if (!limitAllowed)
            throw new BusinessException(limitReason ?? "Limit exceeded", "limit_exceeded");

        var feeAmount = CalculateFee(settings, request.Amount);
        var netAmount = request.Amount - feeAmount;
        if (netAmount <= 0)
            throw new BusinessException("Net amount after fee is zero or negative", "withdrawal_amount_invalid");

        var lockKey = $"wallet:withdraw:{request.WalletId}";
        await using var walletLock = await _lockService.AcquireAsync(lockKey, TimeSpan.FromSeconds(30), ct);
        if (!walletLock.IsAcquired)
            throw new BusinessException("Unable to acquire wallet lock", "wallet_lock_failed");

        await using var tx = await _uow.BeginTransactionAsync(ct);
        try
        {
            var wallet = await _walletRepo.GetByIdForUpdateAsync(request.WalletId, ct)
                ?? throw new BusinessException("Wallet not found", "wallet_not_found");
            if (wallet.TenantId != tenantId || wallet.OwnerId != ownerId)
                throw new ForbiddenException("Wallet does not belong to user");
            if (wallet.AvailableBalance < request.Amount)
                throw new BusinessException("Insufficient available balance", "withdrawal_insufficient_balance");

            var wr = WithdrawalRequest.Create(tenantId, request.WalletId, request.PayoutAccountId,
                ownerType, ownerId, request.Amount, request.Currency, feeAmount, netAmount,
                applicationCode: applicationCode, description: request.Description,
                idempotencyKey: idempotencyKey, metadata: request.Metadata, createdByUserId: ownerId);

            var lockEntry = wallet.LockFunds(request.Amount, reason: "Withdrawal lock",
                referenceType: "WithdrawalRequest", referenceId: wr.Id.ToString(), createdByUserId: ownerId);
            wr.SetLockedTransaction(lockEntry.Id);

            await _reqRepo.AddAsync(wr, ct);
            await _ledgerRepo.AddAsync(lockEntry, ct);
            await _walletRepo.UpdateAsync(wallet, ct);

            if (!string.IsNullOrWhiteSpace(idempotencyKey))
                await _idempotencyRepo.AddAsync(new IdempotencyRecord
                {
                    TenantId = tenantId, IdempotencyKey = idempotencyKey,
                    Operation = "withdrawal.create", Status = "completed",
                    ResourceType = "WithdrawalRequest", ResourceId = wr.Id.ToString(),
                    CreatedByUserId = ownerId
                }, ct);

            await _auditRepo.AddAsync(Audit("withdrawal.created", tenantId, ownerId, wr), ct);
            await _uow.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            // Track limit usage (after successful creation, outside transaction)
            await _limitSvc.TrackUsageAsync(tenantId, "user", ownerId, "withdrawal", request.Currency, request.Amount, ct);

            _logger.LogInformation("Withdrawal {Id} created, locked {Amount} from wallet {WalletId}", wr.Id, request.Amount, request.WalletId);
            return MapToDto(wr);
        }
        catch (Exception ex) when (ex is not AppException)
        {
            await tx.RollbackAsync(ct);
            // Idempotent retry on duplicate key
            if (ex is Microsoft.EntityFrameworkCore.DbUpdateException && !string.IsNullOrWhiteSpace(idempotencyKey))
            {
                var existing = await _reqRepo.GetByIdempotencyKeyAsync(tenantId, idempotencyKey, ct);
                if (existing != null) return MapToDto(existing);
            }
            _logger.LogError(ex, "Failed to create withdrawal for wallet {WalletId}", request.WalletId);
            throw new BusinessException("Failed to create withdrawal", "withdrawal_create_failed");
        }
    }

    public async Task<List<WithdrawalRequestDto>> GetMyRequestsAsync(Guid tenantId, string ownerType, Guid ownerId, int skip, int take, CancellationToken ct)
    {
        var items = await _reqRepo.GetByOwnerAsync(tenantId, ownerType, ownerId, skip, take, ct);
        return items.Select(MapToDto).ToList();
    }

    public async Task<WithdrawalRequestDto> GetAsync(Guid withdrawalId, CancellationToken ct)
    {
        var wr = await _reqRepo.GetByIdAsync(withdrawalId, ct) ?? throw new NotFoundException("WithdrawalRequest", withdrawalId);
        return MapToDto(wr);
    }

    public async Task<WithdrawalRequestDto> CancelAsync(Guid withdrawalId, Guid tenantId, Guid ownerId, string reason, CancellationToken ct)
    {
        var wr = await _reqRepo.GetByIdAsync(withdrawalId, ct) ?? throw new NotFoundException("WithdrawalRequest", withdrawalId);
        if (wr.TenantId != tenantId || wr.OwnerId != ownerId) throw new ForbiddenException();

        var lockKey = $"wallet:withdraw:{wr.WalletId}";
        await using var walletLock = await _lockService.AcquireAsync(lockKey, TimeSpan.FromSeconds(30), ct);
        if (!walletLock.IsAcquired) throw new BusinessException("Unable to acquire wallet lock", "wallet_lock_failed");

        wr = await _reqRepo.GetByIdAsync(withdrawalId, ct) ?? throw new NotFoundException("WithdrawalRequest", withdrawalId);
        if (wr.Status == WithdrawalStatus.Cancelled) return MapToDto(wr);
        if (wr.Status == WithdrawalStatus.Paid || wr.Status == WithdrawalStatus.ProcessingPayout)
            throw new BusinessException("Cannot cancel a paid/processing withdrawal", "withdrawal_cancel_not_allowed");

        wr.Cancel(ownerId, reason);
        await ReleaseLockedFunds(wr, tenantId, ownerId, "Cancelled: " + reason, ct);
        await _reqRepo.UpdateAsync(wr, ct);
        await _auditRepo.AddAsync(Audit("withdrawal.cancelled", tenantId, ownerId, wr, reason), ct);
        await _uow.SaveChangesAsync(ct);
        return MapToDto(wr);
    }

    public async Task<List<WithdrawalRequestDto>> AdminListAsync(Guid? tenantId, WithdrawalStatus? status, int skip, int take, CancellationToken ct)
    {
        if (!tenantId.HasValue) throw new ValidationException("tenantId", "TenantId is required for admin withdrawal listing");
        var items = await _reqRepo.GetByTenantAsync(tenantId.Value, status, skip, take, ct);
        return items.Select(MapToDto).ToList();
    }

    public async Task<WithdrawalRequestDto> AdminGetAsync(Guid withdrawalId, CancellationToken ct)
    {
        var wr = await _reqRepo.GetByIdAsync(withdrawalId, ct) ?? throw new NotFoundException("WithdrawalRequest", withdrawalId);
        return MapToDto(wr);
    }

    public async Task<WithdrawalRequestDto> ApproveAsync(Guid withdrawalId, Guid actorUserId, string? note, CancellationToken ct)
    {
        var wr = await _reqRepo.GetByIdAsync(withdrawalId, ct) ?? throw new NotFoundException("WithdrawalRequest", withdrawalId);
        wr.Approve(actorUserId, note);
        await _reqRepo.UpdateAsync(wr, ct);
        await _auditRepo.AddAsync(Audit("withdrawal.approved", wr.TenantId, actorUserId, wr, note), ct);
        await _uow.SaveChangesAsync(ct);
        return MapToDto(wr);
    }

    public async Task<WithdrawalRequestDto> RejectAsync(Guid withdrawalId, Guid actorUserId, string reason, CancellationToken ct)
    {
        var wr = await _reqRepo.GetByIdAsync(withdrawalId, ct) ?? throw new NotFoundException("WithdrawalRequest", withdrawalId);
        var lockKey = $"wallet:withdraw:{wr.WalletId}";
        await using var walletLock = await _lockService.AcquireAsync(lockKey, TimeSpan.FromSeconds(30), ct);
        if (!walletLock.IsAcquired) throw new BusinessException("Unable to acquire wallet lock", "wallet_lock_failed");

        // Re-read after lock to prevent TOCTOU
        wr = await _reqRepo.GetByIdAsync(withdrawalId, ct) ?? throw new NotFoundException("WithdrawalRequest", withdrawalId);
        if (wr.Status == WithdrawalStatus.Rejected) return MapToDto(wr);
        if (wr.Status == WithdrawalStatus.Paid || wr.Status == WithdrawalStatus.Cancelled)
            throw new BusinessException("Cannot reject a paid/cancelled withdrawal", "withdrawal_reject_not_allowed");

        wr.Reject(actorUserId, reason);
        await ReleaseLockedFunds(wr, wr.TenantId, actorUserId, "Rejected: " + reason, ct);
        await _reqRepo.UpdateAsync(wr, ct);
        await _auditRepo.AddAsync(Audit("withdrawal.rejected", wr.TenantId, actorUserId, wr, reason), ct);
        await _uow.SaveChangesAsync(ct);
        return MapToDto(wr);
    }

    public async Task<WithdrawalRequestDto> RequestMoreInfoAsync(Guid withdrawalId, Guid actorUserId, string reason, CancellationToken ct)
    {
        var wr = await _reqRepo.GetByIdAsync(withdrawalId, ct) ?? throw new NotFoundException("WithdrawalRequest", withdrawalId);
        wr.RequestMoreInfo(actorUserId, reason);
        await _reqRepo.UpdateAsync(wr, ct);
        await _auditRepo.AddAsync(Audit("withdrawal.more_info_required", wr.TenantId, actorUserId, wr, reason), ct);
        await _uow.SaveChangesAsync(ct);
        return MapToDto(wr);
    }

    public async Task<WithdrawalRequestDto> MarkProcessingAsync(Guid withdrawalId, Guid actorUserId, string? note, CancellationToken ct)
    {
        var wr = await _reqRepo.GetByIdAsync(withdrawalId, ct) ?? throw new NotFoundException("WithdrawalRequest", withdrawalId);
        wr.MarkProcessing(actorUserId, note);
        await _reqRepo.UpdateAsync(wr, ct);
        await _auditRepo.AddAsync(Audit("withdrawal.processing_started", wr.TenantId, actorUserId, wr, note), ct);
        await _uow.SaveChangesAsync(ct);
        return MapToDto(wr);
    }

    public async Task<WithdrawalRequestDto> MarkPaidAsync(Guid withdrawalId, Guid actorUserId,
        AdminWithdrawalMarkPaidRequest request, CancellationToken ct)
    {
        var wr = await _reqRepo.GetByIdAsync(withdrawalId, ct) ?? throw new NotFoundException("WithdrawalRequest", withdrawalId);
        if (wr.Status == WithdrawalStatus.Paid) return MapToDto(wr);

        var lockKey = $"wallet:withdraw:{wr.WalletId}";
        await using var walletLock = await _lockService.AcquireAsync(lockKey, TimeSpan.FromSeconds(30), ct);
        if (!walletLock.IsAcquired) throw new BusinessException("Unable to acquire wallet lock", "wallet_lock_failed");

        await using var tx = await _uow.BeginTransactionAsync(ct);
        try
        {
            var wallet = await _walletRepo.GetByIdForUpdateAsync(wr.WalletId, ct)
                ?? throw new BusinessException("Wallet not found", "wallet_not_found");
            var captureEntry = wallet.CaptureLockedFunds(wr.Amount,
                reason: "Withdrawal paid", referenceType: "WithdrawalRequest",
                referenceId: wr.Id.ToString(), createdByUserId: actorUserId);
            wr.MarkPaid(actorUserId, request.BankTrackingNumber, request.BankReferenceId, request.Note);
            wr.SetCaptureTransaction(captureEntry.Id);

            var payoutRecord = await _payoutRecordRepo.GetByWithdrawalIdAsync(withdrawalId, ct);
            if (payoutRecord == null)
            {
                payoutRecord = PayoutRecord.Create(wr.TenantId, wr.Id, wr.PayoutAccountId, wr.WalletId,
                    wr.Amount, wr.Currency, wr.FeeAmount, wr.NetAmount, createdByUserId: actorUserId);
                await _payoutRecordRepo.AddAsync(payoutRecord, ct);
            }
            payoutRecord.MarkSucceeded(request.BankTrackingNumber, request.BankReferenceId);

            await _ledgerRepo.AddAsync(captureEntry, ct);
            await _walletRepo.UpdateAsync(wallet, ct);
            await _reqRepo.UpdateAsync(wr, ct);
            await _payoutRecordRepo.UpdateAsync(payoutRecord, ct);
            await _auditRepo.AddAsync(Audit("withdrawal.paid", wr.TenantId, actorUserId, wr, request.BankTrackingNumber), ct);
            await _uow.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            _logger.LogInformation("Withdrawal {Id} marked paid, captured {Amount}", withdrawalId, wr.Amount);
            return MapToDto(wr);
        }
        catch (Exception ex) when (ex is not AppException)
        {
            await tx.RollbackAsync(ct);
            _logger.LogError(ex, "Failed to mark withdrawal {Id} as paid", withdrawalId);
            throw new BusinessException("Failed to mark withdrawal as paid", "payout_mark_paid_failed");
        }
    }

    public async Task<WithdrawalRequestDto> MarkFailedAsync(Guid withdrawalId, Guid actorUserId,
        AdminWithdrawalMarkFailedRequest request, CancellationToken ct)
    {
        var wr = await _reqRepo.GetByIdAsync(withdrawalId, ct) ?? throw new NotFoundException("WithdrawalRequest", withdrawalId);
        if (request.ReleaseFunds)
        {
            var lockKey = $"wallet:withdraw:{wr.WalletId}";
            await using var walletLock = await _lockService.AcquireAsync(lockKey, TimeSpan.FromSeconds(30), ct);
            if (!walletLock.IsAcquired) throw new BusinessException("Unable to acquire wallet lock", "wallet_lock_failed");
            wr.MarkFailed(request.FailureReason);
            await ReleaseLockedFunds(wr, wr.TenantId, actorUserId, "Failed: " + request.FailureReason, ct);
        }
        else { wr.MarkFailed(request.FailureReason); }

        var payoutRecord = await _payoutRecordRepo.GetByWithdrawalIdAsync(withdrawalId, ct);
        if (payoutRecord != null) { payoutRecord.MarkFailed(request.FailureReason); await _payoutRecordRepo.UpdateAsync(payoutRecord, ct); }

        await _reqRepo.UpdateAsync(wr, ct);
        await _auditRepo.AddAsync(Audit("withdrawal.failed", wr.TenantId, actorUserId, wr, request.FailureReason), ct);
        await _uow.SaveChangesAsync(ct);
        return MapToDto(wr);
    }

    public async Task<WithdrawalSummaryDto> GetSummaryAsync(Guid? tenantId, CancellationToken ct)
    {
        if (!tenantId.HasValue) throw new ValidationException("tenantId", "TenantId is required for withdrawal summary");
        var items = await _reqRepo.GetByTenantAsync(tenantId.Value, ct: ct);
        var byCurrency = items.GroupBy(w => w.Currency).Select(g => new WithdrawalSummaryItem
        {
            Currency = g.Key,
            PendingCount = g.Count(w => w.Status == WithdrawalStatus.PendingReview),
            PendingAmount = g.Where(w => w.Status == WithdrawalStatus.PendingReview).Sum(w => w.Amount),
            ApprovedCount = g.Count(w => w.Status == WithdrawalStatus.Approved),
            ApprovedAmount = g.Where(w => w.Status == WithdrawalStatus.Approved).Sum(w => w.Amount),
            PaidCount = g.Count(w => w.Status == WithdrawalStatus.Paid),
            PaidAmount = g.Where(w => w.Status == WithdrawalStatus.Paid).Sum(w => w.Amount),
            RejectedCount = g.Count(w => w.Status == WithdrawalStatus.Rejected),
            RejectedAmount = g.Where(w => w.Status == WithdrawalStatus.Rejected).Sum(w => w.Amount)
        }).ToList();
        return new WithdrawalSummaryDto { TenantId = tenantId, Items = byCurrency };
    }

    public async Task<List<WithdrawalRequestDto>> GetPendingAsync(Guid? tenantId, int skip, int take, CancellationToken ct)
    {
        if (!tenantId.HasValue) throw new ValidationException("tenantId", "TenantId is required for pending withdrawal listing");
        var items = await _reqRepo.GetByTenantAsync(tenantId.Value, WithdrawalStatus.PendingReview, skip, take, ct);
        return items.Select(MapToDto).ToList();
    }

    private async Task ReleaseLockedFunds(WithdrawalRequest wr, Guid tenantId, Guid actorUserId, string reason, CancellationToken ct)
    {
        var wallet = await _walletRepo.GetByIdForUpdateAsync(wr.WalletId, ct);
        if (wallet == null || wallet.LockedBalance < wr.Amount) return;
        var releaseEntry = wallet.UnlockFunds(wr.Amount, reason: reason,
            referenceType: "WithdrawalRequest", referenceId: wr.Id.ToString(), createdByUserId: actorUserId);
        wr.SetReleaseTransaction(releaseEntry.Id);
        await _ledgerRepo.AddAsync(releaseEntry, ct);
        await _walletRepo.UpdateAsync(wallet, ct);
    }

    private static decimal CalculateFee(TenantPaymentSettings? settings, decimal amount) => 0;

    internal static WithdrawalRequestDto MapToDto(WithdrawalRequest wr) => new()
    {
        Id = wr.Id, TenantId = wr.TenantId, ApplicationCode = wr.ApplicationCode,
        WalletId = wr.WalletId, PayoutAccountId = wr.PayoutAccountId,
        OwnerType = wr.OwnerType, OwnerId = wr.OwnerId,
        Amount = wr.Amount, FeeAmount = wr.FeeAmount, NetAmount = wr.NetAmount,
        Currency = wr.Currency, Status = wr.Status.ToString().ToLowerInvariant(),
        Description = wr.Description,
        LockedWalletTransactionId = wr.LockedWalletTransactionId,
        ReleaseWalletTransactionId = wr.ReleaseWalletTransactionId,
        CaptureWalletTransactionId = wr.CaptureWalletTransactionId,
        ApprovedByUserId = wr.ApprovedByUserId, ApprovedAt = wr.ApprovedAt,
        ApprovalNote = wr.ApprovalNote, RejectionReason = wr.RejectionReason,
        CancellationReason = wr.CancellationReason,
        BankTrackingNumber = wr.BankTrackingNumber, BankReferenceId = wr.BankReferenceId,
        PaidNote = wr.PaidNote, PaidAt = wr.PaidAt,
        CreatedAt = wr.CreatedAt, UpdatedAt = wr.UpdatedAt
    };

    private static AuditLog Audit(string action, Guid tenantId, Guid? actorUserId, WithdrawalRequest wr, string? extra = null) => new()
    {
        TenantId = tenantId, ActorUserId = actorUserId,
        Action = action, EntityType = "WithdrawalRequest", EntityId = wr.Id.ToString(),
        Reason = extra,
        AfterSnapshot = JsonSerializer.Serialize(new { wr.Amount, wr.Currency, wr.NetAmount, wr.Status })
    };
}
