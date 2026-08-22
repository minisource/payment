using Domain.Enums;
using Minisource.Common.Domain;

namespace Domain.Entities;

/// <summary>
/// Append-only record of a single refund gateway attempt.
/// Stores safe request/response payload (secrets redacted) for audit and debugging.
/// </summary>
public sealed class RefundGatewayAttempt : Entity<Guid>
{
    public Guid RefundRequestId { get; private set; }
    public Guid? GatewayConfigId { get; private set; }
    public string ProviderCode { get; private set; } = string.Empty;
    public string GatewayName { get; private set; } = string.Empty;

    public int AttemptNo { get; private set; }
    public RefundGatewayAttemptStatus Status { get; private set; } = RefundGatewayAttemptStatus.Pending;

    // Safe payloads — secrets redacted before storage
    public string? RequestPayloadSafe { get; private set; }
    public string? ResponsePayloadSafe { get; private set; }

    // Gateway result
    public string? GatewayRefundId { get; private set; }
    public string? GatewayTrackingCode { get; private set; }
    public string? GatewayStatus { get; private set; }
    public int? HttpStatusCode { get; private set; }

    // Error
    public string? ErrorCode { get; private set; }
    public string? ErrorMessage { get; private set; }

    public DateTime? StartedAt { get; private set; }
    public DateTime? FinishedAt { get; private set; }

    // Navigation
    public RefundRequest RefundRequest { get; private set; } = null!;

    private RefundGatewayAttempt() { }

    internal static RefundGatewayAttempt Create(
        Guid refundRequestId,
        int attemptNo,
        string providerCode,
        string gatewayName,
        string? requestPayloadSafe = null)
    {
        return new RefundGatewayAttempt
        {
            Id = Guid.NewGuid(),
            RefundRequestId = refundRequestId,
            AttemptNo = attemptNo,
            ProviderCode = providerCode,
            GatewayName = gatewayName,
            RequestPayloadSafe = requestPayloadSafe,
            Status = RefundGatewayAttemptStatus.Pending,
            StartedAt = DateTime.UtcNow
        };
    }

    public void MarkSubmitted(string? requestPayloadSafe = null)
    {
        Status = RefundGatewayAttemptStatus.Submitted;
        if (requestPayloadSafe != null)
            RequestPayloadSafe = requestPayloadSafe;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkSucceeded(
        string? gatewayRefundId = null,
        string? gatewayTrackingCode = null,
        string? gatewayStatus = null,
        string? responsePayloadSafe = null)
    {
        Status = RefundGatewayAttemptStatus.Succeeded;
        GatewayRefundId = gatewayRefundId;
        GatewayTrackingCode = gatewayTrackingCode;
        GatewayStatus = gatewayStatus;
        ResponsePayloadSafe = responsePayloadSafe;
        FinishedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkFailed(
        string errorCode,
        string errorMessage,
        int? httpStatusCode = null,
        string? responsePayloadSafe = null)
    {
        Status = RefundGatewayAttemptStatus.Failed;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
        HttpStatusCode = httpStatusCode;
        ResponsePayloadSafe = responsePayloadSafe;
        FinishedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkTimeout()
    {
        Status = RefundGatewayAttemptStatus.Timeout;
        FinishedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsTerminal => Status is RefundGatewayAttemptStatus.Succeeded
        or RefundGatewayAttemptStatus.Failed or RefundGatewayAttemptStatus.Timeout;
}
