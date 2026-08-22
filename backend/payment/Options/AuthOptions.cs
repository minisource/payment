namespace payment.Options;

/// <summary>
/// Authentication mode for the Payment service.
/// Only None and External modes are supported. No LocalAdmin mode.
/// </summary>
public enum AuthMode
{
    /// <summary>No authentication required. Safe only for local/dev/internal/private deployments.</summary>
    None = 0,
    /// <summary>Authentication via an external Auth service (token introspection / JWT validation).</summary>
    External = 1
}

/// <summary>
/// Configuration options for optional authentication in the Payment service.
/// </summary>
public class AuthOptions
{
    public const string SectionName = "Auth";

    /// <summary>Whether authentication is enabled.</summary>
    public bool Enabled { get; set; } = false;

    /// <summary>Authentication mode: None or External.</summary>
    public AuthMode Mode { get; set; } = AuthMode.None;

    /// <summary>Whether admin endpoints require authentication.</summary>
    public bool RequireAdminAuth { get; set; } = false;

    /// <summary>Whether public API endpoints require authentication.</summary>
    public bool RequireApiAuth { get; set; } = false;

    /// <summary>External auth service configuration.</summary>
    public ExternalAuthOptions External { get; set; } = new();
}

/// <summary>
/// External auth service connection settings.
/// </summary>
public class ExternalAuthOptions
{
    /// <summary>Auth service base URL for token introspection.</summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>OAuth authority URL.</summary>
    public string Authority { get; set; } = string.Empty;

    /// <summary>Expected audience for token validation.</summary>
    public string Audience { get; set; } = "payment";

    /// <summary>Client ID for service-to-service auth.</summary>
    public string ClientId { get; set; } = "payment-admin";

    /// <summary>Whether to validate the token issuer.</summary>
    public bool ValidateIssuer { get; set; } = false;

    /// <summary>Whether to validate the token audience.</summary>
    public bool ValidateAudience { get; set; } = false;

    /// <summary>Whether to validate token lifetime.</summary>
    public bool ValidateLifetime { get; set; } = true;
}
