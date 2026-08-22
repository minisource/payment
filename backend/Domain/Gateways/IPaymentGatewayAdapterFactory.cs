namespace Domain.Gateways;

/// <summary>
/// Factory for creating gateway adapters based on provider and adapter type.
/// </summary>
public interface IPaymentGatewayAdapterFactory
{
    IPaymentGatewayAdapter Create(string adapterType, string providerCode);
}

/// <summary>
/// Service for protecting gateway secrets (encryption/decryption).
/// </summary>
public interface IGatewaySecretProtector
{
    Task<string?> EncryptSecretsAsync(string providerCode, string configJson, string providerSchema, CancellationToken ct);
    Task<string?> DecryptSecretsAsync(string encryptedSecrets, CancellationToken ct);
    string RedactSecrets(string configJson, string? encryptedSecrets, string providerSchema);
}
