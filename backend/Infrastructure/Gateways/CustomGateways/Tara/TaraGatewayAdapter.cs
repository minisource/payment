using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Domain.Gateways;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Gateways.CustomGateways.Tara;

/// <summary>
/// Tara custom gateway adapter implementing IPaymentGatewayAdapter and IRefundGatewayAdapter.
/// Handles payment start (get token), verify (purchase verify), and refund.
/// Auth: Bearer JWT (login per call — TODO: add token cache for production).
/// Uses per-request headers (thread-safe).
/// </summary>
public class TaraGatewayAdapter : IPaymentGatewayAdapter, IRefundGatewayAdapter
{
    private readonly HttpClient _httpClient;
    private readonly IGatewaySecretProtector _secretProtector;
    private readonly ILogger<TaraGatewayAdapter> _logger;

    private const string SuccessCode = "0";

    public TaraGatewayAdapter(
        HttpClient httpClient,
        IGatewaySecretProtector secretProtector,
        ILogger<TaraGatewayAdapter> logger)
    {
        _httpClient = httpClient;
        _secretProtector = secretProtector;
        _logger = logger;
    }

    public async Task<GatewayStartResult> StartAsync(GatewayStartRequest request, CancellationToken ct)
    {
        _logger.LogInformation("Tara starting payment for amount {Amount}", request.Amount);

        try
        {
            var config = await ResolveConfigAsync(request.ConfigJson, request.EncryptedSecrets, ct);
            var accessToken = await GetAccessTokenAsync(config, ct);

            var payload = new
            {
                amount = ((long)request.Amount).ToString(),
                mobile = "",
                order_id = request.PaymentTransactionId.ToString(),
                callback_url = request.CallbackUrl,
                additional_data = "",
                vat = 0,
                service_amount_list = new[]
                {
                    new { amount = (long)request.Amount, service_id = config.TerminalId }
                },
                ip = config.Ip ?? "127.0.0.1"
            };

            var result = await PostWithAuthAsync<TaraGetTokenResponse>(
                "/pay/api/getToken", payload, accessToken, ct);

            if (result?.Result != SuccessCode)
            {
                _logger.LogWarning("Tara getToken failed: code={Code}, desc={Description}",
                    result?.Result, result?.Description);
                return new GatewayStartResult(false, ErrorCode: $"tara_token_{result?.Result}",
                    ErrorMessage: result?.Description ?? "Token request failed");
            }

            _logger.LogInformation("Tara token obtained: {Token}", result.Token);

            var redirectBase = config.RedirectBaseUrl ?? config.BaseUrl ?? "";
            var redirectUrl = $"{redirectBase.TrimEnd('/')}/pay/api/ipgPurchase";

            return new GatewayStartResult(
                true,
                RedirectUrl: redirectUrl,
                Authority: result.Token,
                GatewayReferenceId: $"tara-{result.Token}");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Tara HTTP error during start");
            return new GatewayStartResult(false, ErrorCode: "tara_http_error", ErrorMessage: ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tara unexpected error during start");
            return new GatewayStartResult(false, ErrorCode: "tara_error", ErrorMessage: ex.Message);
        }
    }

    public async Task<GatewayVerifyResult> VerifyAsync(GatewayVerifyRequest request, CancellationToken ct)
    {
        _logger.LogInformation("Tara verifying payment for authority {Authority}", request.Authority);

        try
        {
            var config = await ResolveConfigAsync(request.ConfigJson, request.EncryptedSecrets, ct);
            var accessToken = await GetAccessTokenAsync(config, ct);

            var payload = new
            {
                ip = config.Ip ?? "127.0.0.1",
                token = request.Authority
            };

            var result = await PostWithAuthAsync<TaraVerifyResponse>(
                "/pay/api/purchaseVerify", payload, accessToken, ct);

            if (result?.Result != SuccessCode)
            {
                _logger.LogWarning("Tara verify failed: code={Code}, desc={Description}",
                    result?.Result, result?.Description);
                return new GatewayVerifyResult(false, ErrorCode: $"tara_verify_{result?.Result}",
                    ErrorMessage: result?.Description ?? "Verification failed");
            }

            long.TryParse(result.Amount, out long verifiedAmount);

            _logger.LogInformation("Tara payment verified: RRN={Rrn}", result.Rrn);

            return new GatewayVerifyResult(
                true,
                GatewayReferenceId: result.Rrn,
                TraceNumber: result.Rrn,
                Rrn: result.Rrn,
                VerifiedAmount: verifiedAmount > 0 ? verifiedAmount : request.Amount,
                ResponseCode: SuccessCode,
                ResponseMessage: result.Description);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Tara HTTP error during verify");
            return new GatewayVerifyResult(false, ErrorCode: "tara_http_error", ErrorMessage: ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tara unexpected error during verify");
            return new GatewayVerifyResult(false, ErrorCode: "tara_error", ErrorMessage: ex.Message);
        }
    }

    public async Task<RefundGatewayResult> RefundAsync(RefundGatewayRequest request, CancellationToken ct)
    {
        _logger.LogInformation("Tara processing refund for RRN {Rrn}", request.OriginalTrackingCode);

        try
        {
            var config = await ResolveConfigAsync(request.ConfigJson, request.EncryptedSecrets, ct);

            if (string.IsNullOrWhiteSpace(config.RefundLoginPrincipal) ||
                string.IsNullOrWhiteSpace(config.RefundLoginPassword))
            {
                return new RefundGatewayResult(false,
                    ErrorCode: "tara_refund_credentials_missing",
                    ErrorMessage: "Tara refund login credentials are not configured");
            }

            var refundAccessToken = await GetRefundAccessTokenAsync(config, ct);
            if (!long.TryParse(request.Amount, out long refundAmount) || refundAmount <= 0)
                return new RefundGatewayResult(false, ErrorCode: "tara_invalid_amount",
                    ErrorMessage: $"Invalid refund amount: {request.Amount}");

            var payload = new { amount = refundAmount };
            var json = JsonSerializer.Serialize(payload);

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post,
                $"api/v1/user/purchase/limited/refund/partial/{request.OriginalTrackingCode}");
            httpRequest.Headers.Add("Authorization", $"Bearer {refundAccessToken}");
            httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(httpRequest, ct);
            response.EnsureSuccessStatusCode();

            _logger.LogInformation("Tara refund submitted for RRN {Rrn}", request.OriginalTrackingCode);

            return new RefundGatewayResult(
                IsSuccess: true,
                GatewayTrackingCode: request.OriginalTrackingCode,
                IsPending: true,
                SafeResponse: new { rrn = request.OriginalTrackingCode, status = "submitted" });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Tara HTTP error during refund");
            return new RefundGatewayResult(false,
                ErrorCode: "tara_refund_http_error", ErrorMessage: ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tara unexpected error during refund");
            return new RefundGatewayResult(false,
                ErrorCode: "tara_refund_error", ErrorMessage: ex.Message,
                RequiresManualReview: true);
        }
    }

    // ─── Auth ──────────────────────────────────────────────────
    // TODO: Add token caching (Redis) to reduce login calls.

    private async Task<string> GetAccessTokenAsync(TaraConfig config, CancellationToken ct)
    {
        var loginPayload = new { username = config.UserName, password = config.Password };
        var json = JsonSerializer.Serialize(loginPayload);

        using var request = new HttpRequestMessage(HttpMethod.Post, "/pay/api/v2/authenticate");
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<TaraLoginResponse>(cancellationToken: ct)
            ?? throw new InvalidOperationException("Tara login returned null");
        return result.AccessToken ?? throw new InvalidOperationException("Tara login returned null token");
    }

    private async Task<string> GetRefundAccessTokenAsync(TaraConfig config, CancellationToken ct)
    {
        var loginPayload = new { principal = config.RefundLoginPrincipal, password = config.RefundLoginPassword };
        var json = JsonSerializer.Serialize(loginPayload);

        using var request = new HttpRequestMessage(HttpMethod.Post, "/club/api/v1/user/login/refund");
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<TaraRefundLoginResponse>(cancellationToken: ct)
            ?? throw new InvalidOperationException("Tara refund login returned null");
        return result.AccessCode ?? throw new InvalidOperationException("Tara refund login returned null token");
    }

    private async Task<TResponse?> PostWithAuthAsync<TResponse>(
        string path, object payload, string accessToken, CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(payload);
        using var request = new HttpRequestMessage(HttpMethod.Post, path);
        request.Headers.Add("Authorization", $"Bearer {accessToken}");
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken: ct);
    }

    private async Task<TaraConfig> ResolveConfigAsync(
        string configJson, string? encryptedSecrets, CancellationToken ct)
    {
        var config = JsonSerializer.Deserialize<TaraConfig>(configJson)
            ?? throw new InvalidOperationException("Invalid Tara gateway config");

        if (!string.IsNullOrWhiteSpace(encryptedSecrets))
        {
            var secretsJson = await _secretProtector.DecryptSecretsAsync(encryptedSecrets, ct);
            if (secretsJson is not null)
            {
                var secrets = JsonSerializer.Deserialize<TaraSecrets>(secretsJson);
                if (secrets is not null)
                {
                    config.UserName = secrets.UserName ?? config.UserName;
                    config.Password = secrets.Password ?? config.Password;
                    config.RefundLoginPrincipal = secrets.RefundLoginPrincipal ?? config.RefundLoginPrincipal;
                    config.RefundLoginPassword = secrets.RefundLoginPassword ?? config.RefundLoginPassword;
                }
            }
        }

        if (string.IsNullOrWhiteSpace(config.UserName))
            throw new InvalidOperationException("Tara username is not configured");
        if (string.IsNullOrWhiteSpace(config.Password))
            throw new InvalidOperationException("Tara password is not configured");

        if (string.IsNullOrWhiteSpace(config.BaseUrl))
            config.BaseUrl = config.RedirectBaseUrl;

        if (_httpClient.BaseAddress is null && !string.IsNullOrWhiteSpace(config.BaseUrl))
            _httpClient.BaseAddress = new Uri(config.BaseUrl.TrimEnd('/'));

        return config;
    }
}

// ─── Config DTOs ───────────────────────────────────────────────

internal sealed class TaraConfig
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string TerminalId { get; set; } = string.Empty;
    public string? Ip { get; set; }
    public string? BaseUrl { get; set; }
    public string? RedirectBaseUrl { get; set; }
    public string? RefundLoginPrincipal { get; set; }
    public string? RefundLoginPassword { get; set; }
}

internal sealed class TaraSecrets
{
    [JsonPropertyName("username")]
    public string? UserName { get; set; }
    [JsonPropertyName("password")]
    public string? Password { get; set; }
    [JsonPropertyName("refund_login_principal")]
    public string? RefundLoginPrincipal { get; set; }
    [JsonPropertyName("refund_login_password")]
    public string? RefundLoginPassword { get; set; }
}

// ─── API Response DTOs ─────────────────────────────────────────

internal sealed class TaraLoginResponse
{
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }
}

internal sealed class TaraRefundLoginResponse
{
    [JsonPropertyName("accessCode")]
    public string? AccessCode { get; set; }
}

internal sealed class TaraGetTokenResponse
{
    [JsonPropertyName("result")]
    public string? Result { get; set; }
    [JsonPropertyName("token")]
    public string? Token { get; set; }
    [JsonPropertyName("description")]
    public string? Description { get; set; }
}

internal sealed class TaraVerifyResponse
{
    [JsonPropertyName("result")]
    public string? Result { get; set; }
    [JsonPropertyName("rrn")]
    public string? Rrn { get; set; }
    [JsonPropertyName("amount")]
    public string? Amount { get; set; }
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    [JsonPropertyName("cardMaskPan")]
    public string? CardMaskPan { get; set; }
}
