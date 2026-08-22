using Microsoft.Extensions.Localization;
using Minisource.Common.Exceptions;
using Minisource.Common.Response;
using Presentaion.Resources;
using System.Net;
using System.Text.Json;

namespace Presentaion.Middlewares;

/// <summary>
/// Middleware for handling exceptions and returning standardized BaseResponse
/// with localized user-facing messages.
/// 
/// - error.code stays stable and language-independent
/// - error.user_message is localized via IStringLocalizer
/// - Logs stay in English for debugging
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IStringLocalizer<SharedResource> localizer)
    {
        _next = next;
        _logger = logger;
        _localizer = localizer;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    /// <summary>
    /// Maps an error code to its localized resource key.
    /// Returns the error code itself as fallback (so keys without translation
    /// will show the English code).
    /// </summary>
    private string Localize(string errorCode)
    {
        var localized = _localizer[errorCode];
        return localized.ResourceNotFound ? errorCode : localized.Value;
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        HttpStatusCode statusCode;
        string errorCode;
        string technicalMessage;
        string userMessage;
        object? details = null;
        IReadOnlyList<ValidationError>? validationErrors = null;

        switch (exception)
        {
            case ValidationException validationEx:
                statusCode = HttpStatusCode.BadRequest;
                errorCode = "validation_failed";
                technicalMessage = validationEx.Message;
                userMessage = Localize("payment.validation_failed");
                validationErrors = validationEx.ValidationErrors.Select(e => new ValidationError
                {
                    Field = e.Field,
                    Message = Localize($"validation.{e.Tag ?? "invalid"}") == $"validation.{e.Tag ?? "invalid"}"
                        ? e.Message
                        : string.Format(Localize($"validation.{e.Tag ?? "invalid"}"), e.Field),
                    Tag = e.Tag,
                    Value = e.Value,
                }).ToList();
                break;

            case NotFoundException notFoundEx:
                statusCode = HttpStatusCode.NotFound;
                errorCode = notFoundEx switch
                {
                    { ResourceType: "Wallet" } => "wallet_not_found",
                    { ResourceType: "PaymentIntent" } => "payment_intent_not_found",
                    { ResourceType: "GatewayConfig" } => "gateway_config_not_found",
                    { ResourceType: "PayoutAccount" } => "payout_account_not_found",
                    { ResourceType: "WithdrawalRequest" } => "withdrawal_not_found",
                    { ResourceType: "ReconciliationBatch" } => "reconciliation_batch_not_found",
                    { ResourceType: "RiskCase" } => "risk_case_not_found",
                    { ResourceType: "AuditLog" } => "audit_log_not_found",
                    { ResourceType: "OutboxEvent" } => "outbox_event_not_found",
                    { ResourceType: "WebhookSubscription" } => "webhook_subscription_not_found",
                    { ResourceType: "EventRoutingRule" } => "event_routing_rule_not_found",
                    { ResourceType: "ApprovalRequest" } => "approval_not_found",
                    _ => "resource_not_found",
                };
                technicalMessage = notFoundEx.Message;
                // Map to localized key
                var notFoundKey = errorCode.Replace("_not_found", ".not_found")
                    .Replace("resource", "payment");
                userMessage = Localize(notFoundKey);
                break;

            case UnauthorizedException unauthorizedEx:
                statusCode = HttpStatusCode.Unauthorized;
                errorCode = "unauthenticated";
                technicalMessage = unauthorizedEx.Message;
                userMessage = Localize("payment.unauthenticated");
                break;

            case ForbiddenException forbiddenEx:
                statusCode = HttpStatusCode.Forbidden;
                errorCode = "payment_forbidden";
                technicalMessage = forbiddenEx.Message;
                userMessage = Localize("payment.forbidden");
                break;

            case ConflictException conflictEx:
                statusCode = HttpStatusCode.Conflict;
                errorCode = "conflict";
                technicalMessage = conflictEx.Message;
                userMessage = conflictEx.Message; // Keep original for conflicts
                break;

            case BusinessException businessEx:
                statusCode = HttpStatusCode.UnprocessableEntity;
                errorCode = businessEx.ErrorCode ?? "business_error";
                technicalMessage = businessEx.Message;
                // Derive localization key from error code
                var bizKey = (businessEx.ErrorCode ?? "business_error")
                    .Replace("_", ".");
                userMessage = Localize(bizKey);
                details = businessEx.Details;
                break;

            case PaymentException paymentEx:
                statusCode = HttpStatusCode.UnprocessableEntity;
                errorCode = "payment_error";
                technicalMessage = paymentEx.Message;
                userMessage = Localize("payment_intent.verify_failed");
                details = paymentEx.Details;
                break;

            case WalletException walletEx:
                statusCode = HttpStatusCode.UnprocessableEntity;
                errorCode = "wallet_error";
                technicalMessage = walletEx.Message;
                userMessage = Localize("wallet.insufficient_balance");
                details = new { walletEx.UserId, walletEx.RequestedAmount, walletEx.AvailableBalance };
                break;

            case RateLimitException rateLimitEx:
                statusCode = HttpStatusCode.TooManyRequests;
                errorCode = "rate_limit_exceeded";
                technicalMessage = rateLimitEx.Message;
                userMessage = "Rate limit exceeded. Please try again later.";
                if (rateLimitEx.RetryAfter.HasValue)
                {
                    context.Response.Headers.Append("Retry-After", rateLimitEx.RetryAfter.Value.TotalSeconds.ToString("0"));
                }
                break;

            case ExternalServiceException externalEx:
                statusCode = HttpStatusCode.BadGateway;
                errorCode = "external_service_error";
                technicalMessage = externalEx.Message;
                userMessage = "External service error. Please try again later.";
                details = externalEx.Details;
                _logger.LogError(exception, "External service error: {Service}", externalEx.ServiceName);
                break;

            case AppException appEx:
                statusCode = (HttpStatusCode)appEx.HttpStatusCode;
                errorCode = "application_error";
                technicalMessage = appEx.Message;
                userMessage = Localize("payment.internal_error");
                details = appEx.Details;
                break;

            default:
                statusCode = HttpStatusCode.InternalServerError;
                errorCode = "internal_error";
                technicalMessage = exception.Message;
                userMessage = Localize("payment.internal_error");
                _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
                break;
        }

        // Log non-internal errors at warning level
        if (statusCode < HttpStatusCode.InternalServerError && statusCode != HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Client error {StatusCode} ({ErrorCode}): {Message}",
                (int)statusCode, errorCode, technicalMessage);
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        // Build the error response matching frontend's ApiErrorResponse shape
        var errorResponse = new
        {
            error = new
            {
                code = errorCode,
                message = technicalMessage,
                user_message = userMessage,
                category = GetCategory(statusCode, validationErrors != null),
                details,
                field_errors = validationErrors?.Select(e => new
                {
                    field = e.Field,
                    code = e.Tag,
                    message = e.Message,
                }),
                request_id = context.Items["RequestId"]?.ToString(),
                correlation_id = context.Items["CorrelationId"]?.ToString(),
            }
        };

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse, options));
    }

    private static string GetCategory(HttpStatusCode statusCode, bool isValidation)
    {
        if (isValidation) return "validation";
        return statusCode switch
        {
            HttpStatusCode.BadRequest => "validation",
            HttpStatusCode.Unauthorized => "permission",
            HttpStatusCode.Forbidden => "permission",
            HttpStatusCode.NotFound => "not_found",
            HttpStatusCode.Conflict => "business",
            HttpStatusCode.UnprocessableEntity => "business",
            HttpStatusCode.TooManyRequests => "system",
            HttpStatusCode.BadGateway => "system",
            _ => "system",
        };
    }
}
