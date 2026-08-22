using Application.DTOs;
using Application.Features.PaymentLinks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Minisource.Common.Exceptions;
using Presentaion.Middlewares;

namespace payment.Controllers;

[Route("api/v1/admin")]
[Authorize(Policy = "WalletAdmin")]
public class AdminPaymentLinkController : PaymentControllerBase
{
    private readonly IPaymentLinkService _linkSvc;
    private readonly ILogger<AdminPaymentLinkController> _logger;

    public AdminPaymentLinkController(IPaymentLinkService linkSvc, ILogger<AdminPaymentLinkController> logger)
    {
        _linkSvc = linkSvc;
        _logger = logger;
    }

    private string? PublicBaseUrl => $"{Request.Scheme}://{Request.Host}";

    /// <summary>Admin list all tenant payment links.</summary>
    [HttpGet("payment-links")]
    public async Task<ActionResult<List<PaymentLinkDto>>> List(
        [FromQuery] Guid? tenantId, [FromQuery] string? applicationCode,
        [FromQuery] string? status, [FromQuery] string? currency, [FromQuery] string? amountType,
        [FromQuery] int skip = 0, [FromQuery] int take = 50, CancellationToken ct = default)
    {
        // TODO: Use GetByTenantAsync with cursor-based pagination and proper filtering
        var effectiveTenantId = tenantId ?? (TenantId != Guid.Empty ? TenantId : null);
        if (effectiveTenantId.HasValue)
        {
            var result = await _linkSvc.GetOwnerLinksAsync(
                effectiveTenantId.Value, "user", Guid.Empty, skip, take, PublicBaseUrl, ct);
            return Ok(result);
        }
        return Ok(new List<PaymentLinkDto>());
    }

    /// <summary>Get payment link by ID (admin).</summary>
    [HttpGet("payment-links/{paymentLinkId:guid}")]
    public async Task<ActionResult<PaymentLinkDto>> Get(Guid paymentLinkId, CancellationToken ct)
    {
        var link = await _linkSvc.GetAsync(paymentLinkId, null, PublicBaseUrl, ct);
        return Ok(link);
    }

    /// <summary>Admin update payment link.</summary>
    [HttpPatch("payment-links/{paymentLinkId:guid}")]
    public async Task<ActionResult<PaymentLinkDto>> Update(
        Guid paymentLinkId, [FromBody] UpdatePaymentLinkRequest request, CancellationToken ct)
    {
        return Ok(await _linkSvc.UpdateAsync(paymentLinkId, null, ActorUserId, request, ct));
    }

    /// <summary>Admin pause payment link.</summary>
    [HttpPost("payment-links/{paymentLinkId:guid}/pause")]
    public async Task<IActionResult> Pause(Guid paymentLinkId, CancellationToken ct)
    {
        await _linkSvc.PauseAsync(paymentLinkId, null, ct);
        return Ok(new { Status = "paused" });
    }

    /// <summary>Admin resume payment link.</summary>
    [HttpPost("payment-links/{paymentLinkId:guid}/resume")]
    public async Task<IActionResult> Resume(Guid paymentLinkId, CancellationToken ct)
    {
        await _linkSvc.ResumeAsync(paymentLinkId, null, ct);
        return Ok(new { Status = "active" });
    }

    /// <summary>Admin disable payment link (reversible, distinct from delete).</summary>
    [HttpPost("payment-links/{paymentLinkId:guid}/disable")]
    public async Task<IActionResult> Disable(Guid paymentLinkId, CancellationToken ct)
    {
        await _linkSvc.DisableAsync(paymentLinkId, null, ct);
        return Ok(new { Status = "disabled" });
    }

    /// <summary>Admin delete payment link (same as soft delete).</summary>
    [HttpDelete("payment-links/{paymentLinkId:guid}")]
    public async Task<IActionResult> Delete(Guid paymentLinkId, CancellationToken ct)
    {
        await _linkSvc.SoftDeleteAsync(paymentLinkId, null, ct);
        return Ok(new { Status = "deleted" });
    }

    /// <summary>Admin get payments for a link.</summary>
    [HttpGet("payment-links/{paymentLinkId:guid}/payments")]
    public async Task<ActionResult<List<PaymentIntentDto>>> GetPayments(Guid paymentLinkId, CancellationToken ct)
    {
        return Ok(await _linkSvc.GetPaymentsAsync(paymentLinkId, ct));
    }
}
