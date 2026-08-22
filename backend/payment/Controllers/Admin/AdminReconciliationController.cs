using Application.Features.Reconciliation;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace payment.Controllers.Admin;

[Route("api/v1/admin")]
[Authorize(Policy = "WalletAdmin")]
public class AdminReconciliationController : PaymentControllerBase
{
    private readonly PaymentDbContext _db;
    private readonly IReconciliationService _service;

    public AdminReconciliationController(PaymentDbContext db, IReconciliationService service)
    { _db = db; _service = service; }

    private Guid? TenantFilter => GetEffectiveTenantId();

    // ═══ Batches ════════════════════════════════════════════

    [HttpGet("reconciliation/batches")]
    public async Task<ActionResult> ListBatches(
        [FromQuery] Guid? tenantId,
        [FromQuery] string? batchType,
        [FromQuery] string? status,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 25,
        CancellationToken ct = default)
    {
        var tid = tenantId ?? TenantFilter;
        var q = _db.ReconciliationBatches.AsQueryable();
        if (tid.HasValue) q = q.Where(b => b.TenantId == tid.Value);
        if (!string.IsNullOrEmpty(batchType)) q = q.Where(b => b.ReconciliationType == batchType);
        if (!string.IsNullOrEmpty(status)) q = q.Where(b => b.Status == status);

        var total = await q.CountAsync(ct);
        var raw = await q.OrderByDescending(b => b.StartedAt).Skip(skip).Take(take).ToListAsync(ct);
        var items = raw.Select(b => new
        {
            b.Id, b.TenantId,
            batchType = b.ReconciliationType,
            b.Status,
            totalItems = b.TotalChecked,
            b.MatchedCount, b.MismatchCount,
            unresolvedCount = b.MismatchCount,
            startedAt = b.StartedAt,
            completedAt = b.CompletedAt,
            createdBy = b.TriggeredByUserId?.ToString() ?? "system",
            b.CreatedAt
        }).ToList();

        return Ok(new { items, total });
    }

    [HttpGet("reconciliation/batches/{batchId:guid}")]
    public async Task<ActionResult> GetBatch(Guid batchId, CancellationToken ct)
    {
        var batch = await _db.ReconciliationBatches.FirstOrDefaultAsync(b => b.Id == batchId, ct);
        if (batch == null) return NotFound(new { error = new { code = "NOT_FOUND", message = "Batch not found" } });

        return Ok(new
        {
            batch.Id, batch.TenantId,
            batchType = batch.ReconciliationType,
            batch.Status,
            totalItems = batch.TotalChecked,
            batch.MatchedCount, batch.MismatchCount,
            unresolvedCount = batch.MismatchCount,
            startedAt = batch.StartedAt,
            completedAt = batch.CompletedAt,
            createdBy = batch.TriggeredByUserId?.ToString() ?? "system",
            runParameters = batch.Metadata,
            batch.Summary,
            batch.CreatedAt
        });
    }

    [HttpPost("reconciliation/batches/run")]
    public async Task<ActionResult> RunReconciliation([FromBody] RunReconciliationRequest request, CancellationToken ct)
    {
        var tid = request.TenantId ?? GetRequiredTenantId();
        FinancialReconciliationBatch batch;

        switch (request.BatchType)
        {
            case "wallet_balance":
                batch = await _service.ReconcileWalletBalancesAsync(tid, GetRequiredActorUserId(), ct); break;
            case "gateway_payments":
                batch = await _service.ReconcileGatewayPaymentsAsync(tid, GetRequiredActorUserId(), ct); break;
            case "withdrawal_payouts":
                batch = await _service.ReconcileWithdrawalPayoutsAsync(tid, GetRequiredActorUserId(), ct); break;
            case "payment_wallet_posting":
                batch = await _service.ReconcilePaymentWalletPostingAsync(tid, GetRequiredActorUserId(), ct); break;
            default:
                return BadRequest(new { error = new { code = "VALIDATION", message = $"Unknown batch type: {request.BatchType}" } });
        }

        return Ok(new { success = true, batchId = batch.Id, message = $"Reconciliation {request.BatchType} completed" });
    }

    [HttpPost("reconciliation/batches/{batchId:guid}/cancel")]
    public async Task<ActionResult> CancelBatch(Guid batchId, CancellationToken ct)
    {
        var batch = await _db.ReconciliationBatches.FirstOrDefaultAsync(b => b.Id == batchId, ct);
        if (batch == null) return NotFound();
        if (batch.Status != "running") return BadRequest(new { error = new { code = "INVALID_STATE", message = "Only running batches can be cancelled" } });
        batch.Status = "cancelled";
        batch.CompletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return Ok(new { success = true, message = "Batch cancelled" });
    }

    // ═══ Items ══════════════════════════════════════════════

    [HttpGet("reconciliation/items")]
    public async Task<ActionResult> ListItems(
        [FromQuery] Guid? tenantId,
        [FromQuery] Guid? batchId,
        [FromQuery] string? status,
        [FromQuery] string? severity,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 25,
        CancellationToken ct = default)
    {
        var tid = tenantId ?? TenantFilter;
        var q = _db.ReconciliationItems.AsQueryable();
        if (tid.HasValue) q = q.Where(i => i.TenantId == tid.Value);
        if (batchId.HasValue) q = q.Where(i => i.BatchId == batchId.Value);
        if (!string.IsNullOrEmpty(status)) q = q.Where(i => i.Status == status);
        if (!string.IsNullOrEmpty(severity)) q = q.Where(i => i.Severity == severity);

        var total = await q.CountAsync(ct);
        var rawItems = await q.OrderByDescending(i => i.CreatedAt).Skip(skip).Take(take).ToListAsync(ct);
        var items = rawItems.Select(i => new
        {
            i.Id, i.BatchId, i.TenantId,
            itemType = i.ItemType,
            i.Status, i.Severity,
            expectedAmount = i.ExpectedAmount ?? 0,
            actualAmount = i.ActualAmount ?? 0,
            differenceAmount = (i.ExpectedAmount ?? 0) - (i.ActualAmount ?? 0),
            currency = i.Currency ?? "IRT",
            referenceType = i.PrimaryEntityType,
            referenceId = i.PrimaryEntityId,
            walletId = i.PrimaryEntityType == "WalletAccount" && Guid.TryParse(i.PrimaryEntityId, out var wid) ? (Guid?)wid : null,
            paymentIntentId = i.PrimaryEntityType == "PaymentIntent" && Guid.TryParse(i.PrimaryEntityId, out var piid) ? (Guid?)piid : null,
            paymentTransactionId = i.RelatedEntityType == "PaymentTransaction" && i.RelatedEntityId != null && Guid.TryParse(i.RelatedEntityId, out var ptid) ? (Guid?)ptid : null,
            detectedAt = i.CreatedAt,
            resolvedAt = i.ResolvedAt
        }).ToList();

        return Ok(new { items, total });
    }

    [HttpGet("reconciliation/items/{itemId:guid}")]
    public async Task<ActionResult> GetItem(Guid itemId, CancellationToken ct)
    {
        var item = await _db.ReconciliationItems.FirstOrDefaultAsync(i => i.Id == itemId, ct);
        if (item == null) return NotFound(new { error = new { code = "NOT_FOUND", message = "Item not found" } });

        return Ok(new
        {
            item.Id, item.BatchId, item.TenantId,
            itemType = item.ItemType,
            item.Status, item.Severity,
            expectedAmount = item.ExpectedAmount ?? 0,
            actualAmount = item.ActualAmount ?? 0,
            differenceAmount = (item.ExpectedAmount ?? 0) - (item.ActualAmount ?? 0),
            currency = item.Currency ?? "IRT",
            referenceType = item.PrimaryEntityType,
            referenceId = item.PrimaryEntityId,
            message = item.Message,
            detectionReason = item.IssueCode,
            resolutionNote = item.ResolutionNote,
            resolvedBy = item.ResolvedByUserId,
            detectedAt = item.CreatedAt,
            resolvedAt = item.ResolvedAt
        });
    }

    [HttpPost("reconciliation/items/{itemId:guid}/acknowledge")]
    public async Task<ActionResult> AcknowledgeItem(Guid itemId, CancellationToken ct)
    {
        var item = await _db.ReconciliationItems.FirstOrDefaultAsync(i => i.Id == itemId, ct);
        if (item == null) return NotFound();
        item.Status = "acknowledged";
        await _db.SaveChangesAsync(ct);
        return Ok(new { success = true, message = "Item acknowledged" });
    }

    [HttpPost("reconciliation/items/{itemId:guid}/resolve")]
    public async Task<ActionResult> ResolveItem(Guid itemId, [FromBody] ResolveItemRequest req, CancellationToken ct)
    {
        var item = await _db.ReconciliationItems.FirstOrDefaultAsync(i => i.Id == itemId, ct);
        if (item == null) return NotFound();
        item.Resolve(GetRequiredActorUserId(), req.Reason);
        await _db.SaveChangesAsync(ct);
        return Ok(new { success = true, message = "Item resolved" });
    }

    [HttpPost("reconciliation/items/{itemId:guid}/false-positive")]
    public async Task<ActionResult> MarkFalsePositive(Guid itemId, [FromBody] ResolveItemRequest req, CancellationToken ct)
    {
        var item = await _db.ReconciliationItems.FirstOrDefaultAsync(i => i.Id == itemId, ct);
        if (item == null) return NotFound();
        item.MarkFalsePositive();
        await _db.SaveChangesAsync(ct);
        return Ok(new { success = true, message = "Marked as false positive" });
    }

    [HttpPost("reconciliation/items/{itemId:guid}/ignore")]
    public async Task<ActionResult> IgnoreItem(Guid itemId, [FromBody] ResolveItemRequest req, CancellationToken ct)
    {
        var item = await _db.ReconciliationItems.FirstOrDefaultAsync(i => i.Id == itemId, ct);
        if (item == null) return NotFound();
        item.Status = "ignored";
        item.ResolvedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return Ok(new { success = true, message = "Item ignored" });
    }
}

public record RunReconciliationRequest(
    string BatchType,
    Guid? TenantId = null,
    string? DateFrom = null,
    string? DateTo = null,
    Guid? WalletId = null);

public record ResolveItemRequest(string Reason);
