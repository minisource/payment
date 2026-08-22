using Minisource.Common.Domain;

namespace Domain.Entities;

/// <summary>
/// Represents a payment audit log entry.
/// </summary>
public class PaymentLog : Entity<Guid>
{
    /// <summary>
    /// Payment ID this log belongs to.
    /// </summary>
    public Guid PaymentId { get; private set; }

    /// <summary>
    /// Action performed (e.g., "Created", "Processing", "Completed").
    /// </summary>
    public string Action { get; private set; } = string.Empty;

    /// <summary>
    /// Details of the action as JSON or text.
    /// </summary>
    public string Details { get; private set; } = string.Empty;

    /// <summary>
    /// Log entry timestamp.
    /// </summary>
    public DateTime Timestamp { get; private set; }

    /// <summary>
    /// User or system that performed the action.
    /// </summary>
    public string? Actor { get; private set; }

    /// <summary>
    /// IP address if available.
    /// </summary>
    public string? IpAddress { get; private set; }

    // Navigation
    public Payment Payment { get; private set; } = null!;

    // EF Core constructor
    private PaymentLog() { }

    /// <summary>
    /// Creates a new payment log entry.
    /// </summary>
    internal static PaymentLog Create(
        Guid paymentId,
        string action,
        string details,
        string? actor = null,
        string? ipAddress = null)
    {
        return new PaymentLog
        {
            Id = Guid.NewGuid(),
            PaymentId = paymentId,
            Action = action,
            Details = details,
            Timestamp = DateTime.UtcNow,
            Actor = actor,
            IpAddress = ipAddress
        };
    }
}