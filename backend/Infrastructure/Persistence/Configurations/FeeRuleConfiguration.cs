using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class FeeRuleConfiguration : IEntityTypeConfiguration<FeeRule>
{
    public void Configure(EntityTypeBuilder<FeeRule> entity)
    {
        entity.ToTable("FeeRules");
        entity.HasKey(r => r.Id);
        entity.Property(r => r.FixedAmount).HasPrecision(30, 10);
        entity.Property(r => r.PercentRate).HasPrecision(18, 8);
        entity.Property(r => r.MinFee).HasPrecision(30, 10);
        entity.Property(r => r.MaxFee).HasPrecision(30, 10);
        entity.HasIndex(r => new { r.TenantId, r.Status });
        entity.Ignore(r => r.IsActive);
    }
}
