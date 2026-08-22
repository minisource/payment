using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Application.Features.Outbox;

/// <summary>
/// Publishes domain events to the Notifier service via generic event ingestion.
/// Admin in-app notifications are handled by DomainEventDispatcher (in-process),
/// not via the outbox, to avoid duplicate delivery.
/// </summary>
public class NotifierEventPublisher : IIntegrationEventPublisher
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<NotifierEventPublisher> _logger;
    private readonly NotifierOptions _options;

    public string PublisherName => "notifier";

    public NotifierEventPublisher(
        IHttpClientFactory httpClientFactory,
        Microsoft.Extensions.Options.IOptions<NotifierOptions> options,
        ILogger<NotifierEventPublisher> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<PublishResult> PublishAsync(OutboxEventMessage message, CancellationToken ct)
    {
        if (!_options.Enabled)
            return new PublishResult(true, false, null, null);

        if (string.IsNullOrEmpty(_options.BaseUrl))
            return new PublishResult(false, false, "notifier_no_base_url", "Notifier base URL not configured");

        // Admin notifications are now handled by DomainEventDispatcher (in-process),
        // not via the outbox — to avoid duplicate delivery.
        // The NotifierEventPublisher now only handles generic event ingestion.

        // Generic event ingestion via /api/v1/events/ingest
        return await SendGenericEventAsync(message, ct);
    }

    /// <summary>
    /// Sends the raw domain event to the Notifier's generic event ingestion endpoint.
    /// </summary>
    private async Task<PublishResult> SendGenericEventAsync(OutboxEventMessage message, CancellationToken ct)
    {
        try
        {
            var payload = new
            {
                tenant_id = message.TenantId,
                event_type = message.EventType,
                event_version = message.EventVersion,
                aggregate_type = message.AggregateType,
                aggregate_id = message.AggregateId,
                correlation_id = ExtractHeader(message.Headers, "correlation_id"),
                payload = TryDeserializePayload(message.Payload),
                occurred_at = message.OccurredAt
            };

            var client = _httpClientFactory.CreateClient("NotifierClient");
            client.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds > 0 ? _options.TimeoutSeconds : 10);

            var response = await client.PostAsJsonAsync(
                $"{_options.BaseUrl.TrimEnd('/')}/api/v1/events/ingest",
                payload, ct);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogDebug("Notifier event sent: {EventType}", message.EventType);
                return new PublishResult(true, false, null, null);
            }

            var body = await response.Content.ReadAsStringAsync(ct);
            _logger.LogWarning("Notifier publish failed: {EventType} ({StatusCode}): {Body}",
                message.EventType, (int)response.StatusCode, Truncate(body, 300));

            var retryable = (int)response.StatusCode >= 500 || (int)response.StatusCode == 429;
            return new PublishResult(false, retryable, $"notifier_http_{(int)response.StatusCode}", Truncate(body, 500));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Notifier publish error: {EventType}", message.EventType);
            return new PublishResult(false, true, "notifier_error", Truncate(ex.Message, 500));
        }
    }

    // ── JSON Helpers ──

    private static object? TryDeserializePayload(string payload)
    {
        try { return JsonSerializer.Deserialize<object>(payload); }
        catch { return payload; }
    }

    private static string? ExtractHeader(string? headers, string key)
    {
        if (string.IsNullOrEmpty(headers)) return null;
        try
        {
            var doc = JsonDocument.Parse(headers);
            return doc.RootElement.TryGetProperty(key, out var val) ? val.GetString() : null;
        }
        catch { return null; }
    }

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength] + "...";
}

public class NotifierOptions
{
    public bool Enabled { get; set; } = false;
    public string BaseUrl { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 10;
    public int RetryCount { get; set; } = 3;
    public string ApplicationCode { get; set; } = "payment";
    public bool SendInAppNotifications { get; set; } = true;
    public bool FailPaymentOperationOnNotificationFailure { get; set; } = false;
    public bool UseOutbox { get; set; } = true;
    /// <summary>
    /// Admin user UUIDs that receive admin notifications.
    /// In production, replace with Auth-service-based recipient resolution.
    /// </summary>
    public string[]? AdminUserIds { get; set; }
}

/// <summary>
/// No-op message bus publisher used when message bus is disabled.
/// </summary>
public class NoopMessageBusPublisher : IIntegrationEventPublisher
{
    public string PublisherName => "message_bus";
    public Task<PublishResult> PublishAsync(OutboxEventMessage message, CancellationToken ct)
        => Task.FromResult(new PublishResult(true, false, null, null));
}
