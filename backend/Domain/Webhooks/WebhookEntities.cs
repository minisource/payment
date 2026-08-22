using Minisource.Common.Domain;

namespace Domain.Entities;

public class WebhookSubscription : Entity<Guid>
{
    public Guid TenantId { get; set; }
    public string? ApplicationCode { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? SecretHash { get; set; }
    public string? EncryptedSecret { get; set; }
    public string Status { get; set; } = "active";
    public string[] EventTypes { get; set; } = Array.Empty<string>();
    public int MaxRetryCount { get; set; } = 10;
    public int TimeoutSeconds { get; set; } = 10;
    public string? Headers { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public DateTime? DisabledAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? Metadata { get; set; }

    public WebhookSubscription()
    {
        Id = Guid.NewGuid();
    }
}

public class WebhookDelivery : Entity<Guid>
{
    public Guid TenantId { get; set; }
    public string? ApplicationCode { get; set; }
    public Guid OutboxEventId { get; set; }
    public Guid WebhookSubscriptionId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public int EventVersion { get; set; } = 1;
    public string Status { get; set; } = "pending";
    public int AttemptCount { get; set; }
    public int MaxAttempts { get; set; } = 10;
    public DateTime? NextAttemptAt { get; set; }
    public DateTime? LastAttemptAt { get; set; }
    public int? LastStatusCode { get; set; }
    public string? LastErrorMessage { get; set; }
    public string? ResponseBodySafe { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? FailedAt { get; set; }
    public string? Metadata { get; set; }

    public WebhookDelivery()
    {
        Id = Guid.NewGuid();
    }
}

public class EventRoutingRule : Entity<Guid>
{
    public Guid? TenantId { get; set; }
    public string? ApplicationCode { get; set; }
    public string EventType { get; set; } = string.Empty;
    public int? EventVersion { get; set; }
    public string Target { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
    public string? Filter { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? Metadata { get; set; }

    public EventRoutingRule()
    {
        Id = Guid.NewGuid();
    }
}
