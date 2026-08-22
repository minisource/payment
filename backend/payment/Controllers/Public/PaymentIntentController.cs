using Application.DTOs;
using Application.Features.PaymentIntents;
using Application.Features.Gateways;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Minisource.Common.Exceptions;
using Presentaion.Middlewares;

namespace payment.Controllers;

[Route("api/v1")]
public class PaymentIntentController : PaymentControllerBase
{
    private readonly IPaymentIntentService _intentSvc;
    private readonly IGatewayPaymentService _gatewaySvc;
    private readonly ILogger<PaymentIntentController> _logger;

    public PaymentIntentController(
        IPaymentIntentService intentSvc,
        IGatewayPaymentService gatewaySvc,
        ILogger<PaymentIntentController> logger)
    {
        _intentSvc = intentSvc;
        _gatewaySvc = gatewaySvc;
        _logger = logger;
    }

    /// <summary>Create a new payment intent.</summary>
    [HttpPost("payment-intents")]
    public async Task<ActionResult<PaymentIntentDto>> Create(
        [FromBody] CreatePaymentIntentRequest request, CancellationToken ct)
    {
        var result = await _intentSvc.CreateAsync(GetRequiredTenantId(), GetRequiredActorUserId(), request, ApplicationCode, IdempotencyKey, ct);
        return Ok(result);
    }

    /// <summary>Get payment intent by ID.</summary>
    [HttpGet("payment-intents/{paymentIntentId:guid}")]
    public async Task<ActionResult<PaymentIntentDto>> Get(Guid paymentIntentId, CancellationToken ct)
    {
        return Ok(await _intentSvc.GetAsync(paymentIntentId, ct));
    }

    /// <summary>List user's payment intents.</summary>
    [HttpGet("payment-intents")]
    public async Task<ActionResult<List<PaymentIntentDto>>> List(
        [FromQuery] int skip = 0, [FromQuery] int take = 50, CancellationToken ct = default)
    {
        return Ok(await _intentSvc.GetUserIntentsAsync(GetRequiredTenantId(), GetRequiredActorUserId(), skip, take, ct));
    }

    /// <summary>Start/pay a payment intent — redirect to gateway.</summary>
    [HttpPost("payment-intents/{paymentIntentId:guid}/start")]
    public async Task<ActionResult<StartPaymentIntentResponse>> Start(
        Guid paymentIntentId, [FromBody] StartPaymentIntentRequest request, CancellationToken ct)
    {
        var result = await _gatewaySvc.StartPaymentAsync(GetRequiredTenantId(), paymentIntentId, request, ApplicationCode, ct);
        return Ok(result);
    }

    /// <summary>Cancel a payment intent.</summary>
    [HttpPost("payment-intents/{paymentIntentId:guid}/cancel")]
    public async Task<ActionResult<PaymentIntentDto>> Cancel(
        Guid paymentIntentId, [FromBody] CancelRequest request, CancellationToken ct)
    {
        return Ok(await _intentSvc.CancelAsync(GetRequiredTenantId(), paymentIntentId, request.Reason ?? "Cancelled by user", ct));
    }

    /// <summary>Get transactions for a payment intent.</summary>
    [HttpGet("payment-intents/{paymentIntentId:guid}/transactions")]
    public async Task<ActionResult<List<PaymentTransactionDto>>> GetTransactions(
        Guid paymentIntentId, CancellationToken ct)
    {
        return Ok(await _intentSvc.GetTransactionsAsync(paymentIntentId, ct));
    }
}

public class CancelRequest
{
    public string? Reason { get; set; }
}
