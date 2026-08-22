using Microsoft.AspNetCore.Mvc;
using payment.Options;

namespace payment.Controllers.Public;

/// <summary>
/// Public configuration endpoint for frontend discovery.
/// Exposes auth mode, tenant mode, supported languages, and defaults without secrets.
/// </summary>
[ApiController]
[Route("v1/system")]
public class PublicConfigController : Controller
{
    private readonly IConfiguration _configuration;

    public PublicConfigController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Returns public (frontend-safe) configuration for the Payment service.
    /// No secrets, signing keys, or client secrets are exposed.
    /// </summary>
    [HttpGet("public-config")]
    [ProducesResponseType(typeof(PublicConfigResponse), StatusCodes.Status200OK)]
    public ActionResult<PublicConfigResponse> GetPublicConfig()
    {
        var authEnabled = _configuration.GetValue<bool>("Auth:Enabled");
        var authMode = _configuration.GetValue<string>("Auth:Mode") ?? "None";
        var requireAdminAuth = _configuration.GetValue<bool>("Auth:RequireAdminAuth");
        var requireApiAuth = _configuration.GetValue<bool>("Auth:RequireApiAuth");
        var standaloneEnabled = _configuration.GetValue<bool>("Payment:Standalone:Enabled");
        var defaultTenantId = _configuration.GetValue<string>("Payment:Standalone:DefaultTenantId");
        var defaultAppCode = _configuration.GetValue<string>("Payment:Standalone:DefaultApplicationCode") ?? "payment";
        var tenantMode = _configuration.GetValue<string>("Payment:Tenant:Mode") ?? (standaloneEnabled ? "single" : "multi");
        var showTenantSwitcher = _configuration.GetValue<bool?>("Payment:Tenant:ShowTenantSwitcher") ?? (!standaloneEnabled);
        var allowAllTenants = _configuration.GetValue<bool?>("Payment:Tenant:AllowAllTenantsFilter") ?? (!standaloneEnabled);
        var externalBaseUrl = _configuration.GetValue<string>("Auth:External:BaseUrl") ?? "";
        var externalFrontUrl = _configuration.GetValue<string>("Auth:External:FrontUrl") ?? "";

        var response = new PublicConfigResponse
        {
            Service = "payment",
            StandaloneEnabled = standaloneEnabled,
            Auth = new AuthConfigResponse
            {
                Enabled = authEnabled,
                Mode = authMode,
                RequireAdminAuth = requireAdminAuth,
                RequireApiAuth = requireApiAuth,
                ExternalAuthBaseUrl = externalBaseUrl,
                ExternalAuthFrontUrl = externalFrontUrl,
            },
            DefaultTenantId = defaultTenantId ?? "00000000-0000-0000-0000-000000000001",
            DefaultApplicationCode = defaultAppCode,
            DefaultLanguage = "fa",
            SupportedLanguages = new[] { "fa", "en" },
            Tenant = new TenantConfigResponse
            {
                Enabled = tenantMode != "single" || standaloneEnabled,
                Mode = tenantMode,
                ShowTenantSwitcher = showTenantSwitcher,
                AllowAllTenantsFilter = allowAllTenants,
            },
        };

        return Ok(response);
    }
}

/// <summary>
/// Public configuration response. No secrets included.
/// </summary>
public class PublicConfigResponse
{
    public string Service { get; set; } = "payment";
    public bool StandaloneEnabled { get; set; }
    public AuthConfigResponse Auth { get; set; } = new();
    public TenantConfigResponse Tenant { get; set; } = new();
    public string DefaultTenantId { get; set; } = "";
    public string DefaultApplicationCode { get; set; } = "payment";
    public string DefaultLanguage { get; set; } = "fa";
    public string[] SupportedLanguages { get; set; } = { "fa", "en" };
}

/// <summary>
/// Auth section of public config. No secrets exposed.
/// </summary>
public class AuthConfigResponse
{
    public bool Enabled { get; set; }
    public string Mode { get; set; } = "None";
    public bool RequireAdminAuth { get; set; }
    public bool RequireApiAuth { get; set; }
    public string ExternalAuthBaseUrl { get; set; } = "";
    public string ExternalAuthFrontUrl { get; set; } = "";
}

/// <summary>
/// Tenant section of public config. No secrets.
/// </summary>
public class TenantConfigResponse
{
    public bool Enabled { get; set; }
    public string Mode { get; set; } = "single";
    public bool ShowTenantSwitcher { get; set; }
    public bool AllowAllTenantsFilter { get; set; }
}
