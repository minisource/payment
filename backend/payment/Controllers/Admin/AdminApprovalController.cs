using System.Text.Json;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Application.Features.Approvals;

namespace payment.Controllers.Admin;

[Route("api/v1/admin")]
[Authorize(Policy = "WalletAdmin")]
public class AdminApprovalController : PaymentControllerBase
{
    private readonly IAdminApprovalService _service;
    private readonly PaymentDbContext _db;

    public AdminApprovalController(IAdminApprovalService service, PaymentDbContext db)
    { _service = service; _db = db; }

    private Guid? TenantFilter => GetEffectiveTenantId();

    [HttpGet("approvals")]
    public async Task<ActionResult> ListApprovalRequests(
        [FromQuery] Guid? tenantId,
        [FromQuery] string? status,
        [FromQuery] string? operationType,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 25,
        CancellationToken ct = default)
    {
        var tid = tenantId ?? TenantFilter;
        var q = _db.AdminApprovalRequests.AsQueryable();
        if (tid.HasValue) q = q.Where(r => r.TenantId == tid.Value);
        if (!string.IsNullOrEmpty(status)) q = q.Where(r => r.Status == status);
        if (!string.IsNullOrEmpty(operationType)) q = q.Where(r => r.OperationType == operationType);

        var total = await q.CountAsync(ct);
        var items = await q.OrderByDescending(r => r.RequestedAt).Skip(skip).Take(take)
            .Select(r => new
            {
                r.Id, r.TenantId,
                operationType = r.OperationType,
                r.Status, r.RequestedByUserId,
                subjectType = r.EntityType,
                subjectId = r.EntityId,
                r.RequiredApprovals,
                approvalsCount = r.ApprovalCount,
                r.ExpiresAt, r.CreatedAt, r.UpdatedAt
            }).ToListAsync(ct);

        return Ok(new { items, total });
    }

    [HttpGet("approvals/{approvalRequestId:guid}")]
    public async Task<ActionResult> GetApprovalRequest(Guid approvalRequestId, CancellationToken ct)
    {
        var req = await _db.AdminApprovalRequests.FirstOrDefaultAsync(r => r.Id == approvalRequestId, ct);
        if (req == null) return NotFound(new { error = new { code = "NOT_FOUND", message = "Approval request not found" } });

        var decisions = await _db.AdminApprovalDecisions
            .Where(d => d.ApprovalRequestId == approvalRequestId)
            .OrderBy(d => d.DecidedAt)
            .Select(d => new
            {
                d.Id, d.Decision, d.DecidedByUserId, d.DecidedAt,
                note = d.Reason
            }).ToListAsync(ct);

        return Ok(new
        {
            req.Id, req.TenantId,
            operationType = req.OperationType,
            req.Status,
            req.RequestedByUserId,
            subjectType = req.EntityType,
            subjectId = req.EntityId,
            req.RequiredApprovals,
            approvalsCount = req.ApprovalCount,
            req.ExpiresAt, req.CreatedAt, req.UpdatedAt,
            requestPayload = req.RequestPayload,
            decisions,
            riskEvaluationId = req.RiskEvaluationId
        });
    }

    [HttpPost("approvals/{approvalRequestId:guid}/approve")]
    public async Task<ActionResult> ApproveRequest(Guid approvalRequestId, [FromBody] ApprovalActionRequest? req, CancellationToken ct)
    {
        await _service.ApproveAsync(approvalRequestId, GetRequiredActorUserId(), req?.Note, ct);
        return Ok(new { success = true, message = "Approved" });
    }

    [HttpPost("approvals/{approvalRequestId:guid}/reject")]
    public async Task<ActionResult> RejectRequest(Guid approvalRequestId, [FromBody] ApprovalActionRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Reason))
            return BadRequest(new { error = new { code = "VALIDATION", message = "Reason is required for rejection" } });
        await _service.RejectAsync(approvalRequestId, GetRequiredActorUserId(), req.Reason, ct);
        return Ok(new { success = true, message = "Rejected" });
    }

    [HttpPost("approvals/{approvalRequestId:guid}/execute")]
    public async Task<ActionResult> ExecuteRequest(Guid approvalRequestId, CancellationToken ct)
    {
        await _service.ExecuteAsync(approvalRequestId, GetRequiredActorUserId(), ct);
        return Ok(new { success = true, message = "Executed" });
    }

    [HttpPost("approvals/{approvalRequestId:guid}/cancel")]
    public async Task<ActionResult> CancelRequest(Guid approvalRequestId, [FromBody] ApprovalActionRequest req, CancellationToken ct)
    {
        await _service.CancelAsync(approvalRequestId, ct);
        return Ok(new { success = true, message = "Cancelled" });
    }
}

public record ApprovalActionRequest(string? Note = null, string? Reason = null);
