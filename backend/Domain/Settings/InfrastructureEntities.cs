using Minisource.Common.Domain;

namespace Domain.Entities;

public class IdempotencyRecord : Entity<Guid>
{
    public Guid TenantId { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
    public string RequestHash { get; set; } = string.Empty;
    public string Operation { get; set; } = string.Empty;
    public string Status { get; set; } = "processing";
    public int? ResponseStatusCode { get; set; }
    public string? ResponseBody { get; set; }
    public string? ResourceType { get; set; }
    public string? ResourceId { get; set; }
    public DateTime? LockedUntil { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public string? CreatedByClientId { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }

    public IdempotencyRecord()
    {
        Id = Guid.NewGuid();
    }
}

public class AuditLog : Entity<Guid>
{
    public Guid? TenantId { get; set; }
    public Guid? ActorUserId { get; set; }
    public string? ActorClientId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string? BeforeSnapshot { get; set; }
    public string? AfterSnapshot { get; set; }
    public string? Reason { get; set; }
    public string? RequestId { get; set; }
    public string? CorrelationId { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Metadata { get; set; }

    public string? PreviousAuditHash { get; set; }
    public string? AuditHash { get; set; }
    public string HashAlgorithm { get; set; } = "SHA256";
    public int HashVersion { get; set; } = 1;

    public AuditLog()
    {
        Id = Guid.NewGuid();
    }
}

public class OutboxEvent : Entity<Guid>
{
    public Guid? TenantId { get; set; }
    public string? ApplicationCode { get; set; }
    public string EventType { get; set; } = string.Empty;
    public int EventVersion { get; set; } = 1;
    public string AggregateType { get; set; } = string.Empty;
    public string AggregateId { get; set; } = string.Empty;
    public string Payload { get; set; } = "{}";
    public string? Headers { get; set; }
    public string Status { get; set; } = "pending";
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public DateTime AvailableAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
    public DateTime? LockedUntil { get; set; }
    public string? LockedBy { get; set; }
    public int RetryCount { get; set; }
    public int MaxRetryCount { get; set; } = 10;
    public DateTime? NextRetryAt { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? LastErrorAt { get; set; }
    public DateTime? DeadLetteredAt { get; set; }
    public string? Metadata { get; set; }

    public OutboxEvent()
    {
        Id = Guid.NewGuid();
    }
}
