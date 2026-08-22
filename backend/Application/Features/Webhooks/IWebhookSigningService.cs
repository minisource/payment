namespace Application.Features.Webhooks;

/// <summary>
/// Handles HMAC-SHA256 signing for webhook payloads.
/// Signature format: v1=hex(HMAC-SHA256(secret, timestamp + "." + rawBody))
/// </summary>
public interface IWebhookSigningService
{
    string ComputeSignature(string secret, long timestamp, string rawBody);
    bool VerifySignature(string secret, long timestamp, string rawBody, string signature);
    (string plainSecret, string encryptedSecret, string secretHash) GenerateSecret();
    string DecryptSecret(string encryptedSecret);
}
