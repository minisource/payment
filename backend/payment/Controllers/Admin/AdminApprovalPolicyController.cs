using System.Text.Json;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace payment.Controllers.Admin;

[Route("api/v1/admin")]
[Authorize(Policy = "WalletAdmin")]
public class AdminApprovalPolicyController : PaymentControllerBase
{
    private readonly PaymentDbContext _db;

    public AdminApprovalPolicyController(PaymentDbContext db) => _db = db;

    private Guid? TenantFilter => GetEffectiveTenantId();

    [HttpGet("approval-policies")]
    public async Task<ActionResult> GetApprovalPolicies(
        [FromQuery] Guid? tenantId,
        [FromQuery] string? operationType,
        [FromQuery] string? status,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 25,
        CancellationToken ct = default)
    {
        var tid = tenantId ?? TenantFilter;
        var q = _db.AdminApprovalPolicies.AsQueryable()
            .Where(p => p.DeletedAt == null);

        if (tid.HasValue)
            q = q.Where(p => p.TenantId == null || p.TenantId == tid.Value);

        if (!string.IsNullOrEmpty(operationType))
            q = q.Where(p => p.OperationType == operationType);

        if (!string.IsNullOrEmpty(status))
            q = q.Where(p => p.Status == status);

        var total = await q.CountAsync(ct);
        var items = await q.OrderByDescending(p => p.CreatedAt).Skip(skip).Take(take)
            .Select(p => new
            {
                p.Id, p.TenantId,
                name = p.OperationType,
                description = (string?)null,
                applicationCode = (string?)null,
                operationType = p.OperationType,
                p.Enabled, p.Status,
                amountThreshold = p.AmountThreshold,
                p.Currency,
                requiredApprovals = p.RequiredApprovals,
                requireDifferentUser = p.RequireDifferentUser,
                expiresAfterMinutes = (int?)null,
                p.CreatedAt, p.UpdatedAt
            })
            .ToListAsync(ct);

        return Ok(new { items, total });
    }

    [HttpGet("approval-policies/{policyId:guid}")]
    public async Task<ActionResult> GetApprovalPolicy(Guid policyId, CancellationToken ct)
    {
        var policy = await _db.AdminApprovalPolicies
            .FirstOrDefaultAsync(p => p.Id == policyId && p.DeletedAt == null, ct);

        if (policy == null)
            return NotFound(new { error = new { code = "NOT_FOUND", message = $"Approval policy {policyId} not found" } });

        return Ok(new
        {
            policy.Id, policy.TenantId,
            name = policy.OperationType,
            description = (string?)null,
            applicationCode = (string?)null,
            operationType = policy.OperationType,
            policy.Enabled, policy.Status,
            amountThreshold = policy.AmountThreshold,
            policy.Currency,
            requiredApprovals = policy.RequiredApprovals,
            requireDifferentUser = policy.RequireDifferentUser,
            expiresAfterMinutes = (int?)null,
            policy.CreatedAt, policy.UpdatedAt
        });
    }

    [HttpPost("approval-policies")]
    public async Task<ActionResult> CreateApprovalPolicy([FromBody] CreateApprovalPolicyRequest request, CancellationToken ct)
    {
        var policy = new AdminApprovalPolicy
        {
            TenantId = request.TenantId,
            OperationType = request.OperationType,
            Enabled = request.Enabled ?? true,
            AmountThreshold = request.AmountThreshold,
            Currency = request.Currency,
            RequiredApprovals = request.RequiredApprovals,
            RequireDifferentUser = request.RequireDifferentUser ?? true,
            Status = "active",
            Metadata = request.Metadata != null ? JsonSerializer.Serialize(request.Metadata) : null
        };

        await _db.AdminApprovalPolicies.AddAsync(policy, ct);
        await _db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetApprovalPolicy), new { policyId = policy.Id }, new
        {
            policy.Id, policy.TenantId,
            name = policy.OperationType,
            description = (string?)null,
            applicationCode = (string?)null,
            operationType = policy.OperationType,
            policy.Enabled, policy.Status,
            amountThreshold = policy.AmountThreshold,
            policy.Currency,
            requiredApprovals = policy.RequiredApprovals,
            requireDifferentUser = policy.RequireDifferentUser,
            expiresAfterMinutes = (int?)null,
            policy.CreatedAt, policy.UpdatedAt
        });
    }

    [HttpPatch("approval-policies/{policyId:guid}")]
    public async Task<ActionResult> UpdateApprovalPolicy(Guid policyId, [FromBody] UpdateApprovalPolicyRequest request, CancellationToken ct)
    {
        var policy = await _db.AdminApprovalPolicies
            .FirstOrDefaultAsync(p => p.Id == policyId && p.DeletedAt == null, ct);
        if (policy == null)
            return NotFound(new { error = new { code = "NOT_FOUND", message = $"Approval policy {policyId} not found" } });

        if (request.OperationType != null) policy.OperationType = request.OperationType;
        if (request.Enabled.HasValue) policy.Enabled = request.Enabled.Value;
        if (request.AmountThreshold != null) policy.AmountThreshold = request.AmountThreshold;
        if (request.Currency != null) policy.Currency = request.Currency;
        if (request.RequiredApprovals.HasValue) policy.RequiredApprovals = request.RequiredApprovals.Value;
        if (request.RequireDifferentUser.HasValue) policy.RequireDifferentUser = request.RequireDifferentUser.Value;
        if (request.Status != null) policy.Status = request.Status;

        await _db.SaveChangesAsync(ct);
        return Ok(new
        {
            policy.Id, policy.TenantId,
            name = policy.OperationType,
            description = (string?)null,
            applicationCode = (string?)null,
            operationType = policy.OperationType,
            policy.Enabled, policy.Status,
            amountThreshold = policy.AmountThreshold,
            policy.Currency,
            requiredApprovals = policy.RequiredApprovals,
            requireDifferentUser = policy.RequireDifferentUser,
            expiresAfterMinutes = (int?)null,
            policy.CreatedAt, policy.UpdatedAt
        });
    }

    [HttpPost("approval-policies/{policyId:guid}/enable")]
    public async Task<ActionResult> EnableApprovalPolicy(Guid policyId, CancellationToken ct)
    {
        var policy = await _db.AdminApprovalPolicies.FirstOrDefaultAsync(p => p.Id == policyId && p.DeletedAt == null, ct);
        if (policy == null) return NotFound();
        policy.Enabled = true;
        await _db.SaveChangesAsync(ct);
        return Ok(new { success = true, message = "Policy enabled" });
    }

    [HttpPost("approval-policies/{policyId:guid}/disable")]
    public async Task<ActionResult> DisableApprovalPolicy(Guid policyId, CancellationToken ct)
    {
        var policy = await _db.AdminApprovalPolicies.FirstOrDefaultAsync(p => p.Id == policyId && p.DeletedAt == null, ct);
        if (policy == null) return NotFound();
        policy.Enabled = false;
        await _db.SaveChangesAsync(ct);
        return Ok(new { success = true, message = "Policy disabled" });
    }

    [HttpDelete("approval-policies/{policyId:guid}")]
    public async Task<ActionResult> DeleteApprovalPolicy(Guid policyId, CancellationToken ct)
    {
        var policy = await _db.AdminApprovalPolicies.FirstOrDefaultAsync(p => p.Id == policyId && p.DeletedAt == null, ct);
        if (policy == null) return NotFound();
        policy.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return Ok(new { success = true, message = "Policy deleted" });
    }
}

// ─── Request DTOs ─────────────────────────────────────────────────

public record CreateApprovalPolicyRequest(
    string OperationType,
    int RequiredApprovals,
    Guid? TenantId = null,
    bool? Enabled = null,
    decimal? AmountThreshold = null,
    string? Currency = null,
    bool? RequireDifferentUser = null,
    object? Metadata = null);

public record UpdateApprovalPolicyRequest(
    string? OperationType = null,
    bool? Enabled = null,
    decimal? AmountThreshold = null,
    string? Currency = null,
    int? RequiredApprovals = null,
    bool? RequireDifferentUser = null,
    string? Status = null);
