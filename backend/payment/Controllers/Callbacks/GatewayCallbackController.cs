using Application.Features.Gateways;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentaion.Middlewares;

namespace payment.Controllers;

/// <summary>
/// Public callback endpoint for bank/gateway callbacks.
/// No authentication — this is called by external gateway redirects.
/// </summary>
[Route("api/v1/gateway-callbacks")]
[AllowAnonymous]
public class GatewayCallbackController : PaymentControllerBase
{
    private readonly IGatewayCallbackService _callbackSvc;
    private readonly ILogger<GatewayCallbackController> _logger;

    public GatewayCallbackController(IGatewayCallbackService callbackSvc, ILogger<GatewayCallbackController> logger)
    {
        _callbackSvc = callbackSvc;
        _logger = logger;
    }

    /// <summary>Handle GET callback from bank/gateway.</summary>
    [HttpGet("{paymentTransactionId:guid}")]
    public async Task<IActionResult> GetCallback(Guid paymentTransactionId, CancellationToken ct)
    {
        _logger.LogInformation("GET callback received for transaction {TxnId}", paymentTransactionId);

        var queryString = Request.QueryString.HasValue ? Request.QueryString.Value : null;
        var result = await _callbackSvc.HandleCallbackAsync(paymentTransactionId, queryString, null, ct);

        // Redirect to result page
        if (result.PaymentTransactionId.HasValue)
            return Redirect($"/result/payment-transactions/{result.PaymentTransactionId}");
        return Redirect($"/result/payment-intents/{result.PaymentIntentId}");
    }

    /// <summary>Handle POST callback from bank/gateway.</summary>
    [HttpPost("{paymentTransactionId:guid}")]
    public async Task<IActionResult> PostCallback(Guid paymentTransactionId, CancellationToken ct)
    {
        _logger.LogInformation("POST callback received for transaction {TxnId}", paymentTransactionId);

        string? rawBody = null;
        using (var reader = new StreamReader(Request.Body))
        {
            rawBody = await reader.ReadToEndAsync(ct);
        }

        var result = await _callbackSvc.HandleCallbackAsync(paymentTransactionId, Request.QueryString.Value, rawBody, ct);

        if (result.PaymentTransactionId.HasValue)
            return Redirect($"/result/payment-transactions/{result.PaymentTransactionId}");
        return Redirect($"/result/payment-intents/{result.PaymentIntentId}");
    }
}
