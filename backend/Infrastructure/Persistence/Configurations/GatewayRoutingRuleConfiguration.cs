using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class GatewayRoutingRuleConfiguration : IEntityTypeConfiguration<GatewayRoutingRule>
{
    public void Configure(EntityTypeBuilder<GatewayRoutingRule> entity)
    {
        entity.ToTable("GatewayRoutingRules");
        entity.HasKey(r => r.Id);
        entity.Property(r => r.Currency).HasMaxLength(10);
        entity.Property(r => r.MinAmount).HasPrecision(30, 10);
        entity.Property(r => r.MaxAmount).HasPrecision(30, 10);

        entity.HasIndex(r => new { r.PolicyId, r.Status, r.Priority });

        entity.Ignore(r => r.IsActive);
    }
}
