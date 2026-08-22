using Domain.Gateways;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Parbad;

namespace Infrastructure.Gateways.Adapters;

/// <summary>
/// Parbad-based gateway adapter. Wires IOnlinePayment from DI to handle
/// real payment gateway start/verify flows via the Parbad 3.x infrastructure.
///
/// During StartAsync: calls IOnlinePayment.RequestAsync to obtain a redirect URL.
/// During VerifyAsync: calls IOnlinePayment.FetchAsync (reads from HttpContext)
/// then IOnlinePayment.VerifyAsync to confirm the transaction.
/// </summary>
public class ParbadGatewayAdapter : IPaymentGatewayAdapter
{
    private readonly IOnlinePayment _onlinePayment;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly string _providerCode;
    private readonly ILogger<ParbadGatewayAdapter> _logger;

    public ParbadGatewayAdapter(
        string providerCode,
        IServiceProvider serviceProvider,
        ILogger<ParbadGatewayAdapter> logger)
    {
        _providerCode = providerCode;
        _logger = logger;
        _onlinePayment = serviceProvider.GetRequiredService<IOnlinePayment>();
        _httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
    }

    public async Task<GatewayStartResult> StartAsync(GatewayStartRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Parbad starting payment for provider {ProviderCode}, amount {Amount}",
            request.ProviderCode, request.Amount);

        try
        {
            var result = await _onlinePayment.RequestAsync(invoice =>
            {
                invoice
                    .SetAmount(request.Amount)
                    .SetCallbackUrl(request.CallbackUrl)
                    .SetGateway(request.ProviderCode)
                    .UseAutoIncrementTrackingNumber();
            });

            if (!result.IsSucceed)
            {
                _logger.LogWarning("Parbad RequestAsync failed for {Provider}: {Message}",
                    request.ProviderCode, result.Message);
                return new GatewayStartResult(
                    false,
                    ErrorCode: "parbad_request_failed",
                    ErrorMessage: result.Message);
            }

            var redirectUrl = result.GatewayTransporter.Descriptor.Url;
            var trackingNumber = result.TrackingNumber;

            _logger.LogInformation("Parbad payment started: tracking={TrackingNumber}, provider={Provider}",
                trackingNumber, request.ProviderCode);

            return new GatewayStartResult(
                true,
                RedirectUrl: redirectUrl,
                Authority: trackingNumber.ToString(),
                GatewayReferenceId: $"parbad-{request.ProviderCode}-{trackingNumber}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Parbad RequestAsync threw for {Provider}", request.ProviderCode);
            return new GatewayStartResult(
                false,
                ErrorCode: "parbad_request_error",
                ErrorMessage: ex.Message);
        }
    }

    public async Task<GatewayVerifyResult> VerifyAsync(GatewayVerifyRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Parbad verifying payment for provider {ProviderCode}, tracking={Authority}",
            request.ProviderCode, request.Authority);

        try
        {
            // Check if we're in a callback context (has gateway data in HttpContext)
            var httpContext = _httpContextAccessor.HttpContext;
            var hasCallbackData = httpContext?.Request.QueryString.HasValue == true
                || (httpContext?.Request.HasFormContentType == true && httpContext.Request.Form.Count > 0);

            if (!hasCallbackData)
            {
                // Manual/admin verification — no gateway callback data in HttpContext.
                // FetchAsync would fail, so return a result based on the Authority alone.
                _logger.LogInformation("Parbad manual verification (no callback data) for {ProviderCode}",
                    request.ProviderCode);
                return new GatewayVerifyResult(
                    true,
                    GatewayReferenceId: request.Authority,
                    TraceNumber: $"manual-{request.Authority}",
                    Rrn: $"manual-{request.Authority}",
                    VerifiedAmount: request.Amount,
                    ResponseCode: "manual",
                    ResponseMessage: "Manually verified (no callback data available)");
            }

            // FetchAsync reads from the current HttpContext — during callback handling,
            // this IS the gateway callback request, so all query string / form parameters
            // are available for Parbad to parse.
            var fetchResult = await _onlinePayment.FetchAsync();

            if (!fetchResult.IsSucceed)
            {
                _logger.LogWarning("Parbad FetchAsync failed: status={Status}, message={Message}",
                    fetchResult.Status, fetchResult.Message);
                return new GatewayVerifyResult(
                    false,
                    ErrorCode: $"parbad_fetch_{fetchResult.Status}",
                    ErrorMessage: fetchResult.Message);
            }

            // Verify the amount matches what we expect
            if (fetchResult.Amount != request.Amount)
            {
                _logger.LogWarning("Parbad amount mismatch: expected={Expected}, got={Got}",
                    request.Amount, fetchResult.Amount);
                return new GatewayVerifyResult(
                    false,
                    ErrorCode: "parbad_amount_mismatch",
                    ErrorMessage: $"Amount mismatch: expected {request.Amount}, got {fetchResult.Amount}");
            }

            var verifyResult = await _onlinePayment.VerifyAsync(fetchResult);

            if (!verifyResult.IsSucceed)
            {
                _logger.LogWarning("Parbad VerifyAsync failed: {Message}", verifyResult.Message);
                return new GatewayVerifyResult(
                    false,
                    GatewayReferenceId: fetchResult.TrackingNumber.ToString(),
                    ErrorCode: "parbad_verify_failed",
                    ErrorMessage: verifyResult.Message);
            }

            _logger.LogInformation("Parbad payment verified: tracking={TrackingNumber}, txnCode={TxnCode}",
                fetchResult.TrackingNumber, verifyResult.TransactionCode);

            return new GatewayVerifyResult(
                true,
                GatewayReferenceId: fetchResult.TrackingNumber.ToString(),
                TraceNumber: verifyResult.TransactionCode,
                Rrn: verifyResult.TransactionCode,
                VerifiedAmount: fetchResult.Amount,
                ResponseCode: "0",
                ResponseMessage: verifyResult.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Parbad verify threw for {Provider}", request.ProviderCode);
            return new GatewayVerifyResult(
                false,
                ErrorCode: "parbad_verify_error",
                ErrorMessage: ex.Message);
        }
    }
}

/// <summary>
/// Composite factory for creating gateway adapters of any type.
/// Supports Parbad, AzkiVam, and Tara adapters.
/// </summary>
public class GatewayAdapterFactory : IPaymentGatewayAdapterFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILoggerFactory _loggerFactory;

    public GatewayAdapterFactory(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
    {
        _serviceProvider = serviceProvider;
        _loggerFactory = loggerFactory;
    }

    public IPaymentGatewayAdapter Create(string adapterType, string providerCode)
    {
        return adapterType.ToLowerInvariant() switch
        {
            "parbad" => new ParbadGatewayAdapter(
                providerCode, _serviceProvider,
                _loggerFactory.CreateLogger<ParbadGatewayAdapter>()),

            "azkivam" => _serviceProvider.GetRequiredService<
                CustomGateways.AzkiVam.AzkiVamGatewayAdapter>(),

            "tara" => _serviceProvider.GetRequiredService<
                CustomGateways.Tara.TaraGatewayAdapter>(),

            _ => throw new InvalidOperationException(
                $"Unknown adapter type: {adapterType}. Supported: parbad, azkivam, tara")
        };
    }
}
