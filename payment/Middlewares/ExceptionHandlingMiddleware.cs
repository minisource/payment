using Minisource.Common.Exceptions;
using Minisource.Common.Response;
using System.Net;
using System.Text.Json;

namespace Presentaion.Middlewares;

/// <summary>
/// Middleware for handling exceptions and returning standardized BaseResponse.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
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

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        HttpStatusCode statusCode;
        BaseResponse response;

        switch (exception)
        {
            case ValidationException validationEx:
                statusCode = HttpStatusCode.BadRequest;
                response = validationEx.ToResponse();
                break;

            case NotFoundException notFoundEx:
                statusCode = HttpStatusCode.NotFound;
                response = notFoundEx.ToResponse();
                break;

            case UnauthorizedException unauthorizedEx:
                statusCode = HttpStatusCode.Unauthorized;
                response = unauthorizedEx.ToResponse();
                break;

            case ForbiddenException forbiddenEx:
                statusCode = HttpStatusCode.Forbidden;
                response = forbiddenEx.ToResponse();
                break;

            case ConflictException conflictEx:
                statusCode = HttpStatusCode.Conflict;
                response = conflictEx.ToResponse();
                break;

            case BusinessException businessEx:
                statusCode = HttpStatusCode.UnprocessableEntity;
                response = businessEx.ToResponse();
                break;

            case PaymentException paymentEx:
                statusCode = HttpStatusCode.UnprocessableEntity;
                response = paymentEx.ToResponse();
                break;

            case WalletException walletEx:
                statusCode = HttpStatusCode.UnprocessableEntity;
                response = walletEx.ToResponse();
                break;

            case RateLimitException rateLimitEx:
                statusCode = HttpStatusCode.TooManyRequests;
                response = rateLimitEx.ToResponse();
                if (rateLimitEx.RetryAfter.HasValue)
                {
                    context.Response.Headers.Append("Retry-After", rateLimitEx.RetryAfter.Value.TotalSeconds.ToString("0"));
                }
                break;

            case ExternalServiceException externalEx:
                statusCode = HttpStatusCode.BadGateway;
                response = externalEx.ToResponse();
                _logger.LogError(exception, "External service error: {Service}", externalEx.ServiceName);
                break;

            case AppException appEx:
                statusCode = (HttpStatusCode)appEx.HttpStatusCode;
                response = appEx.ToResponse();
                break;

            default:
                statusCode = HttpStatusCode.InternalServerError;
                response = BaseResponse.InternalError("An unexpected error occurred");
                _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
                break;
        }

        // Log non-internal errors at warning level
        if (statusCode < HttpStatusCode.InternalServerError && statusCode != HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Client error {StatusCode}: {Message}", (int)statusCode, exception.Message);
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }
}
