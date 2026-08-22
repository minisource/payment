using Application.DTOs;
using Domain.Entities;
using Domain.Enums;
using Domain.Gateways;
using Domain.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Minisource.Common.Domain;
using Application.Features.GatewayRouting;
using Minisource.Common.Exceptions;

namespace Application.Features.Gateways;

public class GatewayPaymentService : IGatewayPaymentService
{
    private readonly IPaymentIntentRepository _intentRepo;
    private readonly IPaymentTransactionRepository _txnRepo;
    private readonly IGatewayConfigRepository _configRepo;
    private readonly IGatewayRoutingService _routingSvc;
    private readonly IGatewaySecretProtector _secretProtector;
    private readonly IGatewayProviderRepository _providerRepo;
    private readonly IPaymentGatewayAdapterFactory _adapterFactory;
    private readonly IAuditLogRepository _auditRepo;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<GatewayPaymentService> _logger;
    private readonly string _paymentPublicBaseUrl;

    public GatewayPaymentService(
        IPaymentIntentRepository intentRepo,
        IPaymentTransactionRepository txnRepo,
        IGatewayConfigRepository configRepo,
        IGatewayRoutingService routingSvc,
        IGatewaySecretProtector secretProtector,
        IGatewayProviderRepository providerRepo,
        IPaymentGatewayAdapterFactory adapterFactory,
        IAuditLogRepository auditRepo,
        IUnitOfWork uow,
        ILogger<GatewayPaymentService> logger,
        IConfiguration configuration)
    {
        _intentRepo = intentRepo;
        _txnRepo = txnRepo;
        _configRepo = configRepo;
        _routingSvc = routingSvc;
        _secretProtector = secretProtector;
        _providerRepo = providerRepo;
        _adapterFactory = adapterFactory;
        _auditRepo = auditRepo;
        _uow = uow;
        _logger = logger;
        _paymentPublicBaseUrl = configuration["Payment:PublicBaseUrl"] ?? "https://payment.example.com";
    }

    public async Task<StartPaymentIntentResponse> StartPaymentAsync(Guid tenantId, Guid paymentIntentId,
        StartPaymentIntentRequest request, string? applicationCode, CancellationToken ct)
    {
        var intent = await _intentRepo.GetByIdAsync(paymentIntentId, ct)
            ?? throw new NotFoundException("PaymentIntent", paymentIntentId);

        if (intent.TenantId != tenantId)
            throw new ForbiddenException("Payment intent does not belong to this tenant");

        if (!intent.CanStart)
            throw new BusinessException($"Cannot start payment intent in {intent.Status} status", "payment_intent_invalid_state");

        if (intent.IsExpired)
        {
            intent.MarkExpired();
            await _intentRepo.UpdateAsync(intent, ct);
            await _uow.SaveChangesAsync(ct);
            throw new BusinessException("Payment intent has expired", "payment_intent_expired");
        }

        // 1. Select gateway
        var selection = await _routingSvc.SelectGatewayAsync(new GatewaySelectionRequest(
            tenantId, applicationCode, intent.Amount, intent.Currency,
            request.PreferredProviderCode, request.GatewayConfigId), ct);

        var config = await _configRepo.GetByIdAsync(selection.GatewayConfigId, ct)
            ?? throw new BusinessException("Gateway config not found", "gateway_config_not_found");

        // 2. Decrypt secrets for adapter
        var decryptedSecrets = config.EncryptedSecrets != null
            ? await _secretProtector.DecryptSecretsAsync(config.EncryptedSecrets, ct)
            : null;

        // 3. Create payment transaction
        var transaction = PaymentTransaction.Create(
            tenantId, intent.Id, config.Id,
            selection.ProviderCode, selection.AdapterType,
            intent.Amount, intent.Currency,
            applicationCode: applicationCode);

        await _txnRepo.AddAsync(transaction, ct);

        // 4. Build callback URL
        var callbackUrl = $"{_paymentPublicBaseUrl}/api/v1/gateway-callbacks/{transaction.Id}";

        // 5. Call gateway adapter to start
        var adapter = _adapterFactory.Create(selection.AdapterType, selection.ProviderCode);
        var startRequest = new GatewayStartRequest(
            selection.ProviderCode, selection.AdapterType, config.ConfigJson,
            decryptedSecrets, intent.Amount, intent.Currency,
            transaction.Id, intent.Id, tenantId, callbackUrl,
            intent.Description);

        GatewayStartResult startResult;
        try
        {
            startResult = await adapter.StartAsync(startRequest, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gateway start failed for transaction {TxnId}", transaction.Id);
            transaction.MarkFailed("gateway_start_failed", ex.Message);
            await _txnRepo.UpdateAsync(transaction, ct);
            await _uow.SaveChangesAsync(ct);

            throw new BusinessException(
                $"Gateway start failed: {ex.Message}", "gateway_start_failed", new { TransactionId = transaction.Id });
        }

        if (!startResult.Success)
        {
            transaction.MarkFailed(startResult.ErrorCode, startResult.ErrorMessage);
            await _txnRepo.UpdateAsync(transaction, ct);
            intent.MarkFailed();
            await _intentRepo.UpdateAsync(intent, ct);
            await _uow.SaveChangesAsync(ct);

            throw new BusinessException(
                startResult.ErrorMessage ?? "Gateway start failed",
                startResult.ErrorCode ?? "gateway_start_failed");
        }

        // 6. Mark transaction as sent
        transaction.MarkSentToGateway(startResult.Authority);

        // 7. Update intent status
        intent.MarkGatewaySelected();
        intent.MarkRedirectRequired();

        await _txnRepo.UpdateAsync(transaction, ct);
        await _intentRepo.UpdateAsync(intent, ct);

        // Audit
        await _auditRepo.AddAsync(new AuditLog
        {
            TenantId = tenantId, Action = "payment_intent.started",
            EntityType = "PaymentIntent", EntityId = intent.Id.ToString(),
            AfterSnapshot = System.Text.Json.JsonSerializer.Serialize(new
            {
                transaction.Id, selection.ProviderCode, selection.Strategy
            })
        }, ct);

        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("Payment started: intent {IntentId}, transaction {TxnId}, provider {Provider}",
            intent.Id, transaction.Id, selection.ProviderCode);

        return new StartPaymentIntentResponse
        {
            PaymentIntentId = intent.Id,
            PaymentTransactionId = transaction.Id,
            Status = "redirect_required",
            ProviderCode = selection.ProviderCode,
            GatewayConfigId = selection.GatewayConfigId,
            RedirectUrl = startResult.RedirectUrl,
            ExpiresAt = intent.ExpiresAt
        };
    }
}
