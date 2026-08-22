namespace Domain.Gateways;

/// <summary>
/// Abstraction for refund (reverse payment) operations through a gateway.
/// Adapters that support refund implement this interface.
/// </summary>
public interface IRefundGatewayAdapter
{
    Task<RefundGatewayResult> RefundAsync(RefundGatewayRequest request, CancellationToken cancellationToken);
}

public sealed record RefundGatewayRequest(
    Guid RefundRequestId,
    Guid PaymentTransactionId,
    Guid? GatewayConfigId,
    string ProviderCode,
    string GatewayName,
    string AdapterType,
    string ConfigJson,
    string? EncryptedSecrets,
    string Amount,
    string Currency,
    string OriginalGatewayReferenceId,
    string OriginalTrackingCode,
    Guid TenantId,
    bool IsPartial,
    IReadOnlyDictionary<string, object?>? Metadata = null);

public sealed record RefundGatewayResult(
    bool IsSuccess,
    string? GatewayRefundId = null,
    string? GatewayTrackingCode = null,
    string? GatewayStatus = null,
    bool IsPending = false,
    bool RequiresManualReview = false,
    string? ErrorCode = null,
    string? ErrorMessage = null,
    object? SafeResponse = null);
