namespace Application.Features.Outbox;

/// <summary>
/// Integration event envelope sent to external consumers (webhooks, notifier, message bus).
/// </summary>
public sealed record IntegrationEventEnvelope(
    string Id,
    string Type,
    int Version,
    Guid? TenantId,
    string? ApplicationCode,
    DateTime OccurredAt,
    string AggregateType,
    string AggregateId,
    string? CorrelationId,
    string? RequestId,
    string Payload);

/// <summary>
/// Publisher result for a single event delivery attempt.
/// </summary>
public sealed record PublishResult(
    bool Succeeded,
    bool Retryable,
    string? ErrorCode,
    string? ErrorMessage);

/// <summary>
/// Outbox event message passed to publishers for processing.
/// </summary>
public sealed record OutboxEventMessage(
    Guid EventId,
    string EventType,
    int EventVersion,
    Guid? TenantId,
    string? ApplicationCode,
    string AggregateType,
    string AggregateId,
    string Payload,
    string? Headers,
    DateTime OccurredAt);

/// <summary>
/// Contract for publishing integration events to external systems.
/// Each publisher handles one delivery channel (webhook, notifier, message bus).
/// </summary>
public interface IIntegrationEventPublisher
{
    string PublisherName { get; }
    Task<PublishResult> PublishAsync(OutboxEventMessage message, CancellationToken ct);
}
