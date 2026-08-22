using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Domain.Gateways;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Gateways.CustomGateways.AzkiVam;

/// <summary>
/// AzkiVam custom gateway adapter implementing IPaymentGatewayAdapter and IRefundGatewayAdapter.
/// Handles payment start (create ticket), verify, and refund (reverse/partial reverse).
/// Auth: HMAC-SHA256 signature header + MerchantId header (per-request).
/// </summary>
public class AzkiVamGatewayAdapter : IPaymentGatewayAdapter, IRefundGatewayAdapter
{
    private readonly HttpClient _httpClient;
    private readonly IGatewaySecretProtector _secretProtector;
    private readonly ILogger<AzkiVamGatewayAdapter> _logger;

    public AzkiVamGatewayAdapter(
        HttpClient httpClient,
        IGatewaySecretProtector secretProtector,
        ILogger<AzkiVamGatewayAdapter> logger)
    {
        _httpClient = httpClient;
        _secretProtector = secretProtector;
        _logger = logger;
    }

    public async Task<GatewayStartResult> StartAsync(GatewayStartRequest request, CancellationToken ct)
    {
        _logger.LogInformation("AzkiVam starting payment for amount {Amount} {Currency}", request.Amount, request.Currency);

        try
        {
            var config = await ResolveConfigAsync(request.ConfigJson, request.EncryptedSecrets, ct);

            var createRequest = new
            {
                amount = (int)request.Amount,
                redirect_uri = request.CallbackUrl,
                fallback_uri = config.FallbackUrl ?? request.CallbackUrl,
                provider_id = config.ProviderId,
                mobile_number = "",
                merchant_id = config.MerchantId,
                items = new[]
                {
                    new
                    {
                        name = request.Description ?? $"Payment {request.PaymentIntentId}",
                        count = 1,
                        amount = (int)request.Amount,
                        url = "https://payment.local"
                    }
                }
            };

            var response = await SendAzkiVamRequestAsync<AzkiVamCreateTicketResponse>(
                "/payment/purchase", createRequest, config.ApiKey, config.MerchantId, ct);

            if (response?.PaymentUri is null)
            {
                _logger.LogWarning("AzkiVam create ticket returned null payment URI");
                return new GatewayStartResult(false, ErrorCode: "azkivam_no_payment_uri",
                    ErrorMessage: "Gateway returned no payment URI");
            }

            _logger.LogInformation("AzkiVam ticket created: {TicketId}", response.TicketId);

            return new GatewayStartResult(
                true,
                RedirectUrl: response.PaymentUri,
                Authority: response.TicketId,
                GatewayReferenceId: $"azkivam-{response.TicketId}");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "AzkiVam HTTP error during start");
            return new GatewayStartResult(false, ErrorCode: "azkivam_http_error", ErrorMessage: ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AzkiVam unexpected error during start");
            return new GatewayStartResult(false, ErrorCode: "azkivam_error", ErrorMessage: ex.Message);
        }
    }

    public async Task<GatewayVerifyResult> VerifyAsync(GatewayVerifyRequest request, CancellationToken ct)
    {
        _logger.LogInformation("AzkiVam verifying payment for authority {Authority}", request.Authority);

        try
        {
            var config = await ResolveConfigAsync(request.ConfigJson, request.EncryptedSecrets, ct);

            var verifyRequest = new { ticket_id = request.Authority };

            var response = await SendAzkiVamRequestAsync<AzkiVamVerifyTicketResponse>(
                "/payment/verify", verifyRequest, config.ApiKey, config.MerchantId, ct);

            if (response?.TicketId is null)
            {
                _logger.LogWarning("AzkiVam verify returned null ticket ID");
                return new GatewayVerifyResult(false, ErrorCode: "azkivam_verify_failed",
                    ErrorMessage: "Gateway verification returned no ticket ID");
            }

            return new GatewayVerifyResult(
                true,
                GatewayReferenceId: response.TicketId,
                VerifiedAmount: request.Amount,
                ResponseCode: "0");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "AzkiVam HTTP error during verify");
            return new GatewayVerifyResult(false, ErrorCode: "azkivam_http_error", ErrorMessage: ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AzkiVam unexpected error during verify");
            return new GatewayVerifyResult(false, ErrorCode: "azkivam_error", ErrorMessage: ex.Message);
        }
    }

    public async Task<RefundGatewayResult> RefundAsync(RefundGatewayRequest request, CancellationToken ct)
    {
        _logger.LogInformation("AzkiVam processing refund for ticket {TicketId}", request.OriginalGatewayReferenceId);

        try
        {
            var config = await ResolveConfigAsync(request.ConfigJson, request.EncryptedSecrets, ct);
            if (!int.TryParse(request.Amount, out int refundAmount) || refundAmount <= 0)
                return new RefundGatewayResult(false, ErrorCode: "azkivam_invalid_amount",
                    ErrorMessage: $"Invalid refund amount: {request.Amount}");

            if (request.IsPartial)
            {
                var partialRequest = new
                {
                    ticket_id = request.OriginalGatewayReferenceId,
                    reverse_amount = refundAmount
                };

                var partialResponse = await SendAzkiVamRequestAsync<AzkiVamReverseTicketResponse>(
                    "/payment/partial-reverse-by-amount", partialRequest, config.ApiKey, config.MerchantId, ct);

                return new RefundGatewayResult(
                    IsSuccess: true,
                    GatewayRefundId: partialResponse?.TicketId,
                    SafeResponse: partialResponse);
            }
            else
            {
                var reverseRequest = new
                {
                    ticket_id = request.OriginalGatewayReferenceId,
                    provider_id = config.ProviderId,
                    reason = "regret"
                };

                var reverseResponse = await SendAzkiVamRequestAsync<AzkiVamReverseTicketResponse>(
                    "/payment/reverse", reverseRequest, config.ApiKey, config.MerchantId, ct);

                return new RefundGatewayResult(
                    IsSuccess: true,
                    GatewayRefundId: reverseResponse?.TicketId,
                    SafeResponse: reverseResponse);
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "AzkiVam HTTP error during refund");
            return new RefundGatewayResult(false, ErrorCode: "azkivam_refund_http_error", ErrorMessage: ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AzkiVam unexpected error during refund");
            return new RefundGatewayResult(false, ErrorCode: "azkivam_refund_error", ErrorMessage: ex.Message);
        }
    }

    private async Task<AzkiVamConfig> ResolveConfigAsync(
        string configJson, string? encryptedSecrets, CancellationToken ct)
    {
        var config = JsonSerializer.Deserialize<AzkiVamConfig>(configJson)
            ?? throw new InvalidOperationException("Invalid AzkiVam gateway config");

        if (!string.IsNullOrWhiteSpace(encryptedSecrets))
        {
            var secretsJson = await _secretProtector.DecryptSecretsAsync(encryptedSecrets, ct);
            if (secretsJson is not null)
            {
                var secrets = JsonSerializer.Deserialize<AzkiVamSecrets>(secretsJson);
                if (secrets is not null)
                {
                    config.ApiKey = secrets.ApiKey ?? config.ApiKey;
                    config.MerchantId = secrets.MerchantId ?? config.MerchantId;
                }
            }
        }

        if (string.IsNullOrWhiteSpace(config.ApiKey))
            throw new InvalidOperationException("AzkiVam API key is not configured");
        if (string.IsNullOrWhiteSpace(config.MerchantId))
            throw new InvalidOperationException("AzkiVam Merchant ID is not configured");

        // Ensure BaseAddress is set
        if (_httpClient.BaseAddress is null && !string.IsNullOrWhiteSpace(config.BaseUrl))
            _httpClient.BaseAddress = new Uri(config.BaseUrl.TrimEnd('/'));

        return config;
    }

    private async Task<TResponse?> SendAzkiVamRequestAsync<TResponse>(
        string route, object payload, string apiKey, string merchantId, CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });

        var signature = CreateSignature(route, apiKey);

        using var request = new HttpRequestMessage(HttpMethod.Post, route);
        request.Headers.Add("Signature", signature);
        request.Headers.Add("MerchantId", merchantId);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(ct);
        var wrapper = JsonSerializer.Deserialize<AzkiVamSuccessResponse<TResponse>>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        });
        return wrapper is not null ? wrapper.Result : default;
    }

    /// <summary>Creates AES-CBC signature for AzkiVam API.</summary>
    internal static string CreateSignature(string route, string apiKey)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        var plainSignature = $"{route}#{timestamp}#POST#{apiKey}";

        try
        {
            var keyBytes = HexStringToByteArray(apiKey);
            using var aes = Aes.Create();
            aes.Key = keyBytes;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.IV = new byte[16];

            using var encryptor = aes.CreateEncryptor();
            var plainBytes = Encoding.UTF8.GetBytes(plainSignature);
            var encrypted = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
            return ByteArrayToHexString(encrypted);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"AzkiVam signature generation failed: {ex.Message}", ex);
        }
    }

    private static byte[] HexStringToByteArray(string hex)
    {
        var bytes = new byte[hex.Length / 2];
        for (var i = 0; i < hex.Length; i += 2)
            bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
        return bytes;
    }

    private static string ByteArrayToHexString(byte[] bytes) =>
        BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
}

// ─── Config DTOs ───────────────────────────────────────────────

internal sealed class AzkiVamConfig
{
    public string MerchantId { get; set; } = string.Empty;
    public long ProviderId { get; set; }
    public string? FallbackUrl { get; set; }
    public string? RedirectUrl { get; set; }
    public string? BaseUrl { get; set; }
    public string ApiKey { get; set; } = string.Empty;
}

internal sealed class AzkiVamSecrets
{
    [JsonPropertyName("api_key")]
    public string? ApiKey { get; set; }
    [JsonPropertyName("merchant_id")]
    public string? MerchantId { get; set; }
}

internal sealed class AzkiVamSuccessResponse<T>
{
    [JsonPropertyName("rs_code")]
    public int RsCode { get; set; }
    [JsonPropertyName("result")]
    public T? Result { get; set; }
}

internal sealed class AzkiVamCreateTicketResponse
{
    [JsonPropertyName("payment_uri")]
    public string? PaymentUri { get; set; }
    [JsonPropertyName("ticket_id")]
    public string? TicketId { get; set; }
}

internal sealed class AzkiVamVerifyTicketResponse
{
    [JsonPropertyName("ticket_id")]
    public string? TicketId { get; set; }
}

internal sealed class AzkiVamReverseTicketResponse
{
    [JsonPropertyName("ticket_id")]
    public string? TicketId { get; set; }
}
