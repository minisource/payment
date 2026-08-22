using Application.DTOs;
using Application.Features.PaymentIntents;
using Application.Features.Gateways;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentaion.Middlewares;

namespace payment.Controllers;

/// <summary>
/// Payment result pages. Public-facing HTML pages shown after gateway callback.
/// </summary>
[Route("result")]
[AllowAnonymous]
public class ResultPageController : PaymentControllerBase
{
    private readonly IPaymentIntentService _intentSvc;
    private readonly IGatewayConfigService _configSvc;

    public ResultPageController(IPaymentIntentService intentSvc, IGatewayConfigService configSvc)
    {
        _intentSvc = intentSvc;
        _configSvc = configSvc;
    }

    /// <summary>Show result for a payment intent.</summary>
    [HttpGet("payment-intents/{paymentIntentId:guid}")]
    public async Task<IActionResult> IntentResult(Guid paymentIntentId, CancellationToken ct)
    {
        try
        {
            var intent = await _intentSvc.GetAsync(paymentIntentId, ct);
            var result = new PaymentResultDto
            {
                PaymentIntentId = intent.Id,
                Status = intent.Status,
                Amount = intent.Amount,
                Currency = intent.Currency,
                ReturnUrl = intent.ReturnUrl,
                CompletedAt = intent.SucceededAt ?? intent.FailedAt ?? intent.CancelledAt
            };

            // Return JSON result (TODO: Add Razor views for HTML result page)
            return Ok(result);
        }
        catch
        {
            return Ok(new PaymentResultDto
            {
                PaymentIntentId = paymentIntentId,
                Status = "unknown",
                Amount = 0,
                Currency = "IRT"
            });
        }
    }

    /// <summary>Show result for a payment transaction.</summary>
    [HttpGet("payment-transactions/{paymentTransactionId:guid}")]
    public IActionResult TransactionResult(Guid paymentTransactionId)
    {
        // TODO: Add Razor views for HTML result page
        return Ok(new PaymentResultDto
        {
            PaymentTransactionId = paymentTransactionId,
            Status = "processing",
            Amount = 0,
            Currency = "IRT"
        });
    }
}
