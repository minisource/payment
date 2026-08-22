using System.Text.Json;
using Application.Features.Webhooks;
using Domain.Entities;
using Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Minisource.Common.Domain;
using Presentaion.Middlewares;

namespace payment.Controllers;

[Route("api/v1/admin/webhooks")]
[Authorize(Policy = "WalletAdmin")]
public class WebhooksAdminController : PaymentControllerBase
{
    private readonly IWebhookSubscriptionRepository _subRepo;
    private readonly IWebhookDeliveryRepository _deliveryRepo;
    private readonly IWebhookSigningService _signingService;
    private readonly IAuditLogRepository _auditRepo;
    private readonly IUnitOfWork _uow;

    public WebhooksAdminController(
        IWebhookSubscriptionRepository subRepo,
        IWebhookDeliveryRepository deliveryRepo,
        IWebhookSigningService signingService,
        IAuditLogRepository auditRepo,
        IUnitOfWork uow)
    {
        _subRepo = subRepo;
        _deliveryRepo = deliveryRepo;
        _signingService = signingService;
        _auditRepo = auditRepo;
        _uow = uow;
    }

    // ─── Subscriptions ──────────────────────────────────────

    [HttpGet("subscriptions")]
    public async Task<ActionResult<object>> ListSubscriptions(
        [FromQuery] Guid? tenantId,
        [FromQuery] string? applicationCode,
        [FromQuery] string? status,
        [FromQuery] int skip = 0, [FromQuery] int take = 50,
        CancellationToken ct = default)
    {
        var effectiveTenant = tenantId ?? (TenantId != Guid.Empty ? TenantId : null);
        var total = await _subRepo.CountAsync(effectiveTenant, applicationCode, status, ct);
        var items = (await _subRepo.ListAsync(effectiveTenant, applicationCode, status, skip, take, ct))
            .Select(SafeSubscription).ToList();

        return Ok(new { items, total, skip, take });
    }

    [HttpPost("subscriptions")]
    public async Task<ActionResult<object>> CreateSubscription(
        [FromBody] CreateWebhookRequest request,
        CancellationToken ct)
    {
        var effectiveTenant = request.TenantId ?? TenantId;
        if (effectiveTenant == Guid.Empty)
            return BadRequest(new { error = "webhook_subscription_invalid", message = "TenantId required" });

        // Validate webhook URL
        var urlError = ValidateWebhookUrl(request.Url);
        if (urlError != null)
            return BadRequest(new { error = "webhook_subscription_invalid_url", message = urlError });

        var (plainSecret, encryptedSecret, secretHash) = _signingService.GenerateSecret();

        var sub = new WebhookSubscription
        {
            TenantId = effectiveTenant,
            ApplicationCode = request.ApplicationCode,
            Name = request.Name,
            Url = request.Url,
            EventTypes = request.EventTypes ?? Array.Empty<string>(),
            SecretHash = secretHash,
            EncryptedSecret = encryptedSecret,
            Headers = request.Headers != null ? JsonSerializer.Serialize(request.Headers) : null,
            CreatedByUserId = ActorUserId,
            Status = "active",
            MaxRetryCount = request.MaxRetryCount > 0 ? request.MaxRetryCount : 10,
            TimeoutSeconds = request.TimeoutSeconds > 0 ? request.TimeoutSeconds : 10,
            Metadata = request.Metadata != null ? JsonSerializer.Serialize(request.Metadata) : null
        };

        await _subRepo.AddAsync(sub, ct);

        await _auditRepo.AddAsync(new AuditLog
        {
            TenantId = effectiveTenant,
            ActorUserId = ActorUserId,
            Action = "webhook.subscription_created",
            EntityType = "WebhookSubscription",
            EntityId = sub.Id.ToString()
        }, ct);

        await _uow.SaveChangesAsync(ct);

        return Ok(new { sub.Id, Secret = plainSecret, sub.Status });
    }

    [HttpGet("subscriptions/{subscriptionId:guid}")]
    public async Task<ActionResult<object>> GetSubscription(Guid subscriptionId, CancellationToken ct)
    {
        var sub = await _subRepo.GetByIdAsync(subscriptionId, ct);
        if (sub == null || sub.DeletedAt != null) return NotFound();
        return Ok(SafeSubscription(sub));
    }

    [HttpPatch("subscriptions/{subscriptionId:guid}")]
    public async Task<ActionResult<object>> UpdateSubscription(
        Guid subscriptionId, [FromBody] UpdateWebhookRequest request, CancellationToken ct)
    {
        var sub = await _subRepo.GetByIdAsync(subscriptionId, ct);
        if (sub == null || sub.DeletedAt != null) return NotFound();

        // Validate webhook URL if provided
        if (!string.IsNullOrEmpty(request.Url))
        {
            var urlError = ValidateWebhookUrl(request.Url);
            if (urlError != null)
                return BadRequest(new { error = "webhook_subscription_invalid_url", message = urlError });
        }

        if (!string.IsNullOrEmpty(request.Name)) sub.Name = request.Name;
        if (!string.IsNullOrEmpty(request.Url)) sub.Url = request.Url;
        if (request.EventTypes != null) sub.EventTypes = request.EventTypes;
        if (request.Headers != null) sub.Headers = JsonSerializer.Serialize(request.Headers);
        if (request.MaxRetryCount.HasValue) sub.MaxRetryCount = request.MaxRetryCount.Value;
        if (request.TimeoutSeconds.HasValue) sub.TimeoutSeconds = request.TimeoutSeconds.Value;
        if (request.Metadata != null) sub.Metadata = JsonSerializer.Serialize(request.Metadata);

        await _auditRepo.AddAsync(new AuditLog
        {
            TenantId = sub.TenantId,
            ActorUserId = ActorUserId,
            Action = "webhook.subscription_updated",
            EntityType = "WebhookSubscription",
            EntityId = sub.Id.ToString()
        }, ct);

        await _uow.SaveChangesAsync(ct);
        return Ok(SafeSubscription(sub));
    }

    [HttpPost("subscriptions/{subscriptionId:guid}/disable")]
    public async Task<ActionResult> DisableSubscription(Guid subscriptionId, CancellationToken ct)
    {
        var sub = await _subRepo.GetByIdAsync(subscriptionId, ct);
        if (sub == null) return NotFound();

        sub.Status = "disabled";
        sub.DisabledAt = DateTime.UtcNow;

        await _auditRepo.AddAsync(new AuditLog
        {
            TenantId = sub.TenantId, ActorUserId = ActorUserId,
            Action = "webhook.subscription_disabled",
            EntityType = "WebhookSubscription", EntityId = sub.Id.ToString()
        }, ct);

        await _uow.SaveChangesAsync(ct);
        return Ok(new { sub.Id, sub.Status });
    }

    [HttpPost("subscriptions/{subscriptionId:guid}/enable")]
    public async Task<ActionResult> EnableSubscription(Guid subscriptionId, CancellationToken ct)
    {
        var sub = await _subRepo.GetByIdAsync(subscriptionId, ct);
        if (sub == null) return NotFound();

        sub.Status = "active";
        sub.DisabledAt = null;

        await _auditRepo.AddAsync(new AuditLog
        {
            TenantId = sub.TenantId, ActorUserId = ActorUserId,
            Action = "webhook.subscription_enabled",
            EntityType = "WebhookSubscription", EntityId = sub.Id.ToString()
        }, ct);

        await _uow.SaveChangesAsync(ct);
        return Ok(new { sub.Id, sub.Status });
    }

    [HttpPost("subscriptions/{subscriptionId:guid}/regenerate-secret")]
    public async Task<ActionResult<object>> RegenerateSecret(Guid subscriptionId, CancellationToken ct)
    {
        var sub = await _subRepo.GetByIdAsync(subscriptionId, ct);
        if (sub == null) return NotFound();

        var (plainSecret, encryptedSecret, secretHash) = _signingService.GenerateSecret();
        sub.SecretHash = secretHash;
        sub.EncryptedSecret = encryptedSecret;

        await _auditRepo.AddAsync(new AuditLog
        {
            TenantId = sub.TenantId, ActorUserId = ActorUserId,
            Action = "webhook.secret_regenerated",
            EntityType = "WebhookSubscription", EntityId = sub.Id.ToString()
        }, ct);

        await _uow.SaveChangesAsync(ct);
        return Ok(new { sub.Id, Secret = plainSecret });
    }

    [HttpDelete("subscriptions/{subscriptionId:guid}")]
    public async Task<ActionResult> DeleteSubscription(Guid subscriptionId, CancellationToken ct)
    {
        var sub = await _subRepo.GetByIdAsync(subscriptionId, ct);
        if (sub == null) return NotFound();

        sub.DeletedAt = DateTime.UtcNow;
        sub.Status = "deleted";

        await _auditRepo.AddAsync(new AuditLog
        {
            TenantId = sub.TenantId, ActorUserId = ActorUserId,
            Action = "webhook.subscription_deleted",
            EntityType = "WebhookSubscription", EntityId = sub.Id.ToString()
        }, ct);

        await _uow.SaveChangesAsync(ct);
        return Ok(new { sub.Id, status = "deleted" });
    }

    // ─── Deliveries ─────────────────────────────────────────

    [HttpGet("deliveries")]
    public async Task<ActionResult<object>> ListDeliveries(
        [FromQuery] Guid? subscriptionId,
        [FromQuery] string? status,
        [FromQuery] int skip = 0, [FromQuery] int take = 50,
        CancellationToken ct = default)
    {
        var effectiveTenant = TenantId != Guid.Empty ? TenantId : (Guid?)null;
        var total = await _deliveryRepo.CountAsync(effectiveTenant, subscriptionId, status, ct);
        var items = await _deliveryRepo.ListAsync(effectiveTenant, subscriptionId, status, skip, take, ct);

        return Ok(new { items = items.Select(SafeDelivery), total, skip, take });
    }

    [HttpGet("deliveries/{deliveryId:guid}")]
    public async Task<ActionResult<object>> GetDelivery(Guid deliveryId, CancellationToken ct)
    {
        var delivery = await _deliveryRepo.GetByIdAsync(deliveryId, ct);
        if (delivery == null) return NotFound();
        return Ok(SafeDelivery(delivery));
    }

    [HttpPost("deliveries/{deliveryId:guid}/retry")]
    public async Task<ActionResult> RetryDelivery(Guid deliveryId, CancellationToken ct)
    {
        var delivery = await _deliveryRepo.GetByIdAsync(deliveryId, ct);
        if (delivery == null) return NotFound();

        if (delivery.Status != "failed" && delivery.Status != "dead_lettered")
            return BadRequest(new { error = "webhook_delivery_invalid_state" });

        delivery.Status = "pending";
        delivery.AttemptCount = 0;
        delivery.NextAttemptAt = DateTime.UtcNow;

        await _auditRepo.AddAsync(new AuditLog
        {
            TenantId = delivery.TenantId, ActorUserId = ActorUserId,
            Action = "webhook.delivery_retried",
            EntityType = "WebhookDelivery", EntityId = delivery.Id.ToString()
        }, ct);

        await _uow.SaveChangesAsync(ct);
        return Ok(new { delivery.Id, status = "pending" });
    }

    // ─── Helpers ────────────────────────────────────────────

    private static string? ValidateWebhookUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return "URL is required";

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return "URL must be an absolute URL";

        if (uri.Scheme != "https" && uri.Scheme != "http")
            return "URL scheme must be http or https";

        // In production, require HTTPS
        var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
        if (!isDevelopment && uri.Scheme != "https")
            return "Webhook URL must use HTTPS in production";

        // Block loopback URLs (localhost, 127.0.0.1, [::1]) in production
        if (!isDevelopment && (uri.IsLoopback || uri.Host == "localhost"))
            return "Webhook URL must not point to localhost in production";

        return null;
    }

    private static object SafeSubscription(WebhookSubscription s) => new
    {
        s.Id, s.TenantId, s.ApplicationCode, s.Name, s.Url,
        s.Status, s.EventTypes, s.MaxRetryCount, s.TimeoutSeconds,
        s.CreatedAt, s.UpdatedAt, s.DisabledAt,
        s.CreatedByUserId,
        // NEVER return secret
    };

    private static object SafeDelivery(WebhookDelivery d) => new
    {
        d.Id, d.TenantId, d.ApplicationCode,
        d.OutboxEventId, d.WebhookSubscriptionId,
        d.EventType, d.EventVersion, d.Status,
        d.AttemptCount, d.MaxAttempts, d.NextAttemptAt,
        d.LastAttemptAt, d.LastStatusCode, d.LastErrorMessage,
        d.ResponseBodySafe, d.DeliveredAt, d.FailedAt, d.CreatedAt
    };
}

public record CreateWebhookRequest(
    Guid? TenantId,
    string? ApplicationCode,
    string Name,
    string Url,
    string[]? EventTypes,
    Dictionary<string, string>? Headers,
    int MaxRetryCount,
    int TimeoutSeconds,
    Dictionary<string, object>? Metadata);

public record UpdateWebhookRequest(
    string? Name,
    string? Url,
    string[]? EventTypes,
    Dictionary<string, string>? Headers,
    int? MaxRetryCount,
    int? TimeoutSeconds,
    Dictionary<string, object>? Metadata);
