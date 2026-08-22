using Domain.Enums;
using Minisource.Common.Domain;

namespace Domain.Entities;

/// <summary>
/// Gateway configuration. Supports global, tenant, and application scopes.
/// Secrets stored encrypted; non-sensitive config stored as JSON.
/// </summary>
public class GatewayConfig : Entity<Guid>
{
    public Guid? TenantId { get; set; }
    public string? ApplicationCode { get; set; }

    public string ProviderCode { get; set; } = string.Empty;
    public string AdapterType { get; set; } = "parbad";

    public string Name { get; set; } = string.Empty;
    public string Environment { get; set; } = "production";

    public GatewayConfigStatus Status { get; set; } = GatewayConfigStatus.Active;
    public bool IsDefault { get; set; }

    public int Priority { get; set; } = 100;
    public int Weight { get; set; } = 1;

    public List<string> SupportedCurrencies { get; set; } = ["IRT"];
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }

    public string ConfigJson { get; set; } = "{}";
    public string? EncryptedSecrets { get; set; }

    public string? CallbackBaseUrl { get; set; }

    public GatewayHealthStatus HealthStatus { get; set; } = GatewayHealthStatus.Unknown;
    public DateTime? LastSuccessAt { get; set; }
    public DateTime? LastFailureAt { get; set; }
    public int FailureCount { get; set; }

    public string? Metadata { get; set; }

    public DateTime? DisabledAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public GatewayConfigScope Scope
    {
        get
        {
            if (TenantId.HasValue && !string.IsNullOrEmpty(ApplicationCode)) return GatewayConfigScope.TenantApplication;
            if (TenantId.HasValue) return GatewayConfigScope.Tenant;
            if (!string.IsNullOrEmpty(ApplicationCode)) return GatewayConfigScope.Application;
            return GatewayConfigScope.Global;
        }
    }

    public GatewayConfig() { Id = Guid.NewGuid(); }

    public bool IsUsable => Status == GatewayConfigStatus.Active
        && DeletedAt == null;

    public void Enable()
    {
        Status = GatewayConfigStatus.Active;
        DisabledAt = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Disable()
    {
        Status = GatewayConfigStatus.Disabled;
        DisabledAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SoftDelete()
    {
        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkSuccess()
    {
        LastSuccessAt = DateTime.UtcNow;
        FailureCount = 0;
        HealthStatus = GatewayHealthStatus.Healthy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkFailure()
    {
        LastFailureAt = DateTime.UtcNow;
        FailureCount++;
        HealthStatus = FailureCount >= 3 ? GatewayHealthStatus.Unhealthy
            : FailureCount >= 1 ? GatewayHealthStatus.Degraded
            : GatewayHealthStatus.Healthy;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool AcceptsAmount(decimal amount)
    {
        if (MinAmount.HasValue && amount < MinAmount.Value) return false;
        if (MaxAmount.HasValue && amount > MaxAmount.Value) return false;
        return true;
    }

    public bool AcceptsCurrency(string currency)
        => SupportedCurrencies.Contains(currency, StringComparer.OrdinalIgnoreCase);
}
