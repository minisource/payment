using Application.DTOs;
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
/// Refund service implementing strict pre-hold policy:
/// wallet hold is created BEFORE gateway refund is called.
/// If hold fails (insufficient balance), gateway is never called.
/// </summary>
public class RefundService : IRefundService
{
    private readonly IRefundRequestRepository _refundRepo;
    private readonly IPaymentTransactionRepository _txnRepo;
    private readonly IWalletHoldService _holdService;
    private readonly IWalletAccountRepository _walletRepo;
    private readonly IGatewayConfigRepository _gatewayConfigRepo;
    private readonly IGatewaySecretProtector _secretProtector;
    private readonly IDistributedLockService _lockService;
    private readonly IUnitOfWork _uow;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RefundService> _logger;

    public RefundService(
        IRefundRequestRepository refundRepo,
        IPaymentTransactionRepository txnRepo,
        IWalletHoldService holdService,
        IWalletAccountRepository walletRepo,
        IGatewayConfigRepository gatewayConfigRepo,
        IGatewaySecretProtector secretProtector,
        IDistributedLockService lockService,
        IUnitOfWork uow,
        IServiceProvider serviceProvider,
        ILogger<RefundService> logger)
    {
        _refundRepo = refundRepo;
        _txnRepo = txnRepo;
        _holdService = holdService;
        _walletRepo = walletRepo;
        _gatewayConfigRepo = gatewayConfigRepo;
        _secretProtector = secretProtector;
        _lockService = lockService;
        _uow = uow;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    // ─── Create ──────────────────────────────────────────────

    public async Task<RefundRequestDto> CreateAsync(
        Guid tenantId,
        CreateRefundRequest req,
        string requestedByUserId,
        string? applicationCode = null,
        CancellationToken ct = default)
    {
        // Validate
        if (req.Amount <= 0)
            throw new ValidationException("Amount", "Refund amount must be positive");
        if (string.IsNullOrWhiteSpace(req.Reason))
            throw new ValidationException("Reason", "Refund reason is required");

        // Check idempotency
        if (!string.IsNullOrWhiteSpace(req.IdempotencyKey))
        {
            var existing = await _refundRepo.GetByIdempotencyKeyAsync(tenantId, req.IdempotencyKey, ct);
            if (existing is not null)
                return MapToDto(existing);
        }

        // Load payment transaction
        var txn = await _txnRepo.GetByIdAsync(req.PaymentTransactionId, ct)
            ?? throw new NotFoundException("PaymentTransaction", req.PaymentTransactionId.ToString());

        // Eligibility checks
        EnsureRefundable(txn, req);

        // Check total already refunded
        var totalRefunded = await _refundRepo.GetTotalRefundedAsync(req.PaymentTransactionId, ct);
        if (totalRefunded + req.Amount > txn.Amount)
            throw new ValidationException("Amount", "Refund amount exceeds remaining refundable amount");

        // Resolve wallet account for this tenant and currency
        var wallets = await _walletRepo.GetByTenantAsync(tenantId, skip: 0, take: 100, ct: ct);
        var wallet = wallets.FirstOrDefault(w =>
            w.Currency == req.Currency.ToUpperInvariant() && w.Status == WalletAccountStatus.Active);
        if (wallet is null)
            throw new NotFoundException("WalletAccount",
                $"No active wallet found for tenant {tenantId} currency {req.Currency}");

        // Create refund request — uses ORIGINAL gateway from payment transaction
        var refund = RefundRequest.Create(
            tenantId,
            txn.Id,
            txn.PaymentIntentId,
            txn.GatewayConfigId,
            txn.ProviderCode,
            $"gateway-{txn.ProviderCode}",
            req.Amount,
            req.Currency,
            req.Reason,
            requestedByUserId,
            requestSource: "admin",
            applicationCode: applicationCode,
            idempotencyKey: req.IdempotencyKey,
            walletId: wallet.Id);

        refund.MarkPendingReview();

        await _refundRepo.AddAsync(refund, ct);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("Refund created: {RefundId} for transaction {TxnId}, amount {Amount}",
            refund.Id, req.PaymentTransactionId, req.Amount);

        return MapToDto(refund);
    }

    // ─── Submit / Approve / Reject / Cancel ──────────────────

    public async Task<RefundRequestDto> SubmitForReviewAsync(Guid refundId, CancellationToken ct = default)
    {
        var refund = await _refundRepo.GetByIdForUpdateAsync(refundId, ct)
            ?? throw new NotFoundException("RefundRequest", refundId.ToString());

        refund.MarkPendingReview();
        await _refundRepo.UpdateAsync(refund, ct);
        await _uow.SaveChangesAsync(ct);
        return MapToDto(refund);
    }

    public async Task<RefundRequestDto> ApproveAsync(
        Guid refundId, string approvedByUserId, ApproveRefundRequest req, CancellationToken ct = default)
    {
        var refund = await _refundRepo.GetByIdForUpdateAsync(refundId, ct)
            ?? throw new NotFoundException("RefundRequest", refundId.ToString());

        refund.Approve(approvedByUserId, req.AdminNote);
        await _refundRepo.UpdateAsync(refund, ct);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("Refund approved: {RefundId} by {UserId}", refundId, approvedByUserId);
        return MapToDto(refund);
    }

    public async Task<RefundRequestDto> RejectAsync(
        Guid refundId, string rejectedByUserId, RejectRefundRequest req, CancellationToken ct = default)
    {
        var refund = await _refundRepo.GetByIdForUpdateAsync(refundId, ct)
            ?? throw new NotFoundException("RefundRequest", refundId.ToString());

        if (string.IsNullOrWhiteSpace(req.Reason))
            throw new ValidationException("Reason", "Reject reason is required");

        refund.Reject(req.Reason, rejectedByUserId);
        await _refundRepo.UpdateAsync(refund, ct);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("Refund rejected: {RefundId}", refundId);
        return MapToDto(refund);
    }

    public async Task<RefundRequestDto> CancelAsync(Guid refundId, CancellationToken ct = default)
    {
        var refund = await _refundRepo.GetByIdForUpdateAsync(refundId, ct)
            ?? throw new NotFoundException("RefundRequest", refundId.ToString());

        refund.Cancel();
        await _refundRepo.UpdateAsync(refund, ct);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("Refund cancelled: {RefundId}", refundId);
        return MapToDto(refund);
    }

    // ─── Process (strict pre-hold) ───────────────────────────

    public async Task<RefundRequestDto> ProcessAsync(
        Guid refundId, string processedByUserId, CancellationToken ct = default)
    {
        var lockKey = $"payment:refund:process:{refundId}";
        await using var refundLock = await _lockService.AcquireAsync(lockKey, TimeSpan.FromSeconds(60), ct);
        if (!refundLock.IsAcquired)
            throw new ConflictException("Another refund process is in progress for this request");

        await using var txn = await _uow.BeginTransactionAsync(ct);

        try
        {
            var refund = await _refundRepo.GetByIdForUpdateAsync(refundId, ct)
                ?? throw new NotFoundException("RefundRequest", refundId.ToString());

            // Idempotent: if already completed, return as-is
            if (refund.IsTerminal)
                return MapToDto(refund);

            if (!refund.CanProcess)
                throw new BusinessException("refund_invalid_state",
                    $"Refund {refundId} has status {refund.Status}, cannot process");

            // Ensure wallet is known
            if (!refund.WalletId.HasValue)
                throw new BusinessException("refund_no_wallet", "Refund has no associated wallet");

            // Step 1: Mark hold pending
            refund.MarkHoldPending();

            // Step 2: Validate wallet exists (read-only, no lock needed here)
            var walletExists = await _walletRepo.GetByIdAsync(refund.WalletId.Value, ct);
            if (walletExists is null)
                throw new NotFoundException("WalletAccount", refund.WalletId.Value.ToString());

            // Step 3: Create wallet hold (atomic — gateway NOT called if this fails)
            var holdResult = await _holdService.CreateHoldAsync(
                new CreateWalletHoldRequest(
                    TenantId: refund.TenantId,
                    WalletAccountId: refund.WalletId.Value,
                    Amount: refund.Amount,
                    Currency: refund.Currency,
                    Reason: refund.Reason,
                    ReferenceType: "refund",
                    ReferenceId: refund.Id,
                    ApplicationCode: refund.ApplicationCode,
                    CreatedByUserId: processedByUserId,
                    IdempotencyKey: $"refund:{refund.Id}:hold"),
                ct);

            if (!holdResult.IsSuccess)
            {
                refund.MarkHoldFailed(holdResult.ErrorCode!, holdResult.ErrorMessage!,
                    requiresManualReview: false);
                await _refundRepo.UpdateAsync(refund, ct);
                await _uow.SaveChangesAsync(ct);
                await txn.CommitAsync(ct);
                return MapToDto(refund);
            }

            // Step 4: Mark hold created + start processing
            refund.MarkHoldCreated(holdResult.HoldId!.Value);
            refund.StartProcessing(processedByUserId);

            // Step 3: Load the payment transaction to get the original gateway
            var paymentTxn = await _txnRepo.GetByIdAsync(refund.PaymentTransactionId, ct)
                ?? throw new NotFoundException("PaymentTransaction", refund.PaymentTransactionId.ToString());

            // CRITICAL: Must use the original gateway from the payment transaction
            EnsureRefundUsesOriginalGateway(paymentTxn, refund);

            // Step 4: Load gateway config from DB for actual credentials
            var gatewayConfig = await _gatewayConfigRepo.GetByIdAsync(refund.GatewayConfigId, ct);
            if (gatewayConfig is null)
            {
                refund.MarkGatewayFailed("gateway_config_not_found",
                    $"Gateway config {refund.GatewayConfigId} not found", requiresManualReview: true);
                await _holdService.ReleaseHoldAsync(new ReleaseWalletHoldRequest(
                    holdResult.HoldId!.Value, "Gateway config not found"), ct);
                await _refundRepo.UpdateAsync(refund, ct);
                await _uow.SaveChangesAsync(ct);
                await txn.CommitAsync(ct);
                return MapToDto(refund);
            }

            refund.MarkGatewaySubmitted();

            var gatewayRequest = new RefundGatewayRequest(
                RefundRequestId: refund.Id,
                PaymentTransactionId: refund.PaymentTransactionId,
                GatewayConfigId: refund.GatewayConfigId,
                ProviderCode: refund.ProviderCode,
                GatewayName: refund.GatewayName,
                AdapterType: paymentTxn.AdapterType,
                ConfigJson: gatewayConfig.ConfigJson,
                EncryptedSecrets: gatewayConfig.EncryptedSecrets,
                Amount: refund.Amount.ToString("F0"),
                Currency: refund.Currency,
                OriginalGatewayReferenceId: paymentTxn.GatewayReferenceId ?? "",
                OriginalTrackingCode: paymentTxn.TraceNumber ?? paymentTxn.Authority ?? "",
                TenantId: refund.TenantId,
                IsPartial: refund.Amount < paymentTxn.Amount);

            // Create attempt record
            var attempt = refund.AddAttempt(
                refund.Attempts.Count + 1,
                requestPayloadSafe: $"refund request: {gatewayRequest.RefundRequestId}");

            // Call the gateway refund adapter
            var gatewayResult = await CallGatewayRefundAsync(gatewayRequest, refund, ct);

            if (gatewayResult is null)
            {
                // No adapter available — mark as failed
                attempt.MarkFailed("refund_adapter_not_found",
                    $"No refund adapter for {refund.ProviderCode}");
                refund.MarkGatewayFailed("refund_adapter_not_found",
                    "No refund adapter configured", requiresManualReview: true);

                // Release hold since gateway couldn't be called
                await _holdService.ReleaseHoldAsync(new ReleaseWalletHoldRequest(
                    holdResult.HoldId!.Value,
                    "Gateway adapter not available",
                    ReleasedByUserId: processedByUserId), ct);

                await _refundRepo.UpdateAsync(refund, ct);
                await _uow.SaveChangesAsync(ct);
                await txn.CommitAsync(ct);
                return MapToDto(refund);
            }

            if (!gatewayResult.IsSuccess)
            {
                attempt.MarkFailed(
                    gatewayResult.ErrorCode ?? "gateway_failed",
                    gatewayResult.ErrorMessage ?? "Gateway refund failed",
                    responsePayloadSafe: gatewayResult.SafeResponse?.ToString());

                refund.MarkGatewayFailed(
                    gatewayResult.ErrorCode ?? "gateway_failed",
                    gatewayResult.ErrorMessage ?? "Gateway refund failed",
                    requiresManualReview: gatewayResult.RequiresManualReview);

                // Release hold — gateway failed
                var release = await _holdService.ReleaseHoldAsync(new ReleaseWalletHoldRequest(
                    holdResult.HoldId!.Value,
                    $"Gateway refund failed: {gatewayResult.ErrorCode}",
                    ReleasedByUserId: processedByUserId), ct);

                if (!release.IsSuccess)
                    refund.MarkRequiresManualReview("refund_hold_release_failed",
                        "Gateway failed and hold release also failed");

                await _refundRepo.UpdateAsync(refund, ct);
                await _uow.SaveChangesAsync(ct);
                await txn.CommitAsync(ct);

                _logger.LogWarning("Gateway refund failed for {RefundId}: {Error}",
                    refundId, gatewayResult.ErrorCode);
                return MapToDto(refund);
            }

            // Gateway succeeded
            attempt.MarkSucceeded(
                gatewayResult.GatewayRefundId,
                gatewayResult.GatewayTrackingCode,
                gatewayResult.GatewayStatus,
                responsePayloadSafe: gatewayResult.SafeResponse?.ToString());

            refund.MarkGatewaySucceeded(
                gatewayResult.GatewayRefundId,
                gatewayResult.GatewayTrackingCode,
                safeResponse: gatewayResult.SafeResponse?.ToString());

            // Step 5: Capture hold (permanent wallet debit)
            refund.MarkWalletDebitPending();

            var capture = await _holdService.CaptureHoldAsync(new CaptureWalletHoldRequest(
                holdResult.HoldId!.Value,
                $"Gateway refund succeeded: {gatewayResult.GatewayRefundId}",
                CapturedByUserId: processedByUserId), ct);

            if (!capture.IsSuccess)
            {
                refund.MarkRequiresManualReview("refund_hold_capture_failed",
                    "Gateway refund succeeded but hold capture failed");
                await _refundRepo.UpdateAsync(refund, ct);
                await _uow.SaveChangesAsync(ct);
                await txn.CommitAsync(ct);

                _logger.LogError("Hold capture failed for refund {RefundId} after gateway success!", refundId);
                return MapToDto(refund);
            }

            refund.MarkWalletDebited();
            refund.MarkCompleted();

            await _refundRepo.UpdateAsync(refund, ct);
            await _uow.SaveChangesAsync(ct);
            await txn.CommitAsync(ct);

            _logger.LogInformation("Refund completed: {RefundId}, gatewayRefundId: {GwRefundId}",
                refundId, gatewayResult.GatewayRefundId);

            return MapToDto(refund);
        }
        catch (Exception ex) when (ex is not AppException)
        {
            await txn.RollbackAsync(ct);
            _logger.LogError(ex, "Unexpected error processing refund {RefundId}", refundId);
            throw new BusinessException("refund_process_failed",
                $"Failed to process refund: {ex.Message}", ex);
        }
    }

    // ─── Retry ──────────────────────────────────────────────

    public async Task<RefundRequestDto> RetryAsync(Guid refundId, string processedByUserId, CancellationToken ct = default)
    {
        var refund = await _refundRepo.GetByIdForUpdateAsync(refundId, ct)
            ?? throw new NotFoundException("RefundRequest", refundId.ToString());

        if (!refund.CanRetry)
            throw new BusinessException("refund_invalid_state",
                $"Refund {refundId} cannot be retried in status {refund.Status}");

        // If hold was released, need to re-create hold
        // For simplicity, go through the full process flow
        return await ProcessAsync(refundId, processedByUserId, ct);
    }

    // ─── Manual Review ──────────────────────────────────────

    public async Task<RefundRequestDto> MarkManualReviewAsync(
        Guid refundId, string failureCode, string failureMessage, CancellationToken ct = default)
    {
        var refund = await _refundRepo.GetByIdForUpdateAsync(refundId, ct)
            ?? throw new NotFoundException("RefundRequest", refundId.ToString());

        refund.MarkRequiresManualReview(failureCode, failureMessage);
        await _refundRepo.UpdateAsync(refund, ct);
        await _uow.SaveChangesAsync(ct);

        _logger.LogWarning("Refund marked manual review: {RefundId} — {Code}", refundId, failureCode);
        return MapToDto(refund);
    }

    // ─── Helpers ────────────────────────────────────────────

    private static void EnsureRefundable(PaymentTransaction txn, CreateRefundRequest req)
    {
        if (txn.Status != PaymentTransactionStatus.Verified)
            throw new BusinessException("refund_not_allowed",
                $"Transaction {txn.Id} has status {txn.Status}, cannot refund");

        if (!string.Equals(txn.Currency, req.Currency, StringComparison.OrdinalIgnoreCase))
            throw new ValidationException("Currency",
                $"Currency mismatch: transaction is {txn.Currency}, requested {req.Currency}");

        if (req.Amount > txn.Amount)
            throw new ValidationException("Amount",
                $"Refund amount {req.Amount} exceeds transaction amount {txn.Amount}");
    }

    private static void EnsureRefundUsesOriginalGateway(PaymentTransaction txn, RefundRequest refund)
    {
        if (txn.GatewayConfigId != refund.GatewayConfigId)
            throw new BusinessException("refund_gateway_mismatch",
                $"Refund gateway ({refund.GatewayConfigId}) does not match original payment gateway ({txn.GatewayConfigId})");
        if (!string.Equals(txn.ProviderCode, refund.ProviderCode, StringComparison.OrdinalIgnoreCase))
            throw new BusinessException("refund_gateway_mismatch",
                $"Refund provider ({refund.ProviderCode}) does not match original ({txn.ProviderCode})");
    }

    private async Task<RefundGatewayResult?> CallGatewayRefundAsync(
        RefundGatewayRequest request, RefundRequest refund, CancellationToken ct)
    {
        // Resolve adapter by provider code
        IRefundGatewayAdapter? adapter = refund.ProviderCode.ToLowerInvariant() switch
        {
            "azkivam" => _serviceProvider.GetService(typeof(Infrastructure.Gateways.CustomGateways.AzkiVam.AzkiVamGatewayAdapter)) as IRefundGatewayAdapter,
            "tara" => _serviceProvider.GetService(typeof(Infrastructure.Gateways.CustomGateways.Tara.TaraGatewayAdapter)) as IRefundGatewayAdapter,
            _ => null
        };

        if (adapter is null)
        {
            _logger.LogWarning("No refund adapter found for provider {Provider}", refund.ProviderCode);
            return null;
        }

        return await adapter.RefundAsync(request, ct);
    }

    // ─── Mapping ─────────────────────────────────────────────

    private static RefundRequestDto MapToDto(RefundRequest r) => new()
    {
        Id = r.Id,
        TenantId = r.TenantId,
        ApplicationCode = r.ApplicationCode,
        PaymentIntentId = r.PaymentIntentId,
        PaymentTransactionId = r.PaymentTransactionId,
        WalletId = r.WalletId,
        RequestedByUserId = r.RequestedByUserId,
        RequestedByAdminId = r.RequestedByAdminId,
        RequestSource = r.RequestSource,
        Amount = r.Amount,
        Currency = r.Currency,
        Reason = r.Reason,
        AdminNote = r.AdminNote,
        RejectReason = r.RejectReason,
        Status = r.Status.ToString(),
        GatewayConfigId = r.GatewayConfigId,
        ProviderCode = r.ProviderCode,
        GatewayName = r.GatewayName,
        GatewayRefundId = r.GatewayRefundId,
        GatewayTrackingCode = r.GatewayTrackingCode,
        GatewayReferenceId = r.GatewayReferenceId,
        WalletHoldId = r.WalletHoldId,
        ApprovedByUserId = r.ApprovedByUserId,
        ApprovedAt = r.ApprovedAt,
        ProcessedByUserId = r.ProcessedByUserId,
        ProcessedAt = r.ProcessedAt,
        CompletedAt = r.CompletedAt,
        FailedAt = r.FailedAt,
        FailureCode = r.FailureCode,
        FailureMessage = r.FailureMessage,
        IdempotencyKey = r.IdempotencyKey,
        CorrelationId = r.CorrelationId,
        RequestId = r.RequestId,
        CreatedAt = r.CreatedAt,
        UpdatedAt = r.UpdatedAt,
        Attempts = r.Attempts.Select(a => new RefundGatewayAttemptDto
        {
            Id = a.Id,
            AttemptNo = a.AttemptNo,
            Status = a.Status.ToString(),
            GatewayRefundId = a.GatewayRefundId,
            GatewayTrackingCode = a.GatewayTrackingCode,
            GatewayStatus = a.GatewayStatus,
            HttpStatusCode = a.HttpStatusCode,
            ErrorCode = a.ErrorCode,
            ErrorMessage = a.ErrorMessage,
            StartedAt = a.StartedAt,
            FinishedAt = a.FinishedAt,
        }).ToList()
    };
}
