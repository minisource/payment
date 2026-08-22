using Domain.Enums;
using Minisource.Common.Domain;

namespace Domain.Entities;

/// <summary>
/// Payment gateway provider catalog entry. Defines what config fields a provider needs.
/// </summary>
public class GatewayProvider : Entity<Guid>
{
    public string Code { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string AdapterType { get; set; } = "parbad";
    public GatewayProviderStatus Status { get; set; } = GatewayProviderStatus.Active;
    public List<string> SupportedCurrencies { get; set; } = ["IRT"];
    public List<string> SupportedOperations { get; set; } = ["payment", "verify", "refund"];
    public string RequiredConfigSchema { get; set; } = "{}";  // JSON
    public string? OptionalConfigSchema { get; set; }           // JSON
    public string? Metadata { get; set; }

    public GatewayProvider() { Id = Guid.NewGuid(); }
}
