using Application.DTOs;
using Application.Features.PaymentLinks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentaion.Middlewares;

namespace payment.Controllers;

/// <summary>
/// Public endpoints for payment link pages and starting payment from a public link.
/// No authentication — these are public-facing endpoints.
/// </summary>
[Route("api/v1/public")]
[AllowAnonymous]
public class PublicPaymentLinkController : PaymentControllerBase
{
    private readonly IPublicPaymentService _publicSvc;
    private readonly ILogger<PublicPaymentLinkController> _logger;

    public PublicPaymentLinkController(IPublicPaymentService publicSvc, ILogger<PublicPaymentLinkController> logger)
    {
        _publicSvc = publicSvc;
        _logger = logger;
    }

    /// <summary>Get public-safe payment link data for display.</summary>
    [HttpGet("payment-links/{token}")]
    public async Task<ActionResult<PublicPaymentLinkResponse>> GetPublicLink(string token, CancellationToken ct)
    {
        var result = await _publicSvc.GetPublicLinkAsync(token, ct);
        return Ok(result);
    }

    /// <summary>Start a public gateway payment from a payment link.</summary>
    [HttpPost("payment-links/{token}/pay")]
    public async Task<ActionResult<PublicPayResponse>> StartPublicPayment(
        string token, [FromBody] PublicPayRequest request, CancellationToken ct)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var applicationCode = Request.Headers["X-Application-Code"].FirstOrDefault();

        var result = await _publicSvc.StartPublicPaymentAsync(token, request, applicationCode, ipAddress, ct);
        return Ok(result);
    }
}
