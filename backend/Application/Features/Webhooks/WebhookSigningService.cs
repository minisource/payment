using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;

namespace Application.Features.Webhooks;

/// <summary>
/// Handles HMAC-SHA256 signing for webhook payloads.
/// Signature format: v1=hex(HMAC-SHA256(secret, timestamp + "." + rawBody))
/// </summary>
public class WebhookSigningService : IWebhookSigningService
{
    private readonly IDataProtector _protector;
    private readonly ILogger<WebhookSigningService> _logger;

    public WebhookSigningService(IDataProtectionProvider protectionProvider, ILogger<WebhookSigningService> logger)
    {
        _protector = protectionProvider.CreateProtector("MiniSource.Payment.WebhookSecrets");
        _logger = logger;
    }

    public string ComputeSignature(string secret, long timestamp, string rawBody)
    {
        var payload = $"{timestamp}.{rawBody}";
        var key = Encoding.UTF8.GetBytes(secret);
        var data = Encoding.UTF8.GetBytes(payload);

        using var hmac = new HMACSHA256(key);
        var hash = hmac.ComputeHash(data);
        return $"v1={Convert.ToHexStringLower(hash)}";
    }

    public bool VerifySignature(string secret, long timestamp, string rawBody, string signature)
    {
        try
        {
            var expected = ComputeSignature(secret, timestamp, rawBody);
            var result = CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(expected),
                Encoding.UTF8.GetBytes(signature));
            if (!result)
                _logger.LogWarning("Webhook signature verification failed");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Webhook signature verification error");
            return false;
        }
    }

    public (string plainSecret, string encryptedSecret, string secretHash) GenerateSecret()
    {
        var secretBytes = RandomNumberGenerator.GetBytes(32);
        var plainSecret = $"whsec_{Convert.ToHexStringLower(secretBytes)}";

        // Hash using SHA256 for storage/quick verification
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(plainSecret));
        var secretHash = Convert.ToHexStringLower(hashBytes);

        // Encrypt using ASP.NET Data Protection for at-rest security
        var encryptedSecret = _protector.Protect(plainSecret);

        return (plainSecret, encryptedSecret, secretHash);
    }

    public string DecryptSecret(string encryptedSecret)
    {
        try { return _protector.Unprotect(encryptedSecret); }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to decrypt webhook secret");
            return string.Empty;
        }
    }
}
