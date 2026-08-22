using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class RiskRuleConfiguration : IEntityTypeConfiguration<RiskRule>
{
    public void Configure(EntityTypeBuilder<RiskRule> entity)
    {
        entity.ToTable("RiskRules");
        entity.HasKey(r => r.Id);
        entity.Property(r => r.ThresholdAmount).HasPrecision(30, 10);
        entity.HasIndex(r => new { r.TenantId, r.Status });
        entity.Ignore(r => r.IsActive);
    }
}
