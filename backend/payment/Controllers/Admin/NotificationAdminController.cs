using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Minisource.Common.Tenancy;
using Application.Features.Notifications;

namespace payment.Controllers.Admin;

/// <summary>
/// Admin notification APIs that proxy to the Notifier service.
/// Payment frontend calls these endpoints; Payment backend forwards to Notifier.
/// This keeps auth/tenant behavior centralized in Payment.
/// </summary>
[Authorize(Policy = "WalletAdmin")]
[ApiController]
[Route("api/v1/admin/notifications")]
public class NotificationAdminController : PaymentControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly PaymentDbContext _db;
    private readonly ILogger<NotificationAdminController> _logger;

    public NotificationAdminController(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        PaymentDbContext db,
        ILogger<NotificationAdminController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _db = db;
        _logger = logger;
    }

    private string NotifierBaseUrl =>
        _configuration.GetValue<string>("Notifier:BaseUrl") ?? "http://notifier:8080";

    private bool NotifierEnabled =>
        _configuration.GetValue<bool>("Notifier:Enabled");

    // ─── List Notifications ──────────────────────────

    /// <summary>
    /// Lists admin notifications with optional tenant/user/status filters.
    /// GET /api/v1/admin/notifications
    /// </summary>
    [HttpGet]
    public async Task<ActionResult> ListNotifications(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        [FromQuery] string? channel = null,
        [FromQuery] string? userId = null,
        [FromQuery] string? search = null,
        [FromQuery] string? from = null,
        [FromQuery] string? to = null,
        CancellationToken ct = default)
    {
        if (!NotifierEnabled)
            return Ok(new { data = Array.Empty<object>(), total = 0, page, pageSize, totalPages = 0 });

        var effectiveTenantId = GetEffectiveTenantId();
        var queryParams = new List<string>
        {
            $"page={page}",
            $"pageSize={pageSize}"
        };

        if (!string.IsNullOrEmpty(status)) queryParams.Add($"status={Uri.EscapeDataString(status)}");
        if (!string.IsNullOrEmpty(channel)) queryParams.Add($"channel={Uri.EscapeDataString(channel)}");
        if (!string.IsNullOrEmpty(userId)) queryParams.Add($"userId={Uri.EscapeDataString(userId)}");
        if (!string.IsNullOrEmpty(search)) queryParams.Add($"search={Uri.EscapeDataString(search)}");
        if (!string.IsNullOrEmpty(from)) queryParams.Add($"from={Uri.EscapeDataString(from)}");
        if (!string.IsNullOrEmpty(to)) queryParams.Add($"to={Uri.EscapeDataString(to)}");

        if (effectiveTenantId.HasValue)
            queryParams.Add($"tenantId={effectiveTenantId.Value}");

        var url = $"{NotifierBaseUrl.TrimEnd('/')}/api/v1/notifications?{string.Join("&", queryParams)}";

        return await ProxyGetAsync(url, effectiveTenantId, ct);
    }

    // ─── Get Notification Detail ─────────────────────

    /// <summary>
    /// Gets a single notification by ID.
    /// GET /api/v1/admin/notifications/{id}
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult> GetNotification(string id, CancellationToken ct)
    {
        if (!NotifierEnabled)
            return NotFound();

        var url = $"{NotifierBaseUrl.TrimEnd('/')}/api/v1/notifications/{id}";
        return await ProxyGetAsync(url, GetEffectiveTenantId(), ct);
    }

    // ─── Mark as Read ────────────────────────────────

    /// <summary>
    /// Marks a single notification as read.
    /// PUT /api/v1/admin/notifications/{id}/read
    /// </summary>
    [HttpPut("{id}/read")]
    public async Task<ActionResult> MarkAsRead(string id, CancellationToken ct)
    {
        if (!NotifierEnabled)
            return Ok(new { message = "Notifier disabled" });

        var url = $"{NotifierBaseUrl.TrimEnd('/')}/api/v1/notifications/{id}/read";
        return await ProxyPutAsync(url, ct);
    }

    // ─── Unread Count ────────────────────────────────

    /// <summary>
    /// Gets unread notification count for the current actor.
    /// GET /api/v1/admin/notifications/unread-count
    /// </summary>
    [HttpGet("unread-count")]
    public async Task<ActionResult> GetUnreadCount(CancellationToken ct)
    {
        if (!NotifierEnabled)
            return Ok(new { userId = ActorUserId.ToString(), unreadCount = 0 });

        var userId = ActorUserId;
        if (userId == Guid.Empty)
            return Ok(new { userId = "", unreadCount = 0 });

        var url = $"{NotifierBaseUrl.TrimEnd('/')}/api/v1/notifications/user/{userId}/unread-count";
        return await ProxyGetAsync(url, GetEffectiveTenantId(), ct);
    }

    // ─── Mark All as Read ────────────────────────────

    /// <summary>
    /// Marks all notifications as read for the current actor.
    /// POST /api/v1/admin/notifications/read-all
    /// </summary>
    [HttpPost("read-all")]
    public async Task<ActionResult> MarkAllAsRead(CancellationToken ct)
    {
        if (!NotifierEnabled)
            return Ok(new { message = "Notifier disabled", userId = "", updatedCount = 0 });

        var userId = ActorUserId;
        if (userId == Guid.Empty)
            return BadRequest(new { error = "User ID required" });

        var url = $"{NotifierBaseUrl.TrimEnd('/')}/api/v1/notifications/user/{userId}/read-all";
        return await ProxyPostAsync(url, ct);
    }

    // ─── Notification Settings ────────────────────────

    /// <summary>
    /// Gets notification settings for a tenant (or global defaults).
    /// GET /api/v1/admin/notification-settings
    /// </summary>
    [HttpGet("../notification-settings")]
    public async Task<ActionResult> GetNotificationSettings(CancellationToken ct)
    {
        var effectiveTenantId = GetEffectiveTenantId();
        var query = _db.PaymentNotificationSettings.AsQueryable();

        if (effectiveTenantId.HasValue)
            query = query.Where(s => s.TenantId == effectiveTenantId.Value && s.DeletedAt == null);
        else
            query = query.Where(s => s.DeletedAt == null);

        var settings = await query.OrderBy(s => s.NotificationKey).ToListAsync(ct);

        // Merge with registry to show all keys (defaults = enabled)
        var result = PaymentNotificationKeys.All.Select(keyInfo =>
        {
            var setting = settings.FirstOrDefault(s =>
                s.NotificationKey == keyInfo.Key &&
                s.Channel == "in_app" &&
                s.RecipientType == keyInfo.RecipientType);

            return new
            {
                notification_key = keyInfo.Key,
                channel = "in_app",
                recipient_type = keyInfo.RecipientType,
                description = keyInfo.Description,
                is_enabled = setting?.IsEnabled ?? true,
                tenant_id = effectiveTenantId?.ToString(),
                application_code = "payment"
            };
        }).ToList();

        return Ok(new { items = result });
    }

    /// <summary>
    /// Gets all known notification keys for frontend discovery.
    /// GET /api/v1/admin/notification-settings/keys
    /// </summary>
    [HttpGet("../notification-settings/keys")]
    public ActionResult GetNotificationKeys()
    {
        return Ok(new
        {
            keys = PaymentNotificationKeys.All.Select(k => new
            {
                key = k.Key,
                recipient_type = k.RecipientType,
                description = k.Description
            })
        });
    }

    /// <summary>
    /// Updates a notification setting for a specific tenant.
    /// PUT /api/v1/admin/notification-settings/{notificationKey}
    /// </summary>
    [HttpPut("../notification-settings/{notificationKey}")]
    public async Task<ActionResult> UpdateNotificationSetting(
        string notificationKey,
        [FromBody] UpdateNotificationSettingRequest request,
        CancellationToken ct)
    {
        var tenantId = GetRequiredTenantId();

        var setting = await _db.PaymentNotificationSettings
            .FirstOrDefaultAsync(s =>
                s.TenantId == tenantId &&
                s.NotificationKey == notificationKey &&
                s.Channel == (request.Channel ?? "in_app") &&
                s.RecipientType == (request.RecipientType ?? "admin") &&
                s.DeletedAt == null, ct);

        if (setting == null)
        {
            setting = PaymentNotificationSetting.Create(
                tenantId, notificationKey,
                request.Channel ?? "in_app",
                request.RecipientType ?? "admin",
                request.IsEnabled);
            await _db.PaymentNotificationSettings.AddAsync(setting, ct);
        }
        else
        {
            setting.IsEnabled = request.IsEnabled;
            setting.UpdatedAt = DateTime.UtcNow;
            setting.UpdatedByUserId = ActorUserId != Guid.Empty ? ActorUserId : null;
        }

        await _db.SaveChangesAsync(ct);

        return Ok(new
        {
            notification_key = setting.NotificationKey,
            channel = setting.Channel,
            recipient_type = setting.RecipientType,
            is_enabled = setting.IsEnabled,
            tenant_id = setting.TenantId
        });
    }

    // ─── Proxy Helpers ───────────────────────────────

    private async Task<ActionResult> ProxyGetAsync(string url, Guid? tenantId, CancellationToken ct)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("NotifierProxy");
            client.Timeout = TimeSpan.FromSeconds(10);

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            if (tenantId.HasValue)
                request.Headers.Add(RequestHeaders.TenantId, tenantId.Value.ToString());

            var response = await client.SendAsync(request, ct);
            var body = await response.Content.ReadAsStringAsync(ct);

            return Content(body, "application/json");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Notifier proxy GET failed: {Url}", url);
            return Ok(new { data = Array.Empty<object>(), total = 0, page = 1, pageSize = 20, totalPages = 0 });
        }
    }

    private async Task<ActionResult> ProxyPutAsync(string url, CancellationToken ct)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("NotifierProxy");
            client.Timeout = TimeSpan.FromSeconds(10);

            var response = await client.PutAsync(url, null, ct);
            var body = await response.Content.ReadAsStringAsync(ct);
            return Content(body, "application/json");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Notifier proxy PUT failed: {Url}", url);
            return Ok(new { message = "Operation queued" });
        }
    }

    private async Task<ActionResult> ProxyPostAsync(string url, CancellationToken ct)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("NotifierProxy");
            client.Timeout = TimeSpan.FromSeconds(10);

            var response = await client.PostAsync(url, null, ct);
            var body = await response.Content.ReadAsStringAsync(ct);
            return Content(body, "application/json");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Notifier proxy POST failed: {Url}", url);
            return Ok(new { message = "Operation queued" });
        }
    }
}

public sealed record UpdateNotificationSettingRequest
{
    public bool IsEnabled { get; init; }
    public string? Channel { get; init; }
    public string? RecipientType { get; init; }
}
