using System.Text.Json;
using Domain.Entities;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Application.Features.Webhooks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.Features.Outbox;

/// <summary>
/// Background worker that polls the outbox for pending events and dispatches them
/// to configured publishers (webhook, notifier, message bus).
/// Uses atomic DB row locking for safe multi-instance operation.
/// </summary>
public class OutboxDispatcherHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxDispatcherHostedService> _logger;
    private readonly OutboxDispatcherOptions _options;

    public OutboxDispatcherHostedService(
        IServiceScopeFactory scopeFactory,
        IOptions<OutboxDispatcherOptions> options,
        ILogger<OutboxDispatcherHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.DispatcherEnabled)
        {
            _logger.LogInformation("Outbox dispatcher is disabled");
            return;
        }

        _logger.LogInformation("Outbox dispatcher started. BatchSize={BatchSize}, PollInterval={PollInterval}s",
            _options.BatchSize, _options.PollIntervalSeconds);

        var instanceId = $"{Environment.MachineName}-{Guid.NewGuid():N}"[..40];

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(instanceId, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Outbox dispatcher batch processing failed");
            }

            await Task.Delay(TimeSpan.FromSeconds(_options.PollIntervalSeconds), stoppingToken);
        }

        _logger.LogInformation("Outbox dispatcher stopped");
    }

    private async Task ProcessBatchAsync(string instanceId, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var outboxRepo = scope.ServiceProvider.GetRequiredService<IOutboxEventRepository>();
        var auditRepo = scope.ServiceProvider.GetRequiredService<IAuditLogRepository>();
        var ctx = scope.ServiceProvider.GetRequiredService<Infrastructure.Persistence.PaymentDbContext>();
        var uow = scope.ServiceProvider.GetRequiredService<Minisource.Common.Domain.IUnitOfWork>();
        var publishers = scope.ServiceProvider.GetServices<IIntegrationEventPublisher>().ToList();

        var lockDuration = TimeSpan.FromSeconds(_options.LockDurationSeconds);
        var batch = await outboxRepo.GetDispatchBatchAsync(_options.BatchSize, instanceId, lockDuration, ct);

        if (batch.Count == 0) return;

        _logger.LogDebug("Dispatching {Count} outbox events", batch.Count);

        foreach (var evt in batch)
        {
            ct.ThrowIfCancellationRequested();
            await ProcessSingleEventAsync(evt, publishers, outboxRepo, auditRepo, ctx, uow, ct);
        }
    }

    private async Task ProcessSingleEventAsync(
        Domain.Entities.OutboxEvent evt,
        List<IIntegrationEventPublisher> publishers,
        IOutboxEventRepository outboxRepo,
        IAuditLogRepository auditRepo,
        Infrastructure.Persistence.PaymentDbContext ctx,
        Minisource.Common.Domain.IUnitOfWork uow,
        CancellationToken ct)
    {
        var message = new OutboxEventMessage(
            EventId: evt.Id,
            EventType: evt.EventType,
            EventVersion: evt.EventVersion,
            TenantId: evt.TenantId,
            ApplicationCode: ExtractHeaderValue(evt.Headers, "application_code"),
            AggregateType: evt.AggregateType,
            AggregateId: evt.AggregateId,
            Payload: evt.Payload,
            Headers: evt.Headers,
            OccurredAt: evt.OccurredAt);

        bool allSucceeded = true;
        string? lastError = null;
        var newRetryCount = evt.RetryCount + 1;

        foreach (var publisher in publishers)
        {
            // Event routing: skip if routing rules exclude this publisher for this event
            if (!await IsPublisherRoutedAsync(ctx, message.EventType, message.TenantId, message.ApplicationCode, publisher.PublisherName, ct))
                continue;

            try
            {
                PublishResult result;
                if (publisher.PublisherName == "webhook")
                {
                    // Webhook: deliver to ALL matching active subscriptions
                    result = await PublishToWebhooksAsync(message, ctx, ct);
                }
                else
                {
                    result = await publisher.PublishAsync(message, ct);
                }

                if (!result.Succeeded)
                {
                    allSucceeded = false;
                    lastError = $"[{publisher.PublisherName}] {result.ErrorCode}: {result.ErrorMessage}";
                    _logger.LogWarning("Publisher {Publisher} failed for event {EventId}: {Error}",
                        publisher.PublisherName, evt.Id, lastError);
                }

                await AuditDeliveryAsync(auditRepo, evt, publisher.PublisherName, result, ct);
            }
            catch (Exception ex)
            {
                allSucceeded = false;
                lastError = $"[{publisher.PublisherName}] {ex.GetType().Name}: {ex.Message}";
                _logger.LogError(ex, "Publisher {Publisher} threw for event {EventId}", publisher.PublisherName, evt.Id);
            }
        }

        // Single SaveChangesAsync for audit + status update
        if (allSucceeded)
        {
            await outboxRepo.MarkAsProcessedAsync(evt.Id, ct);
        }
        else if (newRetryCount >= evt.MaxRetryCount)
        {
            await outboxRepo.MarkAsDeadLetteredAsync(evt.Id, lastError, ct);
            _logger.LogWarning("Event {EventId} dead-lettered after {RetryCount} retries (max {Max})",
                evt.Id, newRetryCount, evt.MaxRetryCount);
        }
        else
        {
            var retryDelay = ComputeBackoff(newRetryCount);
            var nextRetryAt = DateTime.UtcNow + retryDelay;
            await outboxRepo.MarkAsFailedAsync(evt.Id, lastError, nextRetryAt, ct);
        }

        await uow.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Publishes a single outbox event to all matching active webhook subscriptions.
    /// Creates WebhookDelivery records for tracking each delivery attempt.
    /// </summary>
    private async Task<PublishResult> PublishToWebhooksAsync(
        OutboxEventMessage message,
        Infrastructure.Persistence.PaymentDbContext ctx,
        CancellationToken ct)
    {
        // Find all active webhook subscriptions matching this event
        var subscriptions = await ctx.WebhookSubscriptions
            .Where(s => s.Status == "active" && s.DeletedAt == null
                && (s.TenantId == message.TenantId || message.TenantId == null)
                && (s.ApplicationCode == message.ApplicationCode || s.ApplicationCode == null))
            .ToListAsync(ct);

        // Filter by event type
        subscriptions = subscriptions
            .Where(s => s.EventTypes.Length == 0 || s.EventTypes.Contains(message.EventType)
                || s.EventTypes.Contains("*"))
            .ToList();

        if (subscriptions.Count == 0)
            return new PublishResult(true, false, null, null); // No matching subscriptions — success

        bool allSucceeded = true;
        string? lastError = null;
        bool anyRetryable = false;

        foreach (var sub in subscriptions)
        {
            // Create a WebhookDelivery record for tracking
            var delivery = new WebhookDelivery
            {
                TenantId = sub.TenantId,
                ApplicationCode = sub.ApplicationCode,
                OutboxEventId = message.EventId,
                WebhookSubscriptionId = sub.Id,
                EventType = message.EventType,
                EventVersion = message.EventVersion,
                Status = "processing",
                MaxAttempts = sub.MaxRetryCount,
                AttemptCount = 1,
                LastAttemptAt = DateTime.UtcNow
            };
            await ctx.WebhookDeliveries.AddAsync(delivery, ct);
            await ctx.SaveChangesAsync(ct); // Save immediately so delivery exists even if HTTP fails

            // Now try to deliver the webhook
            try
            {
                var webhookResult = await TryDeliverWebhookAsync(sub, message, delivery, ct);

                if (webhookResult.Succeeded)
                {
                    delivery.Status = "delivered";
                    delivery.DeliveredAt = DateTime.UtcNow;
                }
                else
                {
                    allSucceeded = false;
                    delivery.Status = "failed";
                    delivery.LastErrorMessage = $"{webhookResult.ErrorCode}: {webhookResult.ErrorMessage}";
                    delivery.FailedAt = DateTime.UtcNow;
                    if (webhookResult.Retryable)
                    {
                        delivery.NextAttemptAt = DateTime.UtcNow + ComputeBackoff(1);
                        anyRetryable = true;
                    }
                    else
                    {
                        delivery.Status = "dead_lettered";
                    }
                    lastError = $"[webhook:{sub.Name}] {webhookResult.ErrorCode}: {webhookResult.ErrorMessage}";
                }
            }
            catch (Exception ex)
            {
                allSucceeded = false;
                anyRetryable = true;
                delivery.Status = "failed";
                delivery.LastErrorMessage = ex.Message;
                delivery.FailedAt = DateTime.UtcNow;
                delivery.NextAttemptAt = DateTime.UtcNow + ComputeBackoff(1);
                lastError = $"[webhook:{sub.Name}] {ex.GetType().Name}: {ex.Message}";
            }

            await ctx.SaveChangesAsync(ct);
        }

        return new PublishResult(allSucceeded, allSucceeded ? false : anyRetryable, allSucceeded ? null : "webhook_partial_failure", lastError);
    }

    private async Task<PublishResult> TryDeliverWebhookAsync(
        WebhookSubscription subscription,
        OutboxEventMessage message,
        WebhookDelivery delivery,
        CancellationToken ct)
    {
        // Resolve services needed for direct HTTP delivery
        using var scope = _scopeFactory.CreateScope();
        var signingService = scope.ServiceProvider.GetRequiredService<IWebhookSigningService>();
        var httpFactory = scope.ServiceProvider.GetRequiredService<System.Net.Http.IHttpClientFactory>();

        try
        {
            var envelope = new IntegrationEventEnvelope(
                Id: message.EventId.ToString(),
                Type: message.EventType,
                Version: message.EventVersion,
                TenantId: message.TenantId,
                ApplicationCode: message.ApplicationCode,
                OccurredAt: message.OccurredAt,
                AggregateType: message.AggregateType,
                AggregateId: message.AggregateId,
                CorrelationId: ExtractHeaderValue(message.Headers, "correlation_id"),
                RequestId: ExtractHeaderValue(message.Headers, "request_id"),
                Payload: message.Payload);

            var rawBody = JsonSerializer.Serialize(envelope);
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            var plainSecret = signingService.DecryptSecret(subscription.EncryptedSecret ?? string.Empty);
            if (string.IsNullOrEmpty(plainSecret))
                return new PublishResult(false, false, "webhook_no_secret", "No secret available");

            var signature = signingService.ComputeSignature(plainSecret, timestamp, rawBody);

            var client = httpFactory.CreateClient("WebhookClient");
            client.Timeout = TimeSpan.FromSeconds(subscription.TimeoutSeconds > 0 ? subscription.TimeoutSeconds : 10);

            var request = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Post, subscription.Url)
            {
                Content = new System.Net.Http.StringContent(rawBody, System.Text.Encoding.UTF8, "application/json")
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

            var response = await client.SendAsync(request, ct);
            var statusCode = (int)response.StatusCode;
            delivery.LastStatusCode = statusCode;

            if (response.IsSuccessStatusCode)
            {
                _logger.LogDebug("Webhook delivered to {Url} ({StatusCode})", subscription.Url, statusCode);
                return new PublishResult(true, false, null, null);
            }

            var responseBody = await response.Content.ReadAsStringAsync(ct);
            delivery.ResponseBodySafe = responseBody.Length > 500 ? responseBody[..500] : responseBody;
            var retryable = statusCode == 408 || statusCode == 429 || statusCode >= 500;
            _logger.LogWarning("Webhook failed: {Url} ({StatusCode})", subscription.Url, statusCode);
            return new PublishResult(false, retryable, $"webhook_http_{statusCode}", Truncate(responseBody, 500));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Webhook delivery error: {Url}", subscription.Url);
            var retryable = ex is not OperationCanceledException;
            return new PublishResult(false, retryable, "webhook_error", Truncate(ex.Message, 500));
        }
    }

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength] + "...";

    private static async Task AuditDeliveryAsync(
        IAuditLogRepository auditRepo,
        Domain.Entities.OutboxEvent evt,
        string publisherName,
        PublishResult result,
        CancellationToken ct)
    {
        await auditRepo.AddAsync(new Domain.Entities.AuditLog
        {
            TenantId = evt.TenantId,
            Action = result.Succeeded
                ? $"outbox.{evt.EventType}.published_to_{publisherName}"
                : $"outbox.{evt.EventType}.publish_failed_to_{publisherName}",
            EntityType = "OutboxEvent",
            EntityId = evt.Id.ToString(),
            Reason = result.Succeeded ? null : $"{result.ErrorCode}: {result.ErrorMessage}",
            CorrelationId = ExtractHeaderValue(evt.Headers, "correlation_id")
        }, ct);
    }

    private TimeSpan ComputeBackoff(int retryCount)
    {
        var baseDelay = TimeSpan.FromSeconds(_options.RetryBaseDelaySeconds);
        var maxDelay = TimeSpan.FromSeconds(_options.RetryMaxDelaySeconds);
        var delayTicks = baseDelay.Ticks * (long)Math.Pow(2, retryCount - 1);
        // Add ±25% random jitter to prevent thundering herd
        var jitter = (long)(delayTicks * 0.25 * (Random.Shared.NextDouble() * 2 - 1));
        var finalDelay = TimeSpan.FromTicks(delayTicks + jitter);
        return finalDelay > maxDelay ? maxDelay : finalDelay;
    }

    /// <summary>
    /// Checks if event routing rules allow this publisher to receive this event.
    /// If no routing rules exist, allow all publishers (default behavior).
    /// If rules exist, only publishers matching a rule are allowed.
    /// </summary>
    private static async Task<bool> IsPublisherRoutedAsync(
        Infrastructure.Persistence.PaymentDbContext ctx,
        string eventType, Guid? tenantId, string? applicationCode,
        string target, CancellationToken ct)
    {
        // Check if any routing rules exist for this event
        var hasAnyRules = await ctx.EventRoutingRules
            .AnyAsync(r => r.Enabled && r.DeletedAt == null, ct);

        if (!hasAnyRules)
            return true; // No rules configured → allow all publishers

        // Check for a matching rule: event_type and target match, tenant/app optional
        var matchingRule = await ctx.EventRoutingRules
            .AnyAsync(r => r.Enabled && r.DeletedAt == null
                && r.EventType == eventType
                && r.Target == target
                && (r.TenantId == null || r.TenantId == tenantId)
                && (r.ApplicationCode == null || r.ApplicationCode == applicationCode), ct);

        return matchingRule;
    }

    private static string? ExtractHeaderValue(string? headers, string key)
    {
        if (string.IsNullOrEmpty(headers)) return null;
        try
        {
            var doc = JsonDocument.Parse(headers);
            return doc.RootElement.TryGetProperty(key, out var val) ? val.GetString() : null;
        }
        catch { return null; }
    }
}

public class OutboxDispatcherOptions
{
    public bool DispatcherEnabled { get; set; } = true;
    public int BatchSize { get; set; } = 50;
    public int PollIntervalSeconds { get; set; } = 5;
    public int LockDurationSeconds { get; set; } = 60;
    public int RetryBaseDelaySeconds { get; set; } = 10;
    public int RetryMaxDelaySeconds { get; set; } = 3600;
}
