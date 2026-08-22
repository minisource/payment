using System.Security.Claims;
using Minisource.Common.Tenancy;
using payment.Abstractions;
using payment.Options;

namespace Presentaion.Middlewares;

/// <summary>
/// Extracts tenant/user/application context from JWT claims, headers, or standalone config fallback
/// and populates ITenantContext for the current request.
/// Supports both auth-enabled and auth-disabled (standalone) modes.
/// </summary>
public sealed class TenantContextMiddleware
{
    private readonly RequestDelegate _next;

    public TenantContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext, IConfiguration configuration)
    {
        var claimsTenantContext = tenantContext as ClaimsTenantContext;
        if (claimsTenantContext == null)
        {
            await _next(context);
            return;
        }

        var authEnabled = configuration.GetValue<bool>("Auth:Enabled");
        var user = context.User;

        if (authEnabled && user.Identity?.IsAuthenticated == true)
        {
            // ── Auth-enabled mode: populate from JWT claims ─────────────
            claimsTenantContext.PopulateFromClaims(user);

            // Override with header if explicitly provided (must match claims)
            var headerTenantId = context.Request.Headers[RequestHeaders.TenantId].FirstOrDefault();
            if (!string.IsNullOrEmpty(headerTenantId) && Guid.TryParse(headerTenantId, out var headerTid))
            {
                if (claimsTenantContext.TenantId.HasValue && headerTid != claimsTenantContext.TenantId.Value)
                {
                    context.Response.StatusCode = 403;
                    await context.Response.WriteAsync(
                        "{\"error\":{\"code\":\"tenant_access_denied\",\"message\":\"Tenant ID does not match token claims\"}}");
                    return;
                }
                claimsTenantContext.OverrideTenantId(headerTid);
            }

            // Application code from header
            var appCode = context.Request.Headers[RequestHeaders.ApplicationCode].FirstOrDefault();
            if (!string.IsNullOrEmpty(appCode))
                claimsTenantContext.OverrideApplicationCode(appCode);
        }
        else
        {
            // ── Auth-disabled (standalone) mode: fallback from headers or config ──
            var headerTenantId = context.Request.Headers[RequestHeaders.TenantId].FirstOrDefault();
            if (!string.IsNullOrEmpty(headerTenantId) && Guid.TryParse(headerTenantId, out var tid))
            {
                claimsTenantContext.OverrideTenantId(tid);
            }
            else
            {
                // Fallback to default tenant from config
                var defaultTenantId = configuration.GetValue<string>("Payment:Standalone:DefaultTenantId");
                if (!string.IsNullOrEmpty(defaultTenantId) && Guid.TryParse(defaultTenantId, out var defaultTid))
                {
                    claimsTenantContext.OverrideTenantId(defaultTid);
                }
            }

            // Application code from header or config fallback
            var appCode = context.Request.Headers[RequestHeaders.ApplicationCode].FirstOrDefault();
            if (!string.IsNullOrEmpty(appCode))
            {
                claimsTenantContext.OverrideApplicationCode(appCode);
            }
            else
            {
                var defaultAppCode = configuration.GetValue<string>("Payment:Standalone:DefaultApplicationCode") ?? "payment";
                claimsTenantContext.OverrideApplicationCode(defaultAppCode);
            }
        }

        // Request/Correlation IDs (always populated)
        var requestId = context.Request.Headers[RequestHeaders.RequestId].FirstOrDefault();
        if (string.IsNullOrEmpty(requestId))
            requestId = $"req_{Guid.NewGuid():N}";
        claimsTenantContext.RequestId = requestId;

        var correlationId = context.Request.Headers[RequestHeaders.CorrelationId].FirstOrDefault();
        claimsTenantContext.CorrelationId = correlationId;

        await _next(context);
    }
}

/// <summary>
/// ITenantContext implementation backed by JWT claims and headers.
/// </summary>
public class ClaimsTenantContext : ITenantContext
{
    public Guid? TenantId { get; private set; }
    public Guid? UserId { get; private set; }
    public string? ApplicationCode { get; private set; }
    public bool IsServiceAccount { get; private set; }

    public string? RequestId { get; internal set; }
    public string? CorrelationId { get; internal set; }

    public void PopulateFromClaims(ClaimsPrincipal user)
    {
        var tenantClaim = user.FindFirst(TenantClaimTypes.TenantId)?.Value;
        if (tenantClaim != null && Guid.TryParse(tenantClaim, out var tid))
            TenantId = tid;

        var userIdClaim = user.FindFirst(TenantClaimTypes.UserId)?.Value
            ?? user.FindFirst("uid")?.Value;
        if (userIdClaim != null && Guid.TryParse(userIdClaim, out var uid))
            UserId = uid;

        var clientIdClaim = user.FindFirst(TenantClaimTypes.ClientId)?.Value;
        IsServiceAccount = !string.IsNullOrEmpty(clientIdClaim) && UserId == null;
    }

    public void OverrideTenantId(Guid tenantId) => TenantId = tenantId;
    public void OverrideApplicationCode(string appCode) => ApplicationCode = appCode;
}
