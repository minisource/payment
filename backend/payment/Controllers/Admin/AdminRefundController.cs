using Application.DTOs;
using Application.Features.Refunds;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Minisource.Common.Exceptions;
using Presentaion.Middlewares;

namespace payment.Controllers;

/// <summary>
/// Admin API for refund management.
/// Provides full lifecycle: create, approve, reject, process, retry, cancel.
/// </summary>
[Route("api/v1/admin")]
[Authorize(Policy = "WalletAdmin")]
public class AdminRefundController : PaymentControllerBase
{
    private readonly IRefundService _refundService;
    private readonly IRefundQueryService _refundQuery;
    private readonly ILogger<AdminRefundController> _logger;

    public AdminRefundController(
        IRefundService refundService,
        IRefundQueryService refundQuery,
        ILogger<AdminRefundController> logger)
    {
        _refundService = refundService;
        _refundQuery = refundQuery;
        _logger = logger;
    }

    // ─── List ─────────────────────────────────────────────────

    /// <summary>List refund requests with filters. Supports tenant_scope=all for cross-tenant view.</summary>
    [HttpGet("refunds")]
    public async Task<ActionResult<object>> ListRefunds(
        [FromQuery] string? status,
        [FromQuery] Guid? paymentTransactionId,
        [FromQuery] Guid? walletId,
        [FromQuery] string? providerCode,
        [FromQuery] string? currency,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] string? query,
        [FromQuery] string? applicationCode,
        [FromQuery] Guid? tenantId,
        [FromQuery] string? tenantScope,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50,
        CancellationToken ct = default)
    {
        // Use effective tenant: null = all tenants (cross-tenant view), Guid.Empty = error
        var effectiveTenant = tenantId ?? GetEffectiveTenantId();
        if (effectiveTenant == Guid.Empty)
            return BadRequest(new { error = "tenant_required", message = "Tenant context is required" });

        var (items, total) = await _refundQuery.ListAsync(
            effectiveTenant, status, paymentTransactionId, walletId,
            providerCode, currency, from, to, query, applicationCode,
            skip, take, ct);

        return Ok(new RefundListResponse { Items = items, Total = total, Skip = skip, Take = take });
    }

    // ─── Detail ───────────────────────────────────────────────

    /// <summary>Get a single refund request.</summary>
    [HttpGet("refunds/{refundId:guid}")]
    public async Task<ActionResult<RefundRequestDto>> GetRefund(Guid refundId, CancellationToken ct)
    {
        var refund = await _refundQuery.GetByIdAsync(refundId, ct)
            ?? throw new NotFoundException("RefundRequest", refundId.ToString());

        return Ok(refund);
    }

    // ─── Create ───────────────────────────────────────────────

    /// <summary>Create a new refund request.</summary>
    [HttpPost("refunds")]
    public async Task<ActionResult<RefundRequestDto>> CreateRefund(
        [FromBody] CreateRefundRequest request, CancellationToken ct)
    {
        var refund = await _refundService.CreateAsync(
            GetRequiredTenantId(), request,
            GetRequiredActorUserId().ToString(),
            ApplicationCode,
            ct);

        _logger.LogInformation("Admin refund created: {RefundId}", refund.Id);
        return Ok(refund);
    }

    // ─── Approve ──────────────────────────────────────────────

    /// <summary>Approve a pending refund.</summary>
    [HttpPost("refunds/{refundId:guid}/approve")]
    public async Task<ActionResult<RefundRequestDto>> ApproveRefund(
        Guid refundId, [FromBody] ApproveRefundRequest? request, CancellationToken ct)
    {
        var refund = await _refundService.ApproveAsync(
            refundId,
            GetRequiredActorUserId().ToString(),
            request ?? new ApproveRefundRequest(),
            ct);

        _logger.LogInformation("Admin refund approved: {RefundId}", refundId);
        return Ok(refund);
    }

    // ─── Reject ───────────────────────────────────────────────

    /// <summary>Reject a pending refund. Requires reason.</summary>
    [HttpPost("refunds/{refundId:guid}/reject")]
    public async Task<ActionResult<RefundRequestDto>> RejectRefund(
        Guid refundId, [FromBody] RejectRefundRequest request, CancellationToken ct)
    {
        var refund = await _refundService.RejectAsync(
            refundId,
            GetRequiredActorUserId().ToString(),
            request,
            ct);

        _logger.LogInformation("Admin refund rejected: {RefundId}", refundId);
        return Ok(refund);
    }

    // ─── Cancel ───────────────────────────────────────────────

    /// <summary>Cancel a refund request.</summary>
    [HttpPost("refunds/{refundId:guid}/cancel")]
    public async Task<ActionResult<RefundRequestDto>> CancelRefund(Guid refundId, CancellationToken ct)
    {
        var refund = await _refundService.CancelAsync(refundId, ct);

        _logger.LogInformation("Admin refund cancelled: {RefundId}", refundId);
        return Ok(refund);
    }

    // ─── Process ──────────────────────────────────────────────

    /// <summary>
    /// Process a refund: create wallet hold, call gateway refund, capture hold on success.
    /// Requires strong confirmation — gateway will be called.
    /// </summary>
    [HttpPost("refunds/{refundId:guid}/process")]
    public async Task<ActionResult<RefundRequestDto>> ProcessRefund(
        Guid refundId, CancellationToken ct)
    {
        var refund = await _refundService.ProcessAsync(
            refundId,
            GetRequiredActorUserId().ToString(),
            ct);

        _logger.LogInformation("Admin refund processed: {RefundId}", refundId);
        return Ok(refund);
    }

    // ─── Retry ────────────────────────────────────────────────

    /// <summary>Retry a previously failed refund.</summary>
    [HttpPost("refunds/{refundId:guid}/retry")]
    public async Task<ActionResult<RefundRequestDto>> RetryRefund(Guid refundId, CancellationToken ct)
    {
        var refund = await _refundService.RetryAsync(
            refundId,
            GetRequiredActorUserId().ToString(),
            ct);

        _logger.LogInformation("Admin refund retried: {RefundId}", refundId);
        return Ok(refund);
    }

    // ─── Manual Review ───────────────────────────────────────

    /// <summary>Mark a refund as requiring manual review.</summary>
    [HttpPost("refunds/{refundId:guid}/mark-manual-review")]
    public async Task<ActionResult<RefundRequestDto>> MarkManualReview(
        Guid refundId, [FromBody] string reason, CancellationToken ct)
    {
        var refund = await _refundService.MarkManualReviewAsync(
            refundId, "manual_review", reason, ct);

        _logger.LogWarning("Admin refund marked manual review: {RefundId}", refundId);
        return Ok(refund);
    }
}
