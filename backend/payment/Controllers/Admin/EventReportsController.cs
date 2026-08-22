using System.Text.Json;
using Domain.Repositories;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Presentaion.Middlewares;

namespace payment.Controllers;

[Route("api/v1/admin/reports/events")]
[Authorize(Policy = "WalletAdmin")]
public class EventReportsController : PaymentControllerBase
{
    private readonly IOutboxEventRepository _outboxRepo;
    private readonly IWebhookSubscriptionRepository _subRepo;
    private readonly IWebhookDeliveryRepository _deliveryRepo;
    private readonly IAuditLogRepository _auditRepo;
    private readonly PaymentDbContext _ctx; // For complex report queries (GroupBy, action string matching)

    public EventReportsController(
        IOutboxEventRepository outboxRepo,
        IWebhookSubscriptionRepository subRepo,
        IWebhookDeliveryRepository deliveryRepo,
        IAuditLogRepository auditRepo,
        PaymentDbContext ctx)
    {
        _outboxRepo = outboxRepo;
        _subRepo = subRepo;
        _deliveryRepo = deliveryRepo;
        _auditRepo = auditRepo;
        _ctx = ctx;
    }

    [HttpGet("overview")]
    public async Task<ActionResult<object>> GetOverview(CancellationToken ct)
    {
        var effectiveTenant = TenantId != Guid.Empty ? TenantId : (Guid?)null;

        var outbox = new
        {
            pending = await _outboxRepo.CountAsync(effectiveTenant, null, null, "pending", null, null, null, null, null, ct),
            processing = await _outboxRepo.CountAsync(effectiveTenant, null, null, "processing", null, null, null, null, null, ct),
            processed = await _outboxRepo.CountAsync(effectiveTenant, null, null, "processed", null, null, null, null, null, ct),
            failed = await _outboxRepo.CountAsync(effectiveTenant, null, null, "failed", null, null, null, null, null, ct),
            dead_lettered = await _outboxRepo.CountAsync(effectiveTenant, null, null, "dead_lettered", null, null, null, null, null, ct),
            skipped = await _outboxRepo.CountAsync(effectiveTenant, null, null, "skipped", null, null, null, null, null, ct)
        };

        var webhooks = new
        {
            delivered = await _deliveryRepo.CountAsync(effectiveTenant, null, "delivered", ct),
            failed = await _deliveryRepo.CountAsync(effectiveTenant, null, "failed", ct),
            dead_lettered = await _deliveryRepo.CountAsync(effectiveTenant, null, "dead_lettered", ct),
            pending = await _deliveryRepo.CountAsync(effectiveTenant, null, "pending", ct)
        };

        var activeSubscriptions = await _subRepo.CountActiveAsync(effectiveTenant, ct);
        var disabledSubscriptions = await _subRepo.CountAsync(effectiveTenant, null, "disabled", ct);

        // Notifier/message-bus stats from audit logs (action string matching)
        var notifierPublished = await _ctx.AuditLogs
            .Where(a => a.Action.Contains("published_to_notifier"))
            .Where(a => !effectiveTenant.HasValue || a.TenantId == effectiveTenant.Value)
            .CountAsync(ct);
        var notifierFailed = await _ctx.AuditLogs
            .Where(a => a.Action.Contains("publish_failed_to_notifier"))
            .Where(a => !effectiveTenant.HasValue || a.TenantId == effectiveTenant.Value)
            .CountAsync(ct);
        var busPublished = await _ctx.AuditLogs
            .Where(a => a.Action.Contains("published_to_message_bus"))
            .Where(a => !effectiveTenant.HasValue || a.TenantId == effectiveTenant.Value)
            .CountAsync(ct);
        var busFailed = await _ctx.AuditLogs
            .Where(a => a.Action.Contains("publish_failed_to_message_bus"))
            .Where(a => !effectiveTenant.HasValue || a.TenantId == effectiveTenant.Value)
            .CountAsync(ct);

        return Ok(new
        {
            tenant_id = effectiveTenant,
            outbox,
            webhooks = new { webhooks.delivered, webhooks.failed, webhooks.dead_lettered, webhooks.pending },
            subscriptions = new { active = activeSubscriptions, disabled = disabledSubscriptions },
            notifier = new { enabled = true, published = notifierPublished, failed = notifierFailed },
            message_bus = new { enabled = true, published = busPublished, failed = busFailed }
        });
    }

    [HttpGet("outbox")]
    public async Task<ActionResult<object>> GetOutboxReport(
        [FromQuery] DateTime? dateFrom, [FromQuery] DateTime? dateTo, CancellationToken ct)
    {
        var effectiveTenant = TenantId != Guid.Empty ? TenantId : (Guid?)null;
        var q = _ctx.OutboxEvents.AsQueryable();
        if (effectiveTenant.HasValue) q = q.Where(e => e.TenantId == effectiveTenant.Value);
        if (dateFrom.HasValue) q = q.Where(e => e.OccurredAt >= dateFrom.Value);
        if (dateTo.HasValue) q = q.Where(e => e.OccurredAt <= dateTo.Value);

        var byStatus = await q.GroupBy(e => e.Status)
            .Select(g => new { status = g.Key, count = g.Count() })
            .ToListAsync(ct);

        var byType = await q.GroupBy(e => e.EventType)
            .Select(g => new { event_type = g.Key, count = g.Count() })
            .OrderByDescending(g => g.count).Take(20)
            .ToListAsync(ct);

        return Ok(new { by_status = byStatus, by_type = byType, date_from = dateFrom, date_to = dateTo });
    }

    [HttpGet("webhooks")]
    public async Task<ActionResult<object>> GetWebhookReport(
        [FromQuery] DateTime? dateFrom, [FromQuery] DateTime? dateTo, CancellationToken ct)
    {
        var effectiveTenant = TenantId != Guid.Empty ? TenantId : (Guid?)null;
        var q = _ctx.Set<Domain.Entities.WebhookDelivery>().AsQueryable();
        if (effectiveTenant.HasValue) q = q.Where(d => d.TenantId == effectiveTenant.Value);
        if (dateFrom.HasValue) q = q.Where(d => d.CreatedAt >= dateFrom.Value);
        if (dateTo.HasValue) q = q.Where(d => d.CreatedAt <= dateTo.Value);

        var byStatus = await q.GroupBy(d => d.Status)
            .Select(g => new { status = g.Key, count = g.Count() })
            .ToListAsync(ct);

        return Ok(new { by_status = byStatus, date_from = dateFrom, date_to = dateTo });
    }

    [HttpGet("notifier")]
    public async Task<ActionResult> GetNotifierReport(CancellationToken ct)
    {
        var effectiveTenant = TenantId != Guid.Empty ? TenantId : (Guid?)null;
        var deliveries = await _ctx.AuditLogs
            .Where(a => a.Action.Contains("notifier"))
            .Where(a => !effectiveTenant.HasValue || a.TenantId == effectiveTenant.Value)
            .OrderByDescending(a => a.CreatedAt)
            .Take(20)
            .Select(a => new { a.Action, a.EntityType, a.EntityId, a.CreatedAt })
            .ToListAsync(ct);

        return Ok(new
        {
            enabled = true,
            published = await _ctx.AuditLogs
                .Where(a => a.Action.Contains("published_to_notifier"))
                .Where(a => !effectiveTenant.HasValue || a.TenantId == effectiveTenant.Value)
                .CountAsync(ct),
            failed = await _ctx.AuditLogs
                .Where(a => a.Action.Contains("publish_failed_to_notifier"))
                .Where(a => !effectiveTenant.HasValue || a.TenantId == effectiveTenant.Value)
                .CountAsync(ct),
            recent_actions = deliveries
        });
    }

    [HttpGet("message-bus")]
    public async Task<ActionResult> GetMessageBusReport(CancellationToken ct)
    {
        var effectiveTenant = TenantId != Guid.Empty ? TenantId : (Guid?)null;
        var deliveries = await _ctx.AuditLogs
            .Where(a => a.Action.Contains("message_bus"))
            .Where(a => !effectiveTenant.HasValue || a.TenantId == effectiveTenant.Value)
            .OrderByDescending(a => a.CreatedAt)
            .Take(20)
            .Select(a => new { a.Action, a.EntityType, a.EntityId, a.CreatedAt })
            .ToListAsync(ct);

        return Ok(new
        {
            enabled = true,
            published = await _ctx.AuditLogs
                .Where(a => a.Action.Contains("published_to_message_bus"))
                .Where(a => !effectiveTenant.HasValue || a.TenantId == effectiveTenant.Value)
                .CountAsync(ct),
            failed = await _ctx.AuditLogs
                .Where(a => a.Action.Contains("publish_failed_to_message_bus"))
                .Where(a => !effectiveTenant.HasValue || a.TenantId == effectiveTenant.Value)
                .CountAsync(ct),
            recent_actions = deliveries
        });
    }
}
