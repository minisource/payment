using System.Net.Http.Json;
using System.Text.Json;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Minisource.Common.Tenancy;
using Application.Features.Outbox;

namespace Application.Features.Notifications;

/// <summary>
/// Payment event model for sending notifications.
/// </summary>
public sealed class PaymentNotificationEvent
{
    public string NotificationKey { get; init; } = default!;
    public Guid TenantId { get; init; }
    public string ApplicationCode { get; init; } = "payment";
    public string Channel { get; init; } = "in_app";
    public string RecipientType { get; init; } = default!; // user|admin
    public Guid? RecipientUserId { get; init; }
    public IReadOnlyCollection<Guid>? AdminUserIds { get; init; }
    public string Title { get; init; } = default!;
    public string Body { get; init; } = default!;
    public string? Locale { get; init; }
    public Dictionary<string, string>? Data { get; init; }
    public string? IdempotencyKey { get; init; }
}

public interface IPaymentNotificationService
{
    Task SendAsync(PaymentNotificationEvent evt, CancellationToken ct = default);
}

/// <summary>
/// Sends tenant-aware in-app notifications to the Notifier service.
/// Respects per-tenant notification settings. Fails gracefully when Notifier is unavailable.
/// </summary>
public class PaymentNotificationService : IPaymentNotificationService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<PaymentNotificationService> _logger;
    private readonly NotifierOptions _options;
    private readonly IServiceScopeFactory _scopeFactory;

    public PaymentNotificationService(
        IHttpClientFactory httpClientFactory,
        IOptions<NotifierOptions> options,
        ILogger<PaymentNotificationService> logger,
        IServiceScopeFactory scopeFactory)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _options = options.Value;
        _scopeFactory = scopeFactory;
    }

    public async Task SendAsync(PaymentNotificationEvent evt, CancellationToken ct = default)
    {
        if (!_options.Enabled || !_options.SendInAppNotifications)
        {
            _logger.LogDebug("Notifier disabled, skipping notification: {Key}", evt.NotificationKey);
            return;
        }

        if (string.IsNullOrEmpty(_options.BaseUrl))
        {
            _logger.LogWarning("Notifier BaseUrl not configured, skipping notification: {Key}", evt.NotificationKey);
            return;
        }

        if (evt.TenantId == Guid.Empty)
        {
            _logger.LogError("Notification skipped — missing TenantId for key: {Key}", evt.NotificationKey);
            return;
        }

        // Check notification settings — default = enabled
        if (!await IsNotificationEnabledAsync(evt, ct))
        {
            _logger.LogDebug("Notification disabled by tenant setting: {Key} tenant={TenantId}",
                evt.NotificationKey, evt.TenantId);
            return;
        }

        // Resolve recipients
        var userIds = ResolveRecipients(evt, _options);
        if (userIds.Count == 0)
        {
            _logger.LogDebug("No recipients for notification: {Key}", evt.NotificationKey);
            return;
        }

        // Build base idempotency key
        var baseKey = evt.IdempotencyKey
            ?? $"payment:{evt.NotificationKey}:{evt.TenantId}:{evt.RecipientType}:{DateTime.UtcNow:yyyyMMddHHmmssfff}";

        try
        {
            var client = _httpClientFactory.CreateClient("NotifierClient");
            client.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds > 0 ? _options.TimeoutSeconds : 10);

            foreach (var userId in userIds)
            {
                // Per-user idempotency key to avoid conflicts
                var perUserKey = $"{baseKey}:{userId}";

                var payload = new
                {
                    channel = evt.Channel,
                    userId = userId.ToString(),
                    subject = evt.Title,
                    body = evt.Body,
                    locale = evt.Locale ?? "en",
                    metadata = evt.Data ?? new Dictionary<string, string>(),
                    idempotencyKey = perUserKey
                };

                var httpRequest = new HttpRequestMessage(HttpMethod.Post,
                    $"{_options.BaseUrl.TrimEnd('/')}/api/v1/notifications")
                {
                    Content = JsonContent.Create(payload, options: new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    })
                };

                httpRequest.Headers.Add(RequestHeaders.TenantId, evt.TenantId.ToString());
                httpRequest.Headers.Add(RequestHeaders.ApplicationCode, evt.ApplicationCode);
                httpRequest.Headers.Add(RequestHeaders.IdempotencyKey, perUserKey);

                var response = await client.SendAsync(httpRequest, ct);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogDebug("Notification sent: {Key} → user:{UserId}", evt.NotificationKey, userId);
                }
                else
                {
                    var body = await response.Content.ReadAsStringAsync(ct);
                    _logger.LogWarning("Notification send failed: {Key} → user:{UserId} ({StatusCode}): {Body}",
                        evt.NotificationKey, userId, (int)response.StatusCode,
                        body.Length > 300 ? body[..300] : body);
                }
            }
        }
        catch (Exception ex)
        {
            // Never fail the business operation due to notification failure
            _logger.LogError(ex, "Notification send error: {Key}", evt.NotificationKey);
        }
    }

    /// <summary>
    /// Checks if a notification is enabled for the given tenant.
    /// Default is enabled when no override exists.
    /// </summary>
    private async Task<bool> IsNotificationEnabledAsync(PaymentNotificationEvent evt, CancellationToken ct)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();

            var setting = await db.PaymentNotificationSettings
                .FirstOrDefaultAsync(s =>
                    s.TenantId == evt.TenantId &&
                    s.NotificationKey == evt.NotificationKey &&
                    s.Channel == evt.Channel &&
                    s.RecipientType == evt.RecipientType &&
                    s.DeletedAt == null, ct);

            // If no setting row exists, default is enabled
            return setting?.IsEnabled ?? true;
        }
        catch (Exception ex)
        {
            // If we can't check settings, default to enabled (don't silently suppress)
            _logger.LogWarning(ex, "Failed to check notification settings for {Key}, defaulting to enabled",
                evt.NotificationKey);
            return true;
        }
    }

    private static List<Guid> ResolveRecipients(PaymentNotificationEvent evt, NotifierOptions options)
    {
        var userIds = new List<Guid>();

        if (evt.RecipientUserId.HasValue)
            userIds.Add(evt.RecipientUserId.Value);

        if (evt.AdminUserIds is { Count: > 0 })
            userIds.AddRange(evt.AdminUserIds);

        // Fallback: if admin recipients not explicitly provided, use config
        if (evt.RecipientType == "admin" && userIds.Count == 0 && options.AdminUserIds is { Length: > 0 })
        {
            foreach (var idStr in options.AdminUserIds)
            {
                if (Guid.TryParse(idStr, out var adminId))
                    userIds.Add(adminId);
            }
        }

        return userIds.Distinct().ToList();
    }
}
