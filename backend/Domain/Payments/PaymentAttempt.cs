using Domain.Enums;
using Minisource.Common.Domain;

namespace Domain.Entities;

/// <summary>
/// Represents a payment gateway attempt.
/// </summary>
public class PaymentAttempt : Entity<Guid>
{
    /// <summary>
    /// Payment ID this attempt belongs to.
    /// </summary>
    public Guid PaymentId { get; private set; }

    /// <summary>
    /// Attempt number (1-based).
    /// </summary>
    public int AttemptNumber { get; private set; }

    /// <summary>
    /// Status of this attempt.
    /// </summary>
    public PaymentStatus Status { get; private set; }

    /// <summary>
    /// Gateway response as JSON.
    /// </summary>
    public string ProviderResponse { get; private set; } = string.Empty;

    /// <summary>
    /// Gateway-specific error code if failed.
    /// </summary>
    public string? ErrorCode { get; private set; }

    /// <summary>
    /// Error message if failed.
    /// </summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>
    /// Attempt completion timestamp.
    /// </summary>
    public DateTime? CompletedAt { get; private set; }

    // Navigation
    public Payment Payment { get; private set; } = null!;

    // EF Core constructor
    private PaymentAttempt() { }

    /// <summary>
    /// Creates a new payment attempt.
    /// </summary>
    internal static PaymentAttempt Create(
        Guid paymentId,
        int attemptNumber,
        PaymentStatus status,
        string providerResponse)
    {
        return new PaymentAttempt
        {
            Id = Guid.NewGuid(),
            PaymentId = paymentId,
            AttemptNumber = attemptNumber,
            Status = status,
            ProviderResponse = providerResponse
        };
    }

    /// <summary>
    /// Marks the attempt as completed.
    /// </summary>
    internal void MarkCompleted(PaymentStatus status, string providerResponse)
    {
        Status = status;
        ProviderResponse = providerResponse;
        CompletedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the attempt as failed.
    /// </summary>
    internal void MarkFailed(string errorCode, string errorMessage, string providerResponse)
    {
        Status = PaymentStatus.Failed;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
        ProviderResponse = providerResponse;
        CompletedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Checks if this attempt was successful.
    /// </summary>
    public bool IsSuccessful => Status == PaymentStatus.Completed;
}