using Minisource.Common.Domain;

namespace Domain.ValueObjects;

/// <summary>
/// Value object representing a unique payment reference/tracking number.
/// </summary>
public class PaymentReference : ValueObject
{
    public long TrackingNumber { get; }
    public string? TransactionReference { get; }
    public string? GatewayReference { get; }

    private PaymentReference(long trackingNumber, string? transactionReference = null, string? gatewayReference = null)
    {
        if (trackingNumber <= 0)
            throw new ArgumentException("Tracking number must be positive", nameof(trackingNumber));

        TrackingNumber = trackingNumber;
        TransactionReference = transactionReference;
        GatewayReference = gatewayReference;
    }

    /// <summary>
    /// Creates a new payment reference with tracking number.
    /// </summary>
    public static PaymentReference Create(long trackingNumber)
        => new(trackingNumber);

    /// <summary>
    /// Creates a new payment reference with all components.
    /// </summary>
    public static PaymentReference CreateWithReferences(
        long trackingNumber,
        string transactionReference,
        string gatewayReference)
        => new(trackingNumber, transactionReference, gatewayReference);

    /// <summary>
    /// Updates with gateway reference after payment initiation.
    /// </summary>
    public PaymentReference WithGatewayReference(string gatewayReference)
        => new(TrackingNumber, TransactionReference, gatewayReference);

    /// <summary>
    /// Updates with transaction reference after payment completion.
    /// </summary>
    public PaymentReference WithTransactionReference(string transactionReference)
        => new(TrackingNumber, transactionReference, GatewayReference);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return TrackingNumber;
    }

    public override string ToString() => TrackingNumber.ToString();
}
