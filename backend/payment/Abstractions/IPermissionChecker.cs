namespace payment.Abstractions;

/// <summary>
/// Centralized permission checker.
/// When auth is enabled, permissions are validated from the token/auth service.
/// When auth is disabled and RequireAdminAuth is false, admin permissions are bypassed.
/// </summary>
public interface IPermissionChecker
{
    /// <summary>Check if the current actor has a specific permission.</summary>
    Task<bool> HasPermissionAsync(string permission, CancellationToken cancellationToken = default);

    /// <summary>Check if the current actor has any of the specified permissions.</summary>
    Task<bool> HasAnyPermissionAsync(IEnumerable<string> permissions, CancellationToken cancellationToken = default);

    /// <summary>Check if the current actor has all of the specified permissions.</summary>
    Task<bool> HasAllPermissionsAsync(IEnumerable<string> permissions, CancellationToken cancellationToken = default);
}

/// <summary>
/// Default implementation that reads permissions from the current actor context.
/// In auth-disabled mode without RequireAdminAuth, all permission checks pass.
/// </summary>
public class DefaultPermissionChecker : IPermissionChecker
{
    private readonly ICurrentActor _currentActor;
    private readonly IConfiguration _configuration;

    public DefaultPermissionChecker(ICurrentActor currentActor, IConfiguration configuration)
    {
        _currentActor = currentActor;
        _configuration = configuration;
    }

    private bool IsAuthDisabled()
    {
        var authEnabled = _configuration.GetValue<bool>("Auth:Enabled");
        var requireAdminAuth = _configuration.GetValue<bool>("Auth:RequireAdminAuth");
        return !authEnabled && !requireAdminAuth;
    }

    public Task<bool> HasPermissionAsync(string permission, CancellationToken cancellationToken = default)
    {
        if (IsAuthDisabled()) return Task.FromResult(true);
        return Task.FromResult(_currentActor.Permissions.Contains(permission) || _currentActor.Roles.Contains("super_admin"));
    }

    public Task<bool> HasAnyPermissionAsync(IEnumerable<string> permissions, CancellationToken cancellationToken = default)
    {
        if (IsAuthDisabled()) return Task.FromResult(true);
        if (_currentActor.Roles.Contains("super_admin")) return Task.FromResult(true);
        return Task.FromResult(permissions.Any(p => _currentActor.Permissions.Contains(p)));
    }

    public Task<bool> HasAllPermissionsAsync(IEnumerable<string> permissions, CancellationToken cancellationToken = default)
    {
        if (IsAuthDisabled()) return Task.FromResult(true);
        if (_currentActor.Roles.Contains("super_admin")) return Task.FromResult(true);
        return Task.FromResult(permissions.All(p => _currentActor.Permissions.Contains(p)));
    }
}
