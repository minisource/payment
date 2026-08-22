using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Minisource.Common.Exceptions;
using payment.Abstractions;
using Presentaion.Middlewares;

namespace payment.Controllers;

/// <summary>
/// Base controller for all Payment API controllers.
/// Provides common properties from JWT claims, request headers, or standalone config fallback.
/// Public-facing controllers (gateway callbacks, payment links) override with [AllowAnonymous].
/// In no-auth mode, tenant/user context falls back to config defaults.
/// </summary>
[ApiController]
[ApiResultFilter]
public abstract class PaymentControllerBase : Controller
{
    private ICurrentActor? _currentActor;
    private IConfiguration? _configuration;

    private ICurrentActor CurrentActor => _currentActor ??= HttpContext.RequestServices.GetRequiredService<ICurrentActor>();
    private IConfiguration Configuration => _configuration ??= HttpContext.RequestServices.GetRequiredService<IConfiguration>();

    /// <summary>
    /// Whether the current request has an authenticated user.
    /// </summary>
    protected bool IsAuthenticated => CurrentActor.IsAuthenticated;

    /// <summary>
    /// Gets the tenant scope from X-Tenant-Scope header or query param.
    /// Returns "all" only if the actor has permission to view all tenants.
    /// </summary>
    protected string? TenantScope
    {
        get
        {
            var headerScope = Request.Headers["X-Tenant-Scope"].FirstOrDefault();
            if (!string.IsNullOrEmpty(headerScope))
                return headerScope;

            var queryScope = Request.Query["tenant_scope"].FirstOrDefault();
            if (!string.IsNullOrEmpty(queryScope))
                return queryScope;

            return null;
        }
    }

    /// <summary>
    /// Gets the current tenant ID from claims, header, or standalone config fallback.
    /// </summary>
    protected Guid TenantId
    {
        get
        {
            // Try header first
            var headerTenantId = Request.Headers["X-Tenant-Id"].FirstOrDefault();
            if (!string.IsNullOrEmpty(headerTenantId) && Guid.TryParse(headerTenantId, out var headerTid))
                return headerTid;

            // Try claims (auth mode)
            var claimTenantId = User.FindFirst("tenant_id")?.Value;
            if (!string.IsNullOrEmpty(claimTenantId) && Guid.TryParse(claimTenantId, out var claimTid))
                return claimTid;

            // Fallback to config default (standalone mode)
            var defaultTenantId = Configuration.GetValue<string>("Payment:Standalone:DefaultTenantId");
            if (!string.IsNullOrEmpty(defaultTenantId) && Guid.TryParse(defaultTenantId, out var defaultTid))
                return defaultTid;

            return Guid.Empty;
        }
    }

    /// <summary>
    /// Gets an effective tenant ID for list/report endpoints.
    /// Returns null when tenant_scope is "all" (admin wants cross-tenant view).
    /// Returns the resolved TenantId for single-tenant scope.
    /// The caller/service should skip tenant filtering when null is returned.
    /// </summary>
    protected Guid? GetEffectiveTenantId()
    {
        // If scope is "all", verify actor has permission to view all tenants
        if ("all".Equals(TenantScope, StringComparison.OrdinalIgnoreCase))
        {
            var canViewAll = User.HasClaim("permission", "payment.tenant.view_all")
                || User.HasClaim("permission", "payment.*")
                || User.IsInRole("super_admin");

            if (canViewAll)
                return null; // null = skip tenant filter

            // User requested "all" but lacks permission — reject explicitly
            throw new UnauthorizedException("tenant_scope_not_allowed: You are not allowed to view all tenants.");
        }

        // Single-tenant mode: use resolved tenant
        var tid = TenantId;
        return tid != Guid.Empty ? tid : null;
    }

    /// <summary>
    /// Gets the current actor user ID from claims or returns Guid.Empty in no-auth mode.
    /// </summary>
    protected Guid ActorUserId => CurrentActor.UserId ?? Guid.Empty;

    /// <summary>
    /// Gets the current user's ID as a raw string from the "sub" claim.
    /// </summary>
    protected string? CurrentUserIdString =>
        User.FindFirst("sub")?.Value;

    /// <summary>
    /// Gets the required actor user ID, throwing if missing.
    /// In no-auth mode (when auth is disabled), returns a system GUID instead of throwing.
    /// </summary>
    protected Guid GetRequiredActorUserId()
    {
        if (ActorUserId != Guid.Empty) return ActorUserId;
        if (!Configuration.GetValue<bool>("Auth:Enabled")) return Guid.Empty;
        throw new UnauthorizedException("User ID required");
    }

    /// <summary>
    /// Gets the required tenant ID, throwing if missing.
    /// In no-auth mode (when auth is disabled), uses config default.
    /// For list/report endpoints that support "all tenants", use GetEffectiveTenantId() instead.
    /// </summary>
    protected Guid GetRequiredTenantId()
    {
        // Reject "all" scope for mutation endpoints — tenant must be explicit
        if ("all".Equals(TenantScope, StringComparison.OrdinalIgnoreCase))
            throw new UnauthorizedException("tenant_scope_not_allowed: عملیات بر روی همه مستاجرها مجاز نیست. لطفاً یک مستاجر مشخص انتخاب کنید.");

        if (TenantId != Guid.Empty) return TenantId;
        throw new UnauthorizedException("Tenant context required");
    }

    /// <summary>
    /// Gets the application code from header, claims, or config default.
    /// </summary>
    protected string ApplicationCode
    {
        get
        {
            var header = Request.Headers["X-Application-Code"].FirstOrDefault();
            if (!string.IsNullOrEmpty(header)) return header;

            var claim = User.FindFirst("application_code")?.Value;
            if (!string.IsNullOrEmpty(claim)) return claim;

            return Configuration.GetValue<string>("Payment:Standalone:DefaultApplicationCode") ?? "payment";
        }
    }

    /// <summary>
    /// Gets the idempotency key from the X-Idempotency-Key request header.
    /// </summary>
    protected string? IdempotencyKey =>
        Request.Headers["X-Idempotency-Key"].FirstOrDefault();

    /// <summary>
    /// Gets the correlation ID from the X-Correlation-Id request header.
    /// </summary>
    protected string? CorrelationId =>
        Request.Headers["X-Correlation-Id"].FirstOrDefault();
}
