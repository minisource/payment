using Application.DTOs;
using Application.Features.PaymentIntents;
using Application.Features.Gateways;
using Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Minisource.Common.Exceptions;
using Presentaion.Middlewares;

namespace payment.Controllers;

[Route("api/v1/admin")]
[Authorize(Policy = "WalletAdmin")]
public class PaymentAdminController : PaymentControllerBase
{
    private readonly IPaymentIntentRepository _intentRepo;
    private readonly IPaymentTransactionRepository _txnRepo;
    private readonly IGatewayCallbackService _callbackSvc;
    private readonly ILogger<PaymentAdminController> _logger;

    public PaymentAdminController(
        IPaymentIntentRepository intentRepo,
        IPaymentTransactionRepository txnRepo,
        IGatewayCallbackService callbackSvc,
        ILogger<PaymentAdminController> logger)
    {
        _intentRepo = intentRepo;
        _txnRepo = txnRepo;
        _callbackSvc = callbackSvc;
        _logger = logger;
    }

    // ═══ Admin Payment Intent APIs ═══════════════════

    [HttpGet("payment-intents")]
    public async Task<ActionResult<List<PaymentIntentDto>>> ListIntents(
        [FromQuery] string? status, [FromQuery] string? applicationCode,
        [FromQuery] Guid? payerUserId, [FromQuery] int skip = 0, [FromQuery] int take = 50,
        CancellationToken ct = default)
    {
        Domain.Enums.PaymentIntentStatus? statusEnum = null;
        if (status != null && Enum.TryParse<Domain.Enums.PaymentIntentStatus>(status, true, out var s))
            statusEnum = s;

        var intents = await _intentRepo.GetByTenantAsync(TenantId, skip, take, statusEnum, applicationCode, payerUserId, ct);
        return Ok(intents.Select(PaymentIntentService.MapIntentToDto).ToList());
    }

    [HttpGet("payment-intents/{paymentIntentId:guid}")]
    public async Task<ActionResult<PaymentIntentDto>> GetIntent(Guid paymentIntentId, CancellationToken ct)
    {
        var intent = await _intentRepo.GetByIdWithTransactionsAsync(paymentIntentId, ct)
            ?? throw new NotFoundException("PaymentIntent", paymentIntentId);
        return Ok(PaymentIntentService.MapIntentToDto(intent));
    }

    // ═══ Admin Payment Transaction APIs ═════════════

    [HttpGet("payment-transactions")]
    public async Task<ActionResult<List<PaymentTransactionDto>>> ListTransactions(
        [FromQuery] string? status, [FromQuery] string? providerCode,
        [FromQuery] DateTime? from, [FromQuery] DateTime? to,
        [FromQuery] int skip = 0, [FromQuery] int take = 50, CancellationToken ct = default)
    {
        Domain.Enums.PaymentTransactionStatus? statusEnum = null;
        if (status != null && Enum.TryParse<Domain.Enums.PaymentTransactionStatus>(status, true, out var s))
            statusEnum = s;

        var txns = await _txnRepo.GetByTenantAsync(TenantId, skip, take, statusEnum, providerCode, from, to, ct);
        return Ok(txns.Select(PaymentIntentService.MapTxnToDto).ToList());
    }

    [HttpGet("payment-transactions/{transactionId:guid}")]
    public async Task<ActionResult<PaymentTransactionDto>> GetTransaction(Guid transactionId, CancellationToken ct)
    {
        var txn = await _txnRepo.GetByIdAsync(transactionId, ct)
            ?? throw new NotFoundException("PaymentTransaction", transactionId);
        return Ok(PaymentIntentService.MapTxnToDto(txn));
    }

    [HttpPost("payment-transactions/{transactionId:guid}/verify")]
    public async Task<ActionResult<PaymentTransactionDto>> ManualVerify(Guid transactionId, CancellationToken ct)
    {
        var result = await _callbackSvc.ManualVerifyAsync(transactionId, GetRequiredActorUserId(), ct);
        return Ok(result);
    }
}
