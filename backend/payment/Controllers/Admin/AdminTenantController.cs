using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentaion.Middlewares;

namespace payment.Controllers;

/// <summary>
/// Read-only admin endpoint to list available tenants.
/// In auth mode, tenant list comes from claims/config.
/// In standalone mode, returns the default tenant only.
/// Payment does NOT implement tenant CRUD — that belongs to Auth/Core service.
/// </summary>
[Route("api/v1/admin")]
[Authorize(Policy = "WalletAdmin")]
public class AdminTenantController : PaymentControllerBase
{
    private readonly IConfiguration _configuration;

    public AdminTenantController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>List tenants available to the current admin.</summary>
    [HttpGet("tenants")]
    public ActionResult<TenantListResponse> ListTenants()
    {
        var standaloneEnabled = _configuration.GetValue<bool>("Payment:Standalone:Enabled");
        var defaultTenantId = _configuration.GetValue<string>("Payment:Standalone:DefaultTenantId") ?? "00000000-0000-0000-0000-000000000001";
        var defaultTenantName = _configuration.GetValue<string>("Payment:Standalone:DefaultTenantName") ?? "Default Tenant";
        var allowAllTenants = _configuration.GetValue<bool?>("Payment:Tenant:AllowAllTenantsFilter") ?? (!standaloneEnabled);

        // In standalone mode, only the default tenant exists
        if (standaloneEnabled)
        {
            return Ok(new TenantListResponse
            {
                Items = new[]
                {
                    new TenantDto
                    {
                        Id = defaultTenantId,
                        Name = defaultTenantName,
                        Code = "default",
                        IsDefault = true,
                    },
                },
                CanViewAll = false,
            });
        }

        // In auth mode, build list from claims or config
        var tenants = new List<TenantDto>();

        // Try getting tenant from JWT claim (most common auth scenario)
        var claimTenantId = User.FindFirst("tenant_id")?.Value;
        var claimTenantName = User.FindFirst("tenant_name")?.Value ?? "Current Tenant";
        if (!string.IsNullOrEmpty(claimTenantId))
        {
            tenants.Add(new TenantDto
            {
                Id = claimTenantId,
                Name = claimTenantName,
                Code = "current",
                IsDefault = true,
            });
        }

        // Check for super-admin / view_all permission
        var canViewAll = allowAllTenants
            && (User.IsInRole("super_admin") || User.HasClaim("permission", "payment.tenant.view_all") || User.HasClaim("permission", "payment.*"));

        // For now, if no tenants found, add default placeholder
        if (tenants.Count == 0)
        {
            tenants.Add(new TenantDto
            {
                Id = defaultTenantId,
                Name = "Default Tenant",
                Code = "default",
                IsDefault = true,
            });
        }

        return Ok(new TenantListResponse
        {
            Items = tenants,
            CanViewAll = canViewAll,
        });
    }
}

public class TenantListResponse
{
    public IReadOnlyList<TenantDto> Items { get; set; } = Array.Empty<TenantDto>();
    public bool CanViewAll { get; set; }
}

public class TenantDto
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Code { get; set; } = "";
    public bool IsDefault { get; set; }
}
