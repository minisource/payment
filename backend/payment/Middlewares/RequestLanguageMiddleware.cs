using System.Globalization;

namespace Presentaion.Middlewares;

/// <summary>
/// Request-scoped language context providing the normalized language for the current request.
/// </summary>
public interface IRequestLanguageContext
{
    /// <summary>Normalized language code: "fa" or "en"</summary>
    string Language { get; }
}

/// <summary>
/// Default implementation of IRequestLanguageContext, populated by RequestLanguageMiddleware.
/// </summary>
public sealed class RequestLanguageContext : IRequestLanguageContext
{
    public string Language { get; set; } = "fa";
}

/// <summary>
/// Middleware that reads language preference from request headers (X-Language, X-Locale, Accept-Language),
/// normalizes to "fa" or "en", and sets the current thread culture accordingly.
/// 
/// Header priority: X-Language > X-Locale > Accept-Language
/// Normalization: fa, fa-IR, fa_IR, fa-ir → fa; en, en-US, en_US, en-us → en
/// Fallback: fa
/// 
/// Registration order: after RequestId/CorrelationId, before ExceptionHandlingMiddleware.
/// </summary>
public sealed class RequestLanguageMiddleware
{
    private readonly RequestDelegate _next;

    public RequestLanguageMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IRequestLanguageContext languageContext)
    {
        var lang = DetectLanguage(context);

        // Set on the request-scoped context
        if (languageContext is RequestLanguageContext ctx)
        {
            ctx.Language = lang;
        }

        // Set .NET thread culture for ResourceManager / IStringLocalizer
        var culture = lang switch
        {
            "fa" => new CultureInfo("fa-IR"),
            _ => new CultureInfo("en-US"),
        };

        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;

        await _next(context);
    }

    /// <summary>
    /// Detects the normalized language from request headers.
    /// Priority: X-Language > X-Locale > Accept-Language
    /// </summary>
    public static string DetectLanguage(HttpContext context)
    {
        var headers = context.Request.Headers;

        // Priority 1: X-Language
        var xLanguage = headers["X-Language"].FirstOrDefault();
        if (!string.IsNullOrEmpty(xLanguage))
        {
            var normalized = NormalizeLanguage(xLanguage);
            if (normalized != null) return normalized;
        }

        // Priority 2: X-Locale
        var xLocale = headers["X-Locale"].FirstOrDefault();
        if (!string.IsNullOrEmpty(xLocale))
        {
            var normalized = NormalizeLanguage(xLocale);
            if (normalized != null) return normalized;
        }

        // Priority 3: Accept-Language
        var acceptLanguage = headers["Accept-Language"].FirstOrDefault();
        if (!string.IsNullOrEmpty(acceptLanguage))
        {
            // Parse first language tag from Accept-Language
            var firstLang = acceptLanguage
                .Split(',')
                .Select(tag => tag.Split(';')[0].Trim())
                .FirstOrDefault();

            if (!string.IsNullOrEmpty(firstLang))
            {
                var normalized = NormalizeLanguage(firstLang);
                if (normalized != null) return normalized;
            }
        }

        // Fallback
        return "fa";
    }

    /// <summary>
    /// Normalizes a language string to "fa" or "en".
    /// Returns null for unsupported languages.
    /// </summary>
    public static string? NormalizeLanguage(string lang)
    {
        if (string.IsNullOrWhiteSpace(lang)) return null;

        var normalized = lang.Trim().ToLowerInvariant()
            .Replace("_", "-")  // Normalize underscore to hyphen
            .Split('-')[0];      // Take only the primary language tag

        return normalized switch
        {
            "fa" => "fa",
            "en" => "en",
            _ => null,  // Unsupported language → fallback
        };
    }
}
