using Domain.Enums;
using Minisource.Common.Domain;

namespace Domain.Entities;

/// <summary>
/// Routing policy that determines which gateway config to select.
/// </summary>
public class GatewayRoutingPolicy : Entity<Guid>
{
    public Guid? TenantId { get; set; }
    public string? ApplicationCode { get; set; }

    public string Name { get; set; } = string.Empty;
    public GatewayRoutingStrategy Strategy { get; set; } = GatewayRoutingStrategy.Priority;

    public bool FallbackEnabled { get; set; } = true;
    public bool RandomizeSamePriority { get; set; }

    public string Status { get; set; } = "active";

    public string? Metadata { get; set; }

    public DateTime? DeletedAt { get; set; }

    private readonly List<GatewayRoutingRule> _rules = [];
    public IReadOnlyCollection<GatewayRoutingRule> Rules => _rules.AsReadOnly();

    public GatewayRoutingPolicy() { Id = Guid.NewGuid(); }
    public bool IsActive => Status == "active" && DeletedAt == null;

    public void AddRule(GatewayRoutingRule rule) => _rules.Add(rule);
}

/// <summary>
/// Individual routing rule linking a policy to a specific gateway config.
/// </summary>
public class GatewayRoutingRule : Entity<Guid>
{
    public Guid PolicyId { get; set; }
    public Guid GatewayConfigId { get; set; }

    public int Priority { get; set; } = 100;
    public int Weight { get; set; } = 1;

    public string? Currency { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }

    public string Status { get; set; } = "active";

    public string? Metadata { get; set; }

    public DateTime? DeletedAt { get; set; }

    public GatewayRoutingRule() { Id = Guid.NewGuid(); }
    public bool IsActive => Status == "active" && DeletedAt == null;
}
