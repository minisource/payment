using System.Globalization;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Presentaion.Middlewares;

namespace payment.Controllers.Admin;

// ═══════════════════════════════════════════════════════════════
// Response DTOs — match frontend types in api/dashboard.ts exactly
// ═══════════════════════════════════════════════════════════════

public record LiabilitySummary(
    string Currency,
    decimal AvailableTotal,
    decimal LockedTotal,
    decimal PendingTotal);

public record PaymentMetricSummary(
    int SuccessfulToday,
    int FailedToday,
    decimal SuccessRate,
    decimal TotalVolume,
    string Currency);

public record WithdrawalMetricSummary(
    int PendingCount,
    decimal PendingAmount,
    int ApprovedCount,
    decimal PaidAmount,
    string Currency);

public record GatewayMetricSummary(
    decimal SuccessRate,
    int FailedTransactions,
    string? TopProvider);

public record SecurityMetricSummary(
    int OpenRiskCases,
    int CriticalRiskCases,
    int UnresolvedRecItems,
    int WalletConsistencyIssues,
    int PendingApprovals);

public record EventDeliveryMetricSummary(
    int PendingOutbox,
    int FailedOutbox,
    int DeadLetteredOutbox,
    int FailedWebhookDeliveries);

public record DashboardOverviewResponse(
    List<LiabilitySummary> Liability,
    List<PaymentMetricSummary> Payments,
    List<WithdrawalMetricSummary> Withdrawals,
    GatewayMetricSummary Gateways,
    SecurityMetricSummary Security,
    EventDeliveryMetricSummary Events);

public record ChartDataPoint(
    string Label,
    decimal Value,
    string? Date);

public record LatestPaymentIntentItem(
    string Id,
    Guid TenantId,
    decimal Amount,
    string Currency,
    string Status,
    string? ProviderCode,
    DateTime CreatedAt);

public record LatestWithdrawalItem(
    string Id,
    Guid TenantId,
    decimal Amount,
    string Currency,
    string Status,
    decimal NetAmount,
    DateTime CreatedAt);

public record LatestRiskCaseItem(
    string Id,
    Guid TenantId,
    string Severity,
    string Status,
    string Description,
    DateTime OpenedAt);

public record LatestReconciliationItem(
    string Id,
    Guid TenantId,
    string ReconciliationType,
    string Status,
    decimal Amount,
    string Currency,
    DateTime CreatedAt);

public record LatestEventDeliveryItem(
    string Id,
    string EventType,
    string Status,
    string? LastErrorMessage,
    DateTime CreatedAt);

// ═══════════════════════════════════════════════════════════════
// Controller
// ═══════════════════════════════════════════════════════════════

[Route("api/v1/admin/dashboard")]
[Authorize(Policy = "WalletAdmin")]
public class DashboardController : PaymentControllerBase
{
    private readonly PaymentDbContext _db;

    public DashboardController(PaymentDbContext db) => _db = db;

    private Guid? TenantFilter => GetEffectiveTenantId();

    private static (DateTime from, DateTime to) DateRange(string? dateFrom, string? dateTo)
    {
        var to = DateTime.TryParse(dateTo, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dt)
            ? dt.Date.AddDays(1) : DateTime.UtcNow.Date.AddDays(1);
        var from = DateTime.TryParse(dateFrom, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var df)
            ? df.Date : to.AddDays(-30);
        return (from, to);
    }

    // ─── Overview ──────────────────────────────────────────────

    [HttpGet("overview")]
    public async Task<ActionResult<DashboardOverviewResponse>> GetOverview(
        [FromQuery] string? dateFrom, [FromQuery] string? dateTo, CancellationToken ct)
    {
        var (from, to) = DateRange(dateFrom, dateTo);
        var tid = TenantFilter;
        var todayStart = DateTime.UtcNow.Date;

        // ── Liability ──────────────────────────────────────
        var accountsQuery = _db.WalletAccounts.AsQueryable();
        if (tid != null) accountsQuery = accountsQuery.Where(w => w.TenantId == tid.Value);

        var liability = await accountsQuery
            .GroupBy(w => w.Currency)
            .Select(g => new LiabilitySummary(
                g.Key,
                g.Sum(w => w.AvailableBalance),
                g.Sum(w => w.LockedBalance),
                g.Sum(w => w.PendingBalance)))
            .ToListAsync(ct);

        // ── Payments (date range) ──────────────────────────
        var paymentsQuery = _db.PaymentIntents.AsQueryable();
        if (tid != null) paymentsQuery = paymentsQuery.Where(p => p.TenantId == tid.Value);

        var todayPayments = await paymentsQuery
            .Where(p => p.CreatedAt >= todayStart && p.CreatedAt < to)
            .GroupBy(p => p.Currency)
            .Select(g => new
            {
                Currency = g.Key,
                Total = g.Count(),
                Successful = g.Count(p => p.Status == PaymentIntentStatus.Succeeded),
                Failed = g.Count(p => p.Status == PaymentIntentStatus.Failed),
                Volume = g.Sum(p => p.Amount)
            })
            .ToListAsync(ct);

        var paymentSummaries = todayPayments.Select(tp => new PaymentMetricSummary(
            tp.Successful, tp.Failed,
            tp.Total > 0 ? (decimal)tp.Successful / tp.Total : 0,
            tp.Volume, tp.Currency)).ToList();

        if (paymentSummaries.Count == 0)
            paymentSummaries.Add(new PaymentMetricSummary(0, 0, 0, 0, "IRT"));

        // ── Withdrawals ────────────────────────────────────
        var withdrawalsQuery = _db.WithdrawalRequests.AsQueryable();
        if (tid != null) withdrawalsQuery = withdrawalsQuery.Where(w => w.TenantId == tid.Value);

        var withdrawalData = await withdrawalsQuery
            .GroupBy(w => w.Currency)
            .Select(g => new WithdrawalMetricSummary(
                g.Count(w => w.Status == WithdrawalStatus.PendingReview),
                g.Where(w => w.Status == WithdrawalStatus.PendingReview).Sum(w => w.Amount),
                g.Count(w => w.Status == WithdrawalStatus.Approved
                    || w.Status == WithdrawalStatus.ProcessingPayout
                    || w.Status == WithdrawalStatus.Paid),
                g.Where(w => w.Status == WithdrawalStatus.Paid).Sum(w => w.NetAmount),
                g.Key))
            .ToListAsync(ct);

        var withdrawalSummaries = withdrawalData.ToList();
        if (withdrawalSummaries.Count == 0)
            withdrawalSummaries.Add(new WithdrawalMetricSummary(0, 0, 0, 0, "IRT"));

        // ── Gateways ───────────────────────────────────────
        var transactionsQuery = _db.PaymentTransactionsDb.AsQueryable();
        if (tid != null) transactionsQuery = transactionsQuery.Where(t => t.TenantId == tid.Value);

        var gatewayStats = await transactionsQuery
            .Where(t => t.CreatedAt >= from && t.CreatedAt < to)
            .GroupBy(t => 1)
            .Select(g => new
            {
                Total = g.Count(),
                Successful = g.Count(t => t.Status == PaymentTransactionStatus.Verified),
                Failed = g.Count(t => t.Status == PaymentTransactionStatus.Failed),
                TopProvider = g.GroupBy(t => t.ProviderCode)
                    .OrderByDescending(pg => pg.Count())
                    .Select(pg => pg.Key)
                    .FirstOrDefault()
            })
            .FirstOrDefaultAsync(ct);

        var gatewaySummary = new GatewayMetricSummary(
            gatewayStats?.Total > 0 ? (decimal)gatewayStats.Successful / gatewayStats.Total : 0,
            gatewayStats?.Failed ?? 0,
            gatewayStats?.TopProvider);

        // ── Security ───────────────────────────────────────
        var riskQuery = _db.RiskCases.AsQueryable();
        if (tid != null) riskQuery = riskQuery.Where(r => r.TenantId == tid.Value);
        var openRiskCases = await riskQuery.CountAsync(r => r.Status == "open", ct);
        var criticalRiskCases = await riskQuery.CountAsync(r => r.Status == "open" && r.Severity == "critical", ct);

        var recQuery = _db.ReconciliationItems.AsQueryable();
        if (tid != null) recQuery = recQuery.Where(r => r.TenantId == tid.Value);
        var unresolvedRec = await recQuery.CountAsync(r => r.Status != "resolved", ct);

        var issuesQuery = _db.WalletConsistencyIssues.AsQueryable();
        if (tid != null) issuesQuery = issuesQuery.Where(i => i.TenantId == tid.Value);
        var walletIssues = await issuesQuery.CountAsync(i => i.Status != "resolved", ct);

        var approvalsQuery = _db.AdminApprovalRequests.AsQueryable();
        if (tid != null) approvalsQuery = approvalsQuery.Where(a => a.TenantId == tid.Value);
        var pendingApprovals = await approvalsQuery.CountAsync(a => a.Status == "pending", ct);

        var securitySummary = new SecurityMetricSummary(
            openRiskCases, criticalRiskCases, unresolvedRec, walletIssues, pendingApprovals);

        // ── Events ─────────────────────────────────────────
        var outboxQuery = _db.OutboxEvents.AsQueryable();
        if (tid != null) outboxQuery = outboxQuery.Where(o => o.TenantId == tid.Value);
        var pendingOutbox = await outboxQuery.CountAsync(o => o.Status == "pending", ct);
        var failedOutbox = await outboxQuery.CountAsync(o => o.Status == "failed", ct);
        var deadLettered = await outboxQuery.CountAsync(o => o.Status == "dead_lettered", ct);

        var failedDeliveries = await _db.WebhookDeliveries
            .Where(d => tid == null || d.TenantId == tid.Value)
            .CountAsync(d => d.Status == "failed", ct);

        var eventsSummary = new EventDeliveryMetricSummary(
            pendingOutbox, failedOutbox, deadLettered, failedDeliveries);

        return Ok(new DashboardOverviewResponse(
            liability, paymentSummaries, withdrawalSummaries,
            gatewaySummary, securitySummary, eventsSummary));
    }

    // ─── Charts ────────────────────────────────────────────────

    [HttpGet("charts/payment-volume")]
    public async Task<ActionResult<List<ChartDataPoint>>> GetPaymentVolumeChart(
        [FromQuery] string? dateFrom, [FromQuery] string? dateTo, CancellationToken ct)
    {
        var (from, to) = DateRange(dateFrom, dateTo);
        var tid = TenantFilter;
        var query = _db.PaymentIntents.AsQueryable();
        if (tid != null) query = query.Where(p => p.TenantId == tid.Value);

        var data = await query
            .Where(p => p.CreatedAt >= from && p.CreatedAt < to)
            .GroupBy(p => p.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Volume = g.Sum(p => p.Amount) })
            .OrderBy(g => g.Date)
            .ToListAsync(ct);

        return data.Select(d => new ChartDataPoint(
            d.Date.ToString("yyyy-MM-dd"), d.Volume, d.Date.ToString("yyyy-MM-dd"))).ToList();
    }

    [HttpGet("charts/withdrawal-volume")]
    public async Task<ActionResult<List<ChartDataPoint>>> GetWithdrawalVolumeChart(
        [FromQuery] string? dateFrom, [FromQuery] string? dateTo, CancellationToken ct)
    {
        var (from, to) = DateRange(dateFrom, dateTo);
        var tid = TenantFilter;
        var query = _db.WithdrawalRequests.AsQueryable();
        if (tid != null) query = query.Where(w => w.TenantId == tid.Value);

        var data = await query
            .Where(w => w.CreatedAt >= from && w.CreatedAt < to)
            .GroupBy(w => w.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Volume = g.Sum(w => w.NetAmount) })
            .OrderBy(g => g.Date)
            .ToListAsync(ct);

        return data.Select(d => new ChartDataPoint(
            d.Date.ToString("yyyy-MM-dd"), d.Volume, d.Date.ToString("yyyy-MM-dd"))).ToList();
    }

    [HttpGet("charts/gateway-status")]
    public async Task<ActionResult<List<ChartDataPoint>>> GetGatewayChart(
        [FromQuery] string? dateFrom, [FromQuery] string? dateTo, CancellationToken ct)
    {
        var (from, to) = DateRange(dateFrom, dateTo);
        var tid = TenantFilter;
        var query = _db.PaymentTransactionsDb.AsQueryable();
        if (tid != null) query = query.Where(t => t.TenantId == tid.Value);

        var data = await query
            .Where(t => t.CreatedAt >= from && t.CreatedAt < to)
            .GroupBy(t => t.Status)
            .Select(g => new ChartDataPoint(g.Key.ToString(), g.Count(), null))
            .ToListAsync(ct);

        return data;
    }

    [HttpGet("charts/risk-severity")]
    public async Task<ActionResult<List<ChartDataPoint>>> GetRiskChart(
        [FromQuery] string? dateFrom, [FromQuery] string? dateTo, CancellationToken ct)
    {
        var (from, to) = DateRange(dateFrom, dateTo);
        var tid = TenantFilter;
        var query = _db.RiskCases.AsQueryable();
        if (tid != null) query = query.Where(r => r.TenantId == tid.Value);

        var data = await query
            .Where(r => r.OpenedAt >= from && r.OpenedAt < to)
            .GroupBy(r => r.Severity)
            .Select(g => new ChartDataPoint(g.Key, g.Count(), null))
            .ToListAsync(ct);

        return data;
    }

    [HttpGet("charts/event-delivery")]
    public async Task<ActionResult<List<ChartDataPoint>>> GetEventDeliveryChart(
        [FromQuery] string? dateFrom, [FromQuery] string? dateTo, CancellationToken ct)
    {
        var (from, to) = DateRange(dateFrom, dateTo);
        var tid = TenantFilter;
        var query = _db.OutboxEvents.AsQueryable();
        if (tid != null) query = query.Where(o => o.TenantId == tid.Value);

        var data = await query
            .Where(o => o.OccurredAt >= from && o.OccurredAt < to)
            .GroupBy(o => o.Status)
            .Select(g => new ChartDataPoint(g.Key, g.Count(), null))
            .ToListAsync(ct);

        return data;
    }

    // ─── Latest Items ──────────────────────────────────────────

    [HttpGet("latest-payment-intents")]
    public async Task<ActionResult> GetLatestPaymentIntents(
        [FromQuery] string? dateFrom, [FromQuery] string? dateTo,
        [FromQuery] int limit = 5, CancellationToken ct = default)
    {
        var (from, to) = DateRange(dateFrom, dateTo);
        var tid = TenantFilter;
        var query = _db.PaymentIntents.AsQueryable();
        if (tid != null) query = query.Where(p => p.TenantId == tid.Value);

        var items = await query
            .Where(p => p.CreatedAt >= from && p.CreatedAt < to)
            .OrderByDescending(p => p.CreatedAt)
            .Take(limit)
            .Select(p => new LatestPaymentIntentItem(
                p.Id.ToString(), p.TenantId, p.Amount, p.Currency,
                p.Status.ToString(), null, p.CreatedAt))
            .ToListAsync(ct);

        return Ok(new { items });
    }

    [HttpGet("latest-withdrawals")]
    public async Task<ActionResult> GetLatestWithdrawals(
        [FromQuery] string? dateFrom, [FromQuery] string? dateTo,
        [FromQuery] int limit = 5, CancellationToken ct = default)
    {
        var (from, to) = DateRange(dateFrom, dateTo);
        var tid = TenantFilter;
        var query = _db.WithdrawalRequests.AsQueryable();
        if (tid != null) query = query.Where(w => w.TenantId == tid.Value);

        var items = await query
            .Where(w => w.CreatedAt >= from && w.CreatedAt < to)
            .OrderByDescending(w => w.CreatedAt)
            .Take(limit)
            .Select(w => new LatestWithdrawalItem(
                w.Id.ToString(), w.TenantId, w.Amount, w.Currency,
                w.Status.ToString(), w.NetAmount, w.CreatedAt))
            .ToListAsync(ct);

        return Ok(new { items });
    }

    [HttpGet("latest-risk-cases")]
    public async Task<ActionResult> GetLatestRiskCases(
        [FromQuery] string? dateFrom, [FromQuery] string? dateTo,
        [FromQuery] int limit = 5, CancellationToken ct = default)
    {
        var (from, to) = DateRange(dateFrom, dateTo);
        var tid = TenantFilter;
        var query = _db.RiskCases.AsQueryable();
        if (tid != null) query = query.Where(r => r.TenantId == tid.Value);

        var items = await query
            .Where(r => r.OpenedAt >= from && r.OpenedAt < to)
            .OrderByDescending(r => r.OpenedAt)
            .Take(limit)
            .Select(r => new LatestRiskCaseItem(
                r.Id.ToString(), r.TenantId, r.Severity, r.Status,
                r.Description ?? r.Title, r.OpenedAt))
            .ToListAsync(ct);

        return Ok(new { items });
    }

    [HttpGet("latest-reconciliation-items")]
    public async Task<ActionResult> GetLatestReconciliationItems(
        [FromQuery] string? dateFrom, [FromQuery] string? dateTo,
        [FromQuery] int limit = 5, CancellationToken ct = default)
    {
        var (from, to) = DateRange(dateFrom, dateTo);
        var tid = TenantFilter;
        var query = _db.ReconciliationItems.AsQueryable();
        if (tid != null) query = query.Where(r => r.TenantId == tid.Value);

        var items = await query
            .Where(r => r.CreatedAt >= from && r.CreatedAt < to)
            .OrderByDescending(r => r.CreatedAt)
            .Take(limit)
            .Select(r => new LatestReconciliationItem(
                r.Id.ToString(), r.TenantId, r.ItemType, r.Status,
                r.ExpectedAmount ?? 0, r.Currency ?? "IRT", r.CreatedAt))
            .ToListAsync(ct);

        return Ok(new { items });
    }

    [HttpGet("latest-event-deliveries")]
    public async Task<ActionResult> GetLatestEventDeliveries(
        [FromQuery] string? dateFrom, [FromQuery] string? dateTo,
        [FromQuery] int limit = 5, CancellationToken ct = default)
    {
        var (from, to) = DateRange(dateFrom, dateTo);
        var tid = TenantFilter;
        var query = _db.OutboxEvents.AsQueryable();
        if (tid != null) query = query.Where(o => o.TenantId == tid.Value);

        var items = await query
            .Where(o => o.Status == "failed" || o.Status == "dead_lettered")
            .Where(o => o.OccurredAt >= from && o.OccurredAt < to)
            .OrderByDescending(o => o.OccurredAt)
            .Take(limit)
            .Select(o => new LatestEventDeliveryItem(
                o.Id.ToString(), o.EventType, o.Status,
                o.ErrorMessage, o.OccurredAt))
            .ToListAsync(ct);

        return Ok(new { items });
    }
}
