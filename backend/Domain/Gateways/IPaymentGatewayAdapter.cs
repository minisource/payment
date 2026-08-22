namespace Domain.Gateways;

/// <summary>
/// Abstraction for payment gateway operations. Core/application layers
/// depend only on this interface, not on Parbad or specific providers.
/// </summary>
public interface IPaymentGatewayAdapter
{
    Task<GatewayStartResult> StartAsync(GatewayStartRequest request, CancellationToken cancellationToken);
    Task<GatewayVerifyResult> VerifyAsync(GatewayVerifyRequest request, CancellationToken cancellationToken);
}

public sealed record GatewayStartRequest(
    string ProviderCode,
    string AdapterType,
    string ConfigJson,
    string? EncryptedSecrets,
    decimal Amount,
    string Currency,
    Guid PaymentTransactionId,
    Guid PaymentIntentId,
    Guid TenantId,
    string CallbackUrl,
    string? Description = null,
    string? Metadata = null);

public sealed record GatewayStartResult(
    bool Success,
    string? RedirectUrl = null,
    string? Authority = null,
    string? GatewayReferenceId = null,
    string? ErrorCode = null,
    string? ErrorMessage = null);

public sealed record GatewayVerifyRequest(
    string ProviderCode,
    string AdapterType,
    string ConfigJson,
    string? EncryptedSecrets,
    string Authority,
    decimal Amount,
    string Currency,
    Guid PaymentTransactionId,
    Guid PaymentIntentId,
    Guid TenantId,
    string? CallbackParams = null);

public sealed record GatewayVerifyResult(
    bool Success,
    string? GatewayReferenceId = null,
    string? TraceNumber = null,
    string? Rrn = null,
    string? CardPanMasked = null,
    string? ResponseCode = null,
    string? ResponseMessage = null,
    string? ErrorCode = null,
    string? ErrorMessage = null,
    decimal? VerifiedAmount = null);
