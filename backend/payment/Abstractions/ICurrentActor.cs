using payment.Options;

namespace payment.Abstractions;

/// <summary>
/// Unified actor context for the current request.
/// Services should use this instead of directly reading claims or headers.
/// Works in both auth-enabled and auth-disabled modes.
/// </summary>
public interface ICurrentActor
{
    /// <summary>Whether the current request has an authenticated user.</summary>
    bool IsAuthenticated { get; }

    /// <summary>Current user ID, or null if not authenticated / anonymous.</summary>
    Guid? UserId { get; }

    /// <summary>Current user email, or null if not available.</summary>
    string? Email { get; }

    /// <summary>Roles assigned to the current actor.</summary>
    IReadOnlyCollection<string> Roles { get; }

    /// <summary>Permissions assigned to the current actor.</summary>
    IReadOnlyCollection<string> Permissions { get; }

    /// <summary>Auth mode for this request: None or External.</summary>
    AuthMode AuthMode { get; }
}

/// <summary>
/// Default implementation of ICurrentActor for when auth is disabled.
/// </summary>
public class AnonymousActor : ICurrentActor
{
    public bool IsAuthenticated => false;
    public Guid? UserId => null;
    public string? Email => null;
    public IReadOnlyCollection<string> Roles => Array.Empty<string>();
    public IReadOnlyCollection<string> Permissions => Array.Empty<string>();
    public AuthMode AuthMode => AuthMode.None;
}

/// <summary>
/// Implementation of ICurrentActor backed by JWT claims from the current HttpContext.
/// </summary>
public class ClaimsActor : ICurrentActor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ClaimsActor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private System.Security.Claims.ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public Guid? UserId
    {
        get
        {
            var sub = User?.FindFirst("sub")?.Value;
            if (sub != null && Guid.TryParse(sub, out var id)) return id;
            var uid = User?.FindFirst("uid")?.Value;
            if (uid != null && Guid.TryParse(uid, out var uid2)) return uid2;
            return null;
        }
    }

    public string? Email => User?.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
        ?? User?.FindFirst("email")?.Value;

    public IReadOnlyCollection<string> Roles
    {
        get
        {
            if (User == null) return Array.Empty<string>();
            return User.FindAll(System.Security.Claims.ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();
        }
    }

    public IReadOnlyCollection<string> Permissions
    {
        get
        {
            if (User == null) return Array.Empty<string>();
            return User.FindAll("permission")
                .Select(c => c.Value)
                .ToList();
        }
    }

    public AuthMode AuthMode => AuthMode.External;
}
