using Application.DTOs;
using Application.Options;
using Application.Services;
using Infrastructure.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Minisource.Common.Response;
using Parbad;
using Presentaion.Middlewares;

namespace payment.Controllers;

/// <summary>
/// Controller for payment operations.
/// </summary>
[ApiController]
[ApiResultFilter]
[Route("api/v1/payments")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly IOptions<PaymentOptions> _options;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(
        IPaymentService paymentService,
        IOptions<PaymentOptions> options,
        ILogger<PaymentsController> logger)
    {
        _paymentService = paymentService;
        _options = options;
        _logger = logger;
    }

    #region Utilities

    private static object? CreateTransporterForClientApp(IGatewayTransporter? gatewayTransporter)
    {
        if (gatewayTransporter?.Descriptor == null) return null;

        var form = gatewayTransporter.Descriptor.Form?.Select(item => new
        {
            item.Key,
            item.Value
        });

        return new
        {
            gatewayTransporter.Descriptor.Type,
            gatewayTransporter.Descriptor.Url,
            Form = form
        };
    }

    private string RedirectFrontendAddress(object result)
    {
        var hostUrl = _options.Value.RedirectFromGatewayToUrl ?? "";
        var queryString = StringExtensions.ConvertToQueryStrings(result).ToLower();
        return $"{hostUrl}?{queryString}";
    }

    #endregion

    /// <summary>
    /// Initiates a new payment.
    /// </summary>
    /// <param name="request">Payment request details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Payment response with gateway transporter info.</returns>
    [HttpPost("pay")]
    [AllowAnonymous]
    public async Task<ActionResult<PayApiResponse>> Pay(
        [FromBody] PayRequest request,
        CancellationToken cancellationToken)
    {
        var url = Url.Action(nameof(Verify), "Payments", null, Request.Scheme);
        request.CallbackUrl = url ?? string.Empty;

        var response = await _paymentService.InitiatePaymentAsync(request, cancellationToken);

        // Create transporter info from PaymentRequestResult if available
        object? transporter = null;
        if (response.PaymentRequestResult?.GatewayTransporter != null)
        {
            transporter = CreateTransporterForClientApp(response.PaymentRequestResult.GatewayTransporter);
        }

        var apiResponse = new PayApiResponse(
            transporter ?? response.GatewayUrl,
            response.TrackingNumber.ToString());

        return Ok(apiResponse);
    }

    /// <summary>
    /// Callback endpoint for payment gateway verification.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Redirects to frontend with verification result.</returns>
    [HttpGet, HttpPost]
    [Route("verify")]
    [AllowAnonymous]
    public async Task<IActionResult> Verify(CancellationToken cancellationToken)
    {
        var response = await _paymentService.VerifyPaymentAsync(cancellationToken);
        
        _logger.LogInformation(
            "Payment verification completed for tracking number {TrackingNumber} with status {Status}",
            response.TrackingNumber,
            response.Status);

        return Redirect(RedirectFrontendAddress(response));
    }

    /// <summary>
    /// Gets a payment by ID.
    /// </summary>
    /// <param name="id">Payment ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Payment details.</returns>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PaymentDto>> GetPayment(
        Guid id,
        CancellationToken cancellationToken)
    {
        var payment = await _paymentService.GetPaymentAsync(id, cancellationToken);
        return Ok(payment);
    }

    /// <summary>
    /// Gets paginated list of payments with optional filters.
    /// </summary>
    /// <param name="status">Filter by payment status.</param>
    /// <param name="from">Filter by start date.</param>
    /// <param name="to">Filter by end date.</param>
    /// <param name="page">Page number (1-based).</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paginated list of payments.</returns>
    [HttpGet]
    public async Task<ActionResult<PagedResult<PaymentDto>>> GetPayments(
        [FromQuery] string? status,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _paymentService.GetPaymentsAsync(status, from, to, page, pageSize, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets payment logs for a specific payment.
    /// </summary>
    /// <param name="id">Payment ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of payment logs.</returns>
    [HttpGet("{id:guid}/logs")]
    public async Task<ActionResult<List<PaymentLogDto>>> GetPaymentLogs(
        Guid id,
        CancellationToken cancellationToken)
    {
        var logs = await _paymentService.GetPaymentLogsAsync(id, cancellationToken);
        return Ok(logs);
    }
}

/// <summary>
/// Payment API response containing gateway transporter info.
/// </summary>
public record PayApiResponse(object? Transporter, string TrackingNumber);
