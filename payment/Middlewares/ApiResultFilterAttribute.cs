using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Minisource.Common.Response;

namespace Presentaion.Middlewares;

/// <summary>
/// Action filter that wraps all responses in BaseResponse for consistent API responses.
/// </summary>
public class ApiResultFilterAttribute : ActionFilterAttribute
{
    public override void OnResultExecuting(ResultExecutingContext context)
    {
        // If already wrapped in BaseResponse, preserve it
        if (context.Result is ObjectResult objectResult && objectResult.Value is BaseResponse)
        {
            base.OnResultExecuting(context);
            return;
        }

        // Wrap OkObjectResult
        if (context.Result is OkObjectResult okObjectResult)
        {
            var wrapped = WrapInBaseResponse(okObjectResult.Value);
            context.Result = new JsonResult(wrapped) { StatusCode = 200 };
        }
        // Wrap OkResult
        else if (context.Result is OkResult)
        {
            var wrapped = BaseResponse.Ok();
            context.Result = new JsonResult(wrapped) { StatusCode = 200 };
        }
        // Handle BadRequest
        else if (context.Result is ObjectResult badRequestResult && badRequestResult.StatusCode == 400)
        {
            var message = ExtractErrorMessage(badRequestResult.Value);
            var wrapped = BaseResponse.Fail(ResultCode.ValidationError, message ?? "Bad request");
            context.Result = new JsonResult(wrapped) { StatusCode = 400 };
        }
        // Handle NotFound
        else if (context.Result is ObjectResult notFoundResult && notFoundResult.StatusCode == 404)
        {
            var message = ExtractErrorMessage(notFoundResult.Value);
            var wrapped = BaseResponse.NotFound(message ?? "Resource not found");
            context.Result = new JsonResult(wrapped) { StatusCode = 404 };
        }
        // Handle Unauthorized
        else if (context.Result is UnauthorizedResult)
        {
            var wrapped = BaseResponse.Unauthorized();
            context.Result = new JsonResult(wrapped) { StatusCode = 401 };
        }
        // Handle Forbidden
        else if (context.Result is ForbidResult)
        {
            var wrapped = BaseResponse.Forbidden();
            context.Result = new JsonResult(wrapped) { StatusCode = 403 };
        }
        // Handle ContentResult
        else if (context.Result is ContentResult contentResult)
        {
            var wrapped = BaseResponse<string>.Ok(contentResult.Content ?? string.Empty);
            context.Result = new JsonResult(wrapped) { StatusCode = contentResult.StatusCode ?? 200 };
        }
        // Handle other ObjectResults
        else if (context.Result is ObjectResult otherResult && otherResult.StatusCode == null)
        {
            var wrapped = WrapInBaseResponse(otherResult.Value);
            context.Result = new JsonResult(wrapped) { StatusCode = 200 };
        }

        base.OnResultExecuting(context);
    }

    private static BaseResponse WrapInBaseResponse(object? value)
    {
        if (value == null)
            return BaseResponse.Ok();

        var valueType = value.GetType();
        var responseType = typeof(BaseResponse<>).MakeGenericType(valueType);
        var okMethod = responseType.GetMethod("Ok", new[] { valueType, typeof(string) });

        if (okMethod != null)
        {
            return (BaseResponse)okMethod.Invoke(null, new[] { value, null })!;
        }

        // Fallback to object wrapper
        return BaseResponse<object>.Ok(value);
    }

    private static string? ExtractErrorMessage(object? value)
    {
        return value switch
        {
            ValidationProblemDetails validationProblemDetails =>
                string.Join(" | ", validationProblemDetails.Errors.SelectMany(p => p.Value).Distinct()),

            SerializableError errors =>
                string.Join(" | ", errors.SelectMany(p => (string[])p.Value).Distinct()),

            ProblemDetails => null,

            not null => value.ToString(),

            _ => null
        };
    }
}
