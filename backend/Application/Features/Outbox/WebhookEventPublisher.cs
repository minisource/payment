using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using Application.Features.Webhooks;
using Microsoft.Extensions.Options;

namespace Application.Features.Outbox;

/// <summary>
/// Publishes events to configured webhook endpoints with HMAC signing.
/// </summary>
public class WebhookEventPublisher : IIntegrationEventPublisher
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IWebhookSigningService _signingService;
    private readonly ILogger<WebhookEventPublisher> _logger;

    public string PublisherName => "webhook";

    public WebhookEventPublisher(
        IHttpClientFactory httpClientFactory,
        IWebhookSigningService signingService,
        ILogger<WebhookEventPublisher> logger)
    {
        _httpClientFactory = httpClientFactory;
        _signingService = signingService;
        _logger = logger;
    }

    /// <summary>
    /// Publishes an event to a specific webhook subscription.
    /// The subscription is passed via Headers metadata (Key: __webhook_subscription_json).
    /// </summary>
    public async Task<PublishResult> PublishAsync(OutboxEventMessage message, CancellationToken ct)
    {
        WebhookSubscription? subscription = null;
        try
        {
            subscription = DeserializeSubscription(message.Headers);
            if (subscription == null)
                return new PublishResult(false, false, "webhook_no_subscription", "No webhook subscription context");

            if (subscription.Status != "active")
                return new PublishResult(true, false, null, null); // Skip silently

            var envelope = BuildEnvelope(message);
            var rawBody = JsonSerializer.Serialize(envelope);
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            // Decrypt secret for signing (uses IDataProtector)
            var plainSecret = _signingService.DecryptSecret(subscription.EncryptedSecret ?? string.Empty);
            if (string.IsNullOrEmpty(plainSecret))
                return new PublishResult(false, false, "webhook_no_secret", "Cannot sign without secret");

            var signature = _signingService.ComputeSignature(plainSecret, timestamp, rawBody);

            var client = _httpClientFactory.CreateClient("WebhookClient");
            client.Timeout = TimeSpan.FromSeconds(subscription.TimeoutSeconds > 0 ? subscription.TimeoutSeconds : 10);

            var request = new HttpRequestMessage(HttpMethod.Post, subscription.Url)
            {
                Content = new StringContent(rawBody, Encoding.UTF8, "application/json")
            };

            request.Headers.Add("X-MiniSource-Event-Id", envelope.Id);
            request.Headers.Add("X-MiniSource-Event-Type", envelope.Type);
            request.Headers.Add("X-MiniSource-Event-Version", envelope.Version.ToString());
            if (envelope.TenantId.HasValue)
                request.Headers.Add("X-MiniSource-Tenant-Id", envelope.TenantId.Value.ToString());
            request.Headers.Add("X-MiniSource-Timestamp", timestamp.ToString());
            request.Headers.Add("X-MiniSource-Signature", signature);
            if (!string.IsNullOrEmpty(envelope.CorrelationId))
                request.Headers.Add("X-Correlation-Id", envelope.CorrelationId);

            // Custom headers from subscription
            ParseCustomHeaders(subscription.Headers, request);

            var response = await client.SendAsync(request, ct);
            var statusCode = (int)response.StatusCode;

            if (response.IsSuccessStatusCode)
            {
                _logger.LogDebug("Webhook delivered: {EventType} → {Url} ({StatusCode})",
                    message.EventType, subscription.Url, statusCode);
                return new PublishResult(true, false, null, null);
            }

            var responseBody = await response.Content.ReadAsStringAsync(ct);
            var retryable = IsRetryable(statusCode);

            _logger.LogWarning("Webhook failed: {EventType} → {Url} ({StatusCode}), Retryable={Retryable}",
                message.EventType, subscription.Url, statusCode, retryable);

            return new PublishResult(false, retryable, $"webhook_http_{statusCode}",
                $"HTTP {statusCode}: {Truncate(responseBody, 500)}");
        }
        catch (TaskCanceledException)
        {
            _logger.LogWarning("Webhook timed out: {EventType} → {Url}", message.EventType, subscription?.Url);
            return new PublishResult(false, true, "webhook_timeout", "Request timed out");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Webhook HTTP error: {EventType} → {Url}", message.EventType, subscription?.Url);
            return new PublishResult(false, true, "webhook_http_error", Truncate(ex.Message, 500));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Webhook unexpected error: {EventType}", message.EventType);
            return new PublishResult(false, true, "webhook_unexpected", Truncate(ex.Message, 500));
        }
    }

    private static IntegrationEventEnvelope BuildEnvelope(OutboxEventMessage msg) => new(
        Id: msg.EventId.ToString(),
        Type: msg.EventType,
        Version: msg.EventVersion,
        TenantId: msg.TenantId,
        ApplicationCode: msg.ApplicationCode,
        OccurredAt: msg.OccurredAt,
        AggregateType: msg.AggregateType,
        AggregateId: msg.AggregateId,
        CorrelationId: ExtractHeader(msg.Headers, "correlation_id"),
        RequestId: ExtractHeader(msg.Headers, "request_id"),
        Payload: msg.Payload);

    private static bool IsRetryable(int statusCode) => statusCode switch
    {
        >= 200 and < 300 => false, // success
        408 => true,  // Request Timeout
        429 => true,  // Too Many Requests
        >= 500 => true, // Server errors
        _ => false    // 4xx client errors (not retryable)
    };

    private static WebhookSubscription? DeserializeSubscription(string? headers)
    {
        if (string.IsNullOrEmpty(headers)) return null;
        try
        {
            var doc = JsonDocument.Parse(headers);
            if (doc.RootElement.TryGetProperty("__webhook_subscription_json", out var subJson))
                return JsonSerializer.Deserialize<WebhookSubscription>(subJson.GetString() ?? "");
        }
        catch { }
        return null;
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

    private static void ParseCustomHeaders(string? headers, HttpRequestMessage request)
    {
        if (string.IsNullOrEmpty(headers)) return;
        try
        {
            var doc = JsonDocument.Parse(headers);
            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                if (prop.Name.StartsWith("__")) continue; // Internal metadata
                if (!request.Headers.Contains(prop.Name))
                    request.Headers.TryAddWithoutValidation(prop.Name, prop.Value.GetString());
            }
        }
        catch { }
    }

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength] + "...";
}
