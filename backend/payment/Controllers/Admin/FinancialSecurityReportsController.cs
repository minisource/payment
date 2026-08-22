using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Presentaion.Middlewares;

namespace payment.Controllers;

[Route("api/v1/admin/reports/financial-security")]
[Authorize(Policy = "WalletAdmin")]
public class FinancialSecurityReportsController : PaymentControllerBase
{
    private readonly PaymentDbContext _ctx;

    public FinancialSecurityReportsController(PaymentDbContext ctx) => _ctx = ctx;

    [HttpGet("overview")]
    public async Task<IActionResult> Overview([FromQuery] Guid? tenantId, CancellationToken ct)
    {
        var tId = tenantId ?? (TenantId != Guid.Empty ? TenantId : (Guid?)null);
        var walletQuery = _ctx.WalletAccounts.AsQueryable();
        var issueQuery = _ctx.WalletConsistencyIssues.AsQueryable();
        var riskQuery = _ctx.RiskCases.AsQueryable();
        var recQuery = _ctx.ReconciliationItems.AsQueryable();
        var approvalQuery = _ctx.AdminApprovalRequests.AsQueryable();

        if (tId.HasValue)
        {
            walletQuery = walletQuery.Where(w => w.TenantId == tId.Value);
            issueQuery = issueQuery.Where(i => i.TenantId == tId.Value);
            riskQuery = riskQuery.Where(r => r.TenantId == tId.Value);
            recQuery = recQuery.Where(r => r.TenantId == tId.Value);
            approvalQuery = approvalQuery.Where(a => a.TenantId == tId.Value);
        }

        var totalWallets = await walletQuery.CountAsync(ct);
        var walletsWithIssues = await issueQuery.Where(i => i.Status == "open").Select(i => i.WalletId).Distinct().CountAsync(ct);
        var openRiskCases = await riskQuery.CountAsync(r => r.Status == "open", ct);
        var criticalRiskCases = await riskQuery.CountAsync(r => r.Status == "open" && r.Severity == "critical", ct);
        var unresolvedRecItems = await recQuery.CountAsync(r => r.Status != "resolved" && r.Status != "false_positive", ct);
        var pendingApprovals = await approvalQuery.CountAsync(a => a.Status == "pending", ct);

        // Total liability by currency
        var liability = await walletQuery.Where(w => w.Status == Domain.Enums.WalletAccountStatus.Active)
            .GroupBy(w => w.Currency)
            .Select(g => new { currency = g.Key, available_total = g.Sum(w => w.AvailableBalance), locked_total = g.Sum(w => w.LockedBalance) })
            .ToListAsync(ct);

        return Ok(new
        {
            tenant_id = tId,
            wallets = new { total_wallets = totalWallets, wallets_with_issues = walletsWithIssues, liability },
            risk = new { open_cases = openRiskCases, critical_cases = criticalRiskCases },
            reconciliation = new { unresolved_items = unresolvedRecItems },
            admin_security = new { pending_approval_requests = pendingApprovals }
        });
    }

    [HttpGet("risk-cases")]
    public async Task<IActionResult> RiskCases([FromQuery] Guid? tenantId, [FromQuery] int skip = 0, [FromQuery] int take = 50, CancellationToken ct = default)
    {
        var tId = tenantId ?? (TenantId != Guid.Empty ? TenantId : (Guid?)null);
        var q = _ctx.RiskCases.AsQueryable();
        if (tId.HasValue) q = q.Where(r => r.TenantId == tId.Value);
        var total = await q.CountAsync(ct);
        var items = await q.OrderByDescending(r => r.OpenedAt).Skip(skip).Take(take).ToListAsync(ct);
        return Ok(new { items, total, skip, take });
    }

    [HttpGet("reconciliation")]
    public async Task<IActionResult> Reconciliation([FromQuery] Guid? tenantId, [FromQuery] int skip = 0, [FromQuery] int take = 50, CancellationToken ct = default)
    {
        var tId = tenantId ?? (TenantId != Guid.Empty ? TenantId : (Guid?)null);
        var q = _ctx.ReconciliationItems.AsQueryable();
        if (tId.HasValue) q = q.Where(r => r.TenantId == tId.Value);
        var total = await q.CountAsync(ct);
        var items = await q.OrderByDescending(r => r.CreatedAt).Skip(skip).Take(take).ToListAsync(ct);
        return Ok(new { items, total, skip, take });
    }

    [HttpGet("ledger-integrity")]
    public async Task<IActionResult> LedgerIntegrity([FromQuery] Guid? tenantId, [FromQuery] int skip = 0, [FromQuery] int take = 50, CancellationToken ct = default)
    {
        var tId = tenantId ?? (TenantId != Guid.Empty ? TenantId : (Guid?)null);
        var q = _ctx.WalletConsistencyChecks.AsQueryable();
        if (tId.HasValue) q = q.Where(c => c.TenantId == tId.Value);
        var total = await q.CountAsync(ct);
        var items = await q.OrderByDescending(c => c.StartedAt).Skip(skip).Take(take).ToListAsync(ct);
        return Ok(new { items, total, skip, take });
    }

    [HttpGet("limit-usage")]
    public async Task<IActionResult> LimitUsage([FromQuery] Guid? tenantId, [FromQuery] int skip = 0, [FromQuery] int take = 50, CancellationToken ct = default)
    {
        var tId = tenantId ?? (TenantId != Guid.Empty ? TenantId : (Guid?)null);
        var q = _ctx.PaymentLimitUsages.AsQueryable();
        if (tId.HasValue) q = q.Where(u => u.TenantId == tId.Value);
        var total = await q.CountAsync(ct);
        var items = await q.OrderByDescending(u => u.UpdatedAt).Skip(skip).Take(take).ToListAsync(ct);
        return Ok(new { items, total, skip, take });
    }

    [HttpGet("admin-actions")]
    public async Task<IActionResult> AdminActions([FromQuery] Guid? tenantId, [FromQuery] int skip = 0, [FromQuery] int take = 50, CancellationToken ct = default)
    {
        var tId = tenantId ?? (TenantId != Guid.Empty ? TenantId : (Guid?)null);
        var q = _ctx.AdminApprovalRequests.AsQueryable();
        if (tId.HasValue) q = q.Where(a => a.TenantId == tId.Value);
        var total = await q.CountAsync(ct);
        var items = await q.OrderByDescending(a => a.RequestedAt).Skip(skip).Take(take).ToListAsync(ct);
        return Ok(new { items, total, skip, take });
    }

    [HttpGet("suspicious-activity")]
    public async Task<IActionResult> SuspiciousActivity([FromQuery] Guid? tenantId, [FromQuery] int skip = 0, [FromQuery] int take = 50, CancellationToken ct = default)
    {
        var tId = tenantId ?? (TenantId != Guid.Empty ? TenantId : (Guid?)null);
        var q = _ctx.RiskCases.AsQueryable().Where(r => r.Severity == "high" || r.Severity == "critical");
        if (tId.HasValue) q = q.Where(r => r.TenantId == tId.Value);
        var total = await q.CountAsync(ct);
        var items = await q.OrderByDescending(r => r.OpenedAt).Skip(skip).Take(take).ToListAsync(ct);
        return Ok(new { items, total, skip, take });
    }
}
