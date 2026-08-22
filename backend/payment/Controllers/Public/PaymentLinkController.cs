using Application.DTOs;
using Application.Features.PaymentLinks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Minisource.Common.Exceptions;
using Presentaion.Middlewares;

namespace payment.Controllers;

[Route("api/v1")]
public class PaymentLinkController : PaymentControllerBase
{
    private readonly IPaymentLinkService _linkSvc;
    private readonly ILogger<PaymentLinkController> _logger;

    public PaymentLinkController(IPaymentLinkService linkSvc, ILogger<PaymentLinkController> logger)
    {
        _linkSvc = linkSvc;
        _logger = logger;
    }

    private string? PublicBaseUrl => $"{Request.Scheme}://{Request.Host}";

    /// <summary>Create a new payment link.</summary>
    [HttpPost("payment-links")]
    public async Task<ActionResult<CreatePaymentLinkResponse>> Create(
        [FromBody] CreatePaymentLinkRequest request, CancellationToken ct)
    {
        var result = await _linkSvc.CreateAsync(GetRequiredTenantId(), GetRequiredActorUserId(), request, ApplicationCode, PublicBaseUrl, ct);
        return Ok(result);
    }

    /// <summary>List owner's payment links.</summary>
    [HttpGet("payment-links")]
    public async Task<ActionResult<List<PaymentLinkDto>>> List(
        [FromQuery] int skip = 0, [FromQuery] int take = 50, CancellationToken ct = default)
    {
        return Ok(await _linkSvc.GetOwnerLinksAsync(GetRequiredTenantId(), "user", GetRequiredActorUserId(), skip, take, PublicBaseUrl, ct));
    }

    /// <summary>Get payment link by ID.</summary>
    [HttpGet("payment-links/{paymentLinkId:guid}")]
    public async Task<ActionResult<PaymentLinkDto>> Get(Guid paymentLinkId, CancellationToken ct)
    {
        return Ok(await _linkSvc.GetAsync(paymentLinkId, GetRequiredTenantId(), PublicBaseUrl, ct));
    }

    /// <summary>Update payment link.</summary>
    [HttpPatch("payment-links/{paymentLinkId:guid}")]
    public async Task<ActionResult<PaymentLinkDto>> Update(
        Guid paymentLinkId, [FromBody] UpdatePaymentLinkRequest request, CancellationToken ct)
    {
        return Ok(await _linkSvc.UpdateAsync(paymentLinkId, GetRequiredTenantId(), GetRequiredActorUserId(), request, ct));
    }

    /// <summary>Pause payment link.</summary>
    [HttpPost("payment-links/{paymentLinkId:guid}/pause")]
    public async Task<IActionResult> Pause(Guid paymentLinkId, CancellationToken ct)
    {
        await _linkSvc.PauseAsync(paymentLinkId, GetRequiredTenantId(), ct);
        return Ok(new { Status = "paused" });
    }

    /// <summary>Resume payment link.</summary>
    [HttpPost("payment-links/{paymentLinkId:guid}/resume")]
    public async Task<IActionResult> Resume(Guid paymentLinkId, CancellationToken ct)
    {
        await _linkSvc.ResumeAsync(paymentLinkId, GetRequiredTenantId(), ct);
        return Ok(new { Status = "active" });
    }

    /// <summary>Soft-delete payment link.</summary>
    [HttpDelete("payment-links/{paymentLinkId:guid}")]
    public async Task<IActionResult> Delete(Guid paymentLinkId, CancellationToken ct)
    {
        await _linkSvc.SoftDeleteAsync(paymentLinkId, GetRequiredTenantId(), ct);
        return Ok(new { Status = "deleted" });
    }

    /// <summary>Get payments for this link.</summary>
    [HttpGet("payment-links/{paymentLinkId:guid}/payments")]
    public async Task<ActionResult<List<PaymentIntentDto>>> GetPayments(Guid paymentLinkId, CancellationToken ct)
    {
        return Ok(await _linkSvc.GetPaymentsAsync(paymentLinkId, ct));
    }
}
