using Domain.Enums;
using Minisource.Common.Domain;

namespace Domain.Entities;

/// <summary>
/// Gateway payment transaction linked to a PaymentIntent and GatewayConfig.
/// Represents a single attempt through a specific gateway.
/// </summary>
public class PaymentTransaction : Entity<Guid>
{
    public Guid TenantId { get; private set; }
    public string? ApplicationCode { get; private set; }

    public Guid PaymentIntentId { get; private set; }
    public Guid GatewayConfigId { get; private set; }

    public string ProviderCode { get; private set; } = string.Empty;
    public string AdapterType { get; private set; } = "parbad";

    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = string.Empty;

    public PaymentTransactionStatus Status { get; private set; } = PaymentTransactionStatus.Created;

    public string? Authority { get; private set; }
    public string? GatewayReferenceId { get; private set; }
    public string? TraceNumber { get; private set; }
    public string? Rrn { get; private set; }
    public string? CardPanMasked { get; private set; }

    public string? GatewayResponseCode { get; private set; }
    public string? GatewayResponseMessage { get; private set; }

    public DateTime? CallbackReceivedAt { get; private set; }
    public DateTime? VerifiedAt { get; private set; }
    public DateTime? FailedAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public DateTime? ExpiredAt { get; private set; }

    public string? CallbackPayloadSafe { get; private set; }
    public string? VerifyPayloadSafe { get; private set; }

    public string? IdempotencyKey { get; private set; }
    public string? Metadata { get; private set; }

    public PaymentIntent PaymentIntent { get; private set; } = null!;
    public GatewayConfig GatewayConfig { get; private set; } = null!;

    private PaymentTransaction() { }

    public static PaymentTransaction Create(
        Guid tenantId,
        Guid paymentIntentId,
        Guid gatewayConfigId,
        string providerCode,
        string adapterType,
        decimal amount,
        string currency,
        string? applicationCode = null,
        string? idempotencyKey = null,
        string? metadata = null)
    {
        return new PaymentTransaction
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ApplicationCode = applicationCode,
            PaymentIntentId = paymentIntentId,
            GatewayConfigId = gatewayConfigId,
            ProviderCode = providerCode,
            AdapterType = adapterType,
            Amount = amount,
            Currency = currency.ToUpperInvariant(),
            IdempotencyKey = idempotencyKey,
            Metadata = metadata,
            Status = PaymentTransactionStatus.Created
        };
    }

    public void MarkSentToGateway(string? authority)
    {
        Status = PaymentTransactionStatus.SentToGateway;
        Authority = authority;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkCallbackReceived(string? safePayload, string? gatewayReferenceId = null, string? traceNumber = null)
    {
        Status = PaymentTransactionStatus.CallbackReceived;
        CallbackReceivedAt = DateTime.UtcNow;
        CallbackPayloadSafe = safePayload;
        GatewayReferenceId = gatewayReferenceId ?? GatewayReferenceId;
        TraceNumber = traceNumber ?? TraceNumber;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkVerifyPending()
    {
        Status = PaymentTransactionStatus.VerifyPending;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkVerified(string? gatewayReferenceId = null, string? rrn = null, string? cardPanMasked = null, string? verifyPayloadSafe = null)
    {
        if (Status == PaymentTransactionStatus.Verified)
            return; // Idempotent
        Status = PaymentTransactionStatus.Verified;
        VerifiedAt = DateTime.UtcNow;
        GatewayReferenceId = gatewayReferenceId ?? GatewayReferenceId;
        Rrn = rrn ?? Rrn;
        CardPanMasked = cardPanMasked ?? CardPanMasked;
        VerifyPayloadSafe = verifyPayloadSafe ?? VerifyPayloadSafe;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkFailed(string? responseCode = null, string? responseMessage = null)
    {
        Status = PaymentTransactionStatus.Failed;
        FailedAt = DateTime.UtcNow;
        GatewayResponseCode = responseCode ?? GatewayResponseCode;
        GatewayResponseMessage = responseMessage ?? GatewayResponseMessage;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status == PaymentTransactionStatus.Verified)
            throw new InvalidOperationException("Cannot cancel verified transaction");
        Status = PaymentTransactionStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
