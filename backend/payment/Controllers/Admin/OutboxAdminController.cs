using System.Text.Json;
using Application.Features.Webhooks;
using Domain.Entities;
using Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Minisource.Common.Domain;
using Presentaion.Middlewares;

namespace payment.Controllers;

[Route("api/v1/admin/outbox")]
[Authorize(Policy = "WalletAdmin")]
public class OutboxAdminController : PaymentControllerBase
{
    private readonly IOutboxEventRepository _repo;
    private readonly IAuditLogRepository _auditRepo;
    private readonly IUnitOfWork _uow;
    private readonly IWebhookSigningService _signingService;

    public OutboxAdminController(
        IOutboxEventRepository repo,
        IAuditLogRepository auditRepo,
        IUnitOfWork uow,
        IWebhookSigningService signingService)
    {
        _repo = repo;
        _auditRepo = auditRepo;
        _uow = uow;
        _signingService = signingService;
    }

    [HttpGet("events")]
    public async Task<ActionResult<object>> ListEvents(
        [FromQuery] Guid? tenantId,
        [FromQuery] string? eventType,
        [FromQuery] string? status,
        [FromQuery] string? aggregateType,
        [FromQuery] string? aggregateId,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] string? query,
        [FromQuery] string? applicationCode,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50,
        CancellationToken ct = default)
    {
        var effectiveTenant = tenantId ?? (TenantId != Guid.Empty ? TenantId : null);
        var items = await _repo.ListAsync(effectiveTenant, applicationCode, eventType,
            status, aggregateType, aggregateId, dateFrom, dateTo, query, skip, take, ct);
        var total = await _repo.CountAsync(effectiveTenant, applicationCode, eventType,
            status, aggregateType, aggregateId, dateFrom, dateTo, query, ct);

        return Ok(new { items = items.Select(SafeEvent), total, skip, take });
    }

    [HttpGet("events/{eventId:guid}")]
    public async Task<ActionResult<object>> GetEvent(Guid eventId, CancellationToken ct)
    {
        var evt = await _repo.GetByIdAsync(eventId, ct);
        if (evt == null) return NotFound();
        return Ok(SafeEvent(evt));
    }

    [HttpPost("events/{eventId:guid}/retry")]
    [RequestRateLimit(MaxRequests = 10, WindowSeconds = 60)]
    public async Task<ActionResult> RetryEvent(Guid eventId, CancellationToken ct)
    {
        var evt = await _repo.GetByIdAsync(eventId, ct);
        if (evt == null) return NotFound();

        if (evt.Status != "failed" && evt.Status != "dead_lettered")
            return BadRequest(new { error = "outbox_event_invalid_state", message = "Only failed or dead-lettered events can be retried" });

        // Use ExecuteUpdateAsync for consistency with all other status-change methods
        await _repo.MarkAsPendingForRetryAsync(eventId, ct);

        await _auditRepo.AddAsync(new AuditLog
        {
            TenantId = evt.TenantId,
            Action = "outbox.event_retried",
            EntityType = "OutboxEvent",
            EntityId = eventId.ToString(),
            ActorUserId = ActorUserId
        }, ct);

        await _uow.SaveChangesAsync(ct);
        return Ok(new { id = eventId, status = "pending" });
    }

    [HttpPost("events/{eventId:guid}/skip")]
    public async Task<ActionResult> SkipEvent(Guid eventId, [FromBody] SkipRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Reason))
            return BadRequest(new { error = "outbox_event_skip_reason_required" });

        var evt = await _repo.GetByIdAsync(eventId, ct);
        if (evt == null) return NotFound();

        await _repo.MarkAsSkippedAsync(eventId, request.Reason, ct);

        await _auditRepo.AddAsync(new AuditLog
        {
            TenantId = evt.TenantId,
            Action = "outbox.event_skipped",
            EntityType = "OutboxEvent",
            EntityId = evt.Id.ToString(),
            Reason = request.Reason,
            ActorUserId = ActorUserId
        }, ct);

        await _uow.SaveChangesAsync(ct);
        return Ok(new { id = eventId, status = "skipped" });
    }

    [HttpPost("events/{eventId:guid}/dead-letter")]
    public async Task<ActionResult> DeadLetterEvent(Guid eventId, [FromBody] SkipRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Reason))
            return BadRequest(new { error = "outbox_event_dead_letter_reason_required" });

        var evt = await _repo.GetByIdAsync(eventId, ct);
        if (evt == null) return NotFound();

        await _repo.MarkAsDeadLetteredAsync(eventId, request.Reason, ct);

        await _auditRepo.AddAsync(new AuditLog
        {
            TenantId = evt.TenantId,
            Action = "outbox.event_dead_lettered",
            EntityType = "OutboxEvent",
            EntityId = evt.Id.ToString(),
            Reason = request.Reason,
            ActorUserId = ActorUserId
        }, ct);

        await _uow.SaveChangesAsync(ct);
        return Ok(new { id = eventId, status = "dead_lettered" });
    }

    private static object SafeEvent(OutboxEvent e) => new
    {
        e.Id, e.TenantId, e.EventType, e.EventVersion,
        e.AggregateType, e.AggregateId,
        e.Status, e.OccurredAt, e.AvailableAt, e.ProcessedAt,
        e.RetryCount, e.MaxRetryCount, e.NextRetryAt,
        e.ErrorMessage, e.LastErrorAt, e.DeadLetteredAt,
        // Do NOT expose raw Payload — it may contain PII
        PayloadPreview = TruncatePayload(e.Payload)
    };

    private static string? TruncatePayload(string? payload)
    {
        if (string.IsNullOrEmpty(payload)) return null;
        try
        {
            var doc = JsonDocument.Parse(payload);
            return JsonSerializer.Serialize(new { _truncated = true, keys = doc.RootElement.EnumerateObject().Select(p => p.Name).ToList() });
        }
        catch { return payload.Length > 200 ? payload[..200] + "..." : payload; }
    }
}

public record SkipRequest(string Reason);
