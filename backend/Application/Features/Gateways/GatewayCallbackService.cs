using Application.DTOs;
using Domain.Entities;
using Domain.Enums;
using Domain.Gateways;
using Domain.Repositories;
using Microsoft.Extensions.Logging;
using Minisource.Common.Domain;
using Minisource.Common.Exceptions;
using Application.Features.PaymentWalletPosting;
using Application.Features.PaymentIntents;
using Application.Features.PaymentLinks;
using Minisource.Common.Locking;

namespace Application.Features.Gateways;

public class GatewayCallbackService : IGatewayCallbackService
{
    private readonly IPaymentIntentRepository _intentRepo;
    private readonly IPaymentTransactionRepository _txnRepo;
    private readonly IGatewayConfigRepository _configRepo;
    private readonly IGatewayProviderRepository _providerRepo;
    private readonly IGatewaySecretProtector _secretProtector;
    private readonly IPaymentGatewayAdapterFactory _adapterFactory;
    private readonly IPaymentWalletPostingService _walletPostingSvc;
    private readonly IPaymentLinkService _linkSvc;
    private readonly IAuditLogRepository _auditRepo;
    private readonly IDistributedLockService _lockService;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<GatewayCallbackService> _logger;

    public GatewayCallbackService(
        IPaymentIntentRepository intentRepo,
        IPaymentTransactionRepository txnRepo,
        IGatewayConfigRepository configRepo,
        IGatewayProviderRepository providerRepo,
        IGatewaySecretProtector secretProtector,
        IPaymentGatewayAdapterFactory adapterFactory,
        IPaymentWalletPostingService walletPostingSvc,
        IPaymentLinkService linkSvc,
        IAuditLogRepository auditRepo,
        IDistributedLockService lockService,
        IUnitOfWork uow,
        ILogger<GatewayCallbackService> logger)
    {
        _intentRepo = intentRepo;
        _txnRepo = txnRepo;
        _configRepo = configRepo;
        _providerRepo = providerRepo;
        _secretProtector = secretProtector;
        _adapterFactory = adapterFactory;
        _walletPostingSvc = walletPostingSvc;
        _linkSvc = linkSvc;
        _auditRepo = auditRepo;
        _lockService = lockService;
        _uow = uow;
        _logger = logger;
    }

    public async Task<PaymentResultDto> HandleCallbackAsync(Guid paymentTransactionId, string? queryString, string? rawBody, CancellationToken ct)
    {
        // Use distributed lock to prevent duplicate callback processing
        var lockKey = $"payment:callback:{paymentTransactionId}";
        await using var cbLock = await _lockService.AcquireAsync(lockKey, TimeSpan.FromSeconds(60), ct);
        if (!cbLock.IsAcquired)
        {
            _logger.LogWarning("Failed to acquire callback lock for transaction {TxnId}", paymentTransactionId);
            // Still try to load existing result
            return await BuildExistingResultAsync(paymentTransactionId, ct);
        }

        var transaction = await _txnRepo.GetByIdAsync(paymentTransactionId, ct)
            ?? throw new NotFoundException("PaymentTransaction", paymentTransactionId);

        // Idempotent: if already verified, return existing result
        if (transaction.Status == PaymentTransactionStatus.Verified)
            return await BuildResultAsync(transaction, ct);

        if (transaction.Status == PaymentTransactionStatus.Failed)
            return await BuildResultAsync(transaction, ct);

        var intent = await _intentRepo.GetByIdAsync(transaction.PaymentIntentId, ct)
            ?? throw new NotFoundException("PaymentIntent", transaction.PaymentIntentId);

        // 1. Mark callback received
        var safePayload = System.Text.Json.JsonSerializer.Serialize(new
        {
            queryString = SanitizePayload(queryString),
            body = SanitizePayload(rawBody)
        });
        transaction.MarkCallbackReceived(safePayload);

        // 2. Load gateway config
        var config = await _configRepo.GetByIdAsync(transaction.GatewayConfigId, ct);
        if (config == null)
        {
            _logger.LogError("Gateway config {ConfigId} not found for transaction {TxnId}",
                transaction.GatewayConfigId, transaction.Id);
            transaction.MarkFailed("gateway_config_not_found", "Gateway config not found");
            await _txnRepo.UpdateAsync(transaction, ct);
            await _uow.SaveChangesAsync(ct);
            return await BuildResultAsync(transaction, ct);
        }

        // 3. Decrypt secrets
        var decryptedSecrets = config.EncryptedSecrets != null
            ? await _secretProtector.DecryptSecretsAsync(config.EncryptedSecrets, ct)
            : null;

        // 4. Mark verify pending
        transaction.MarkVerifyPending();

        // 5. Call gateway adapter to verify
        var adapter = _adapterFactory.Create(transaction.AdapterType, transaction.ProviderCode);
        var verifyRequest = new GatewayVerifyRequest(
            transaction.ProviderCode, transaction.AdapterType, config.ConfigJson,
            decryptedSecrets, transaction.Authority ?? "",
            transaction.Amount, transaction.Currency,
            transaction.Id, transaction.PaymentIntentId, transaction.TenantId,
            queryString);

        GatewayVerifyResult verifyResult;
        try
        {
            verifyResult = await adapter.VerifyAsync(verifyRequest, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gateway verify failed for transaction {TxnId}", transaction.Id);
            transaction.MarkFailed("gateway_verify_failed", ex.Message);
            config.MarkFailure();
            await _txnRepo.UpdateAsync(transaction, ct);
            await _configRepo.UpdateAsync(config, ct);
            await _uow.SaveChangesAsync(ct);
            return await BuildResultAsync(transaction, ct);
        }

        if (!verifyResult.Success)
        {
            // Verify failed
            transaction.MarkFailed(verifyResult.ErrorCode ?? "verify_failed",
                verifyResult.ErrorMessage ?? "Gateway verification failed");
            config.MarkFailure();
            intent.MarkFailed();

            await _txnRepo.UpdateAsync(transaction, ct);
            await _configRepo.UpdateAsync(config, ct);
            await _intentRepo.UpdateAsync(intent, ct);

            await _auditRepo.AddAsync(new AuditLog
            {
                TenantId = transaction.TenantId, Action = "gateway_transaction.failed",
                EntityType = "PaymentTransaction", EntityId = transaction.Id.ToString(),
                Reason = verifyResult.ErrorMessage
            }, ct);

            await _uow.SaveChangesAsync(ct);
            return await BuildResultAsync(transaction, ct);
        }

        // 6. Verify successful
        transaction.MarkVerified(
            verifyResult.GatewayReferenceId, verifyResult.Rrn,
            verifyResult.CardPanMasked,
            System.Text.Json.JsonSerializer.Serialize(new
            {
                verifyResult.TraceNumber, verifyResult.VerifiedAmount,
                responseCode = verifyResult.ResponseCode
            }));

        config.MarkSuccess();
        intent.MarkSucceeded();

        await _txnRepo.UpdateAsync(transaction, ct);
        await _configRepo.UpdateAsync(config, ct);
        await _intentRepo.UpdateAsync(intent, ct);

        // Audit
        await _auditRepo.AddAsync(new AuditLog
        {
            TenantId = transaction.TenantId, Action = "gateway_transaction.verified",
            EntityType = "PaymentTransaction", EntityId = transaction.Id.ToString(),
            AfterSnapshot = System.Text.Json.JsonSerializer.Serialize(new
            {
                verifyResult.GatewayReferenceId, verifyResult.TraceNumber,
                verifyResult.VerifiedAmount, transaction.ProviderCode
            })
        }, ct);

        await _uow.SaveChangesAsync(ct);

        // 7. Post wallet if needed
        if (intent.WalletBehavior != WalletBehavior.None)
        {
            try
            {
                await _walletPostingSvc.PostVerifiedPaymentAsync(intent, transaction, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Wallet posting failed for intent {IntentId}", intent.Id);
                intent.MarkWalletPostingFailed();
                await _intentRepo.UpdateAsync(intent, ct);
                await _uow.SaveChangesAsync(ct);
                // Don't fail the callback — payment still succeeded, wallet posting is separate
            }
        }

        // 8. Update payment link statistics if this was a public link payment
        if (intent.PaymentLinkId.HasValue)
        {
            try
            {
                await _linkSvc.RecordSuccessfulPaymentAsync(intent.PaymentLinkId.Value, intent.Amount, ct);
                // TODO: Emit outbox event payment_link.payment_succeeded
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to update payment link stats for link {LinkId}", intent.PaymentLinkId);
            }
        }

        _logger.LogInformation("Payment verified: intent {IntentId}, transaction {TxnId}",
            intent.Id, transaction.Id);

        return await BuildResultAsync(transaction, ct);
    }

    public async Task<PaymentTransactionDto> ManualVerifyAsync(Guid paymentTransactionId, Guid actorUserId, CancellationToken ct)
    {
        var transaction = await _txnRepo.GetByIdAsync(paymentTransactionId, ct)
            ?? throw new NotFoundException("PaymentTransaction", paymentTransactionId);

        if (transaction.Status == PaymentTransactionStatus.Verified)
            return PaymentIntentService.MapTxnToDto(transaction);

        if (transaction.Status != PaymentTransactionStatus.SentToGateway
            && transaction.Status != PaymentTransactionStatus.CallbackReceived
            && transaction.Status != PaymentTransactionStatus.Failed)
            throw new BusinessException(
                $"Cannot manually verify transaction in {transaction.Status} status",
                "payment_transaction_invalid_state");

        var intent = await _intentRepo.GetByIdAsync(transaction.PaymentIntentId, ct)
            ?? throw new NotFoundException("PaymentIntent", transaction.PaymentIntentId);

        var config = await _configRepo.GetByIdAsync(transaction.GatewayConfigId, ct)
            ?? throw new BusinessException("Gateway config not found", "gateway_config_not_found");

        var decryptedSecrets = config.EncryptedSecrets != null
            ? await _secretProtector.DecryptSecretsAsync(config.EncryptedSecrets, ct)
            : null;

        var adapter = _adapterFactory.Create(transaction.AdapterType, transaction.ProviderCode);
        var verifyRequest = new GatewayVerifyRequest(
            transaction.ProviderCode, transaction.AdapterType, config.ConfigJson,
            decryptedSecrets, transaction.Authority ?? "",
            transaction.Amount, transaction.Currency,
            transaction.Id, transaction.PaymentIntentId, transaction.TenantId,
            null);

        var verifyResult = await adapter.VerifyAsync(verifyRequest, ct);

        if (!verifyResult.Success)
        {
            transaction.MarkFailed(verifyResult.ErrorCode, verifyResult.ErrorMessage);
            config.MarkFailure();
            await _txnRepo.UpdateAsync(transaction, ct);
            await _configRepo.UpdateAsync(config, ct);
            await _uow.SaveChangesAsync(ct);

            throw new BusinessException(
                verifyResult.ErrorMessage ?? "Manual verification failed",
                verifyResult.ErrorCode ?? "gateway_verify_failed");
        }

        transaction.MarkVerified(
            verifyResult.GatewayReferenceId, verifyResult.Rrn,
            verifyResult.CardPanMasked);

        config.MarkSuccess();

        if (intent.Status != PaymentIntentStatus.Succeeded)
            intent.MarkSucceeded();

        await _txnRepo.UpdateAsync(transaction, ct);
        await _configRepo.UpdateAsync(config, ct);
        await _intentRepo.UpdateAsync(intent, ct);

        await _auditRepo.AddAsync(new AuditLog
        {
            TenantId = transaction.TenantId, ActorUserId = actorUserId,
            Action = "gateway_transaction.verified_admin",
            EntityType = "PaymentTransaction", EntityId = transaction.Id.ToString()
        }, ct);

        await _uow.SaveChangesAsync(ct);

        return PaymentIntentService.MapTxnToDto(transaction);
    }

    private async Task<PaymentResultDto> BuildResultAsync(PaymentTransaction transaction, CancellationToken ct)
    {
        var intent = await _intentRepo.GetByIdAsync(transaction.PaymentIntentId, ct);
        var provider = await _providerRepo.GetByCodeAsync(transaction.ProviderCode, ct);

        return new PaymentResultDto
        {
            PaymentIntentId = intent?.Id ?? transaction.PaymentIntentId,
            PaymentTransactionId = transaction.Id,
            Status = intent?.Status.ToString() ?? transaction.Status.ToString(),
            ProviderDisplayName = provider?.DisplayName ?? transaction.ProviderCode,
            Amount = transaction.Amount,
            Currency = transaction.Currency,
            GatewayReferenceId = transaction.GatewayReferenceId,
            TraceNumber = transaction.TraceNumber,
            ReturnUrl = intent?.ReturnUrl,
            CompletedAt = transaction.VerifiedAt ?? intent?.SucceededAt
        };
    }

    private async Task<PaymentResultDto> BuildExistingResultAsync(Guid paymentTransactionId, CancellationToken ct)
    {
        var transaction = await _txnRepo.GetByIdAsync(paymentTransactionId, ct);
        if (transaction == null)
            return new PaymentResultDto { Status = "unknown", PaymentTransactionId = paymentTransactionId };
        return await BuildResultAsync(transaction, ct);
    }

    private static string? SanitizePayload(string? input)
    {
        if (input == null) return null;
        // Remove sensitive patterns (mask card numbers, etc.)
        // Simple length limit for safety
        return input.Length > 10000 ? input[..10000] : input;
    }
}
