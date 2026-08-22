using System.Text.Json;
using Domain.Gateways;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Gateways;

/// <summary>
/// Gateway secret protector using ASP.NET Core Data Protection API.
/// Encrypts/decrypts sensitive gateway credentials based on provider schema.
/// </summary>
public class GatewaySecretProtector : IGatewaySecretProtector
{
    private readonly IDataProtector _protector;
    private readonly ILogger<GatewaySecretProtector> _logger;
    private const string Purpose = "GatewaySecrets";

    public GatewaySecretProtector(IDataProtectionProvider provider, ILogger<GatewaySecretProtector> logger)
    {
        _protector = provider.CreateProtector(Purpose);
        _logger = logger;
    }

    public Task<string?> EncryptSecretsAsync(string providerCode, string configJson, string providerSchema, CancellationToken ct)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(configJson) || configJson == "{}")
                return Task.FromResult<string?>(null);

            var config = JsonSerializer.Deserialize<JsonElement>(configJson);
            var schema = JsonSerializer.Deserialize<JsonElement>(providerSchema);

            var secrets = new Dictionary<string, string?>();

            if (schema.ValueKind == JsonValueKind.Object)
            {
                foreach (var prop in schema.EnumerateObject())
                {
                    if (prop.Value.ValueKind == JsonValueKind.Object)
                    {
                        var isSecret = false;
                        if (prop.Value.TryGetProperty("secret", out var secretVal) && secretVal.ValueKind == JsonValueKind.True)
                            isSecret = true;
                        if (prop.Value.TryGetProperty("required", out var requiredVal) && requiredVal.ValueKind == JsonValueKind.False)
                            _ = requiredVal;

                        if (isSecret && config.ValueKind == JsonValueKind.Object &&
                            config.TryGetProperty(prop.Name, out var configVal))
                        {
                            secrets[prop.Name] = configVal.ToString();
                        }
                    }
                }
            }

            if (!secrets.Any())
                return Task.FromResult<string?>(null);

            var json = JsonSerializer.Serialize(secrets);
            var encrypted = _protector.Protect(json);
            return Task.FromResult<string?>(encrypted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to encrypt gateway secrets for provider {ProviderCode}", providerCode);
            throw;
        }
    }

    public Task<string?> DecryptSecretsAsync(string encryptedSecrets, CancellationToken ct)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(encryptedSecrets))
                return Task.FromResult<string?>(null);

            var decrypted = _protector.Unprotect(encryptedSecrets);
            return Task.FromResult<string?>(decrypted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to decrypt gateway secrets");
            throw new InvalidOperationException("Failed to decrypt gateway secrets", ex);
        }
    }

    public string RedactSecrets(string configJson, string? encryptedSecrets, string providerSchema)
    {
        try
        {
            var config = JsonSerializer.Deserialize<JsonElement>(configJson);
            var schema = JsonSerializer.Deserialize<JsonElement>(providerSchema);

            if (schema.ValueKind != JsonValueKind.Object || config.ValueKind != JsonValueKind.Object)
                return configJson;

            var redacted = JsonSerializer.Deserialize<Dictionary<string, object>>(configJson) ?? [];

            foreach (var prop in schema.EnumerateObject())
            {
                if (prop.Value.ValueKind == JsonValueKind.Object &&
                    prop.Value.TryGetProperty("secret", out var secretVal) &&
                    secretVal.ValueKind == JsonValueKind.True &&
                    redacted.ContainsKey(prop.Name))
                {
                    redacted[prop.Name] = "********";
                }
            }

            return JsonSerializer.Serialize(redacted);
        }
        catch
        {
            return configJson;
        }
    }
}
