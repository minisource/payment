using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class LimitRuleConfiguration : IEntityTypeConfiguration<LimitRule>
{
    public void Configure(EntityTypeBuilder<LimitRule> entity)
    {
        entity.ToTable("LimitRules");
        entity.HasKey(r => r.Id);
        entity.Property(r => r.MinAmount).HasPrecision(30, 10);
        entity.Property(r => r.MaxAmount).HasPrecision(30, 10);
        entity.Property(r => r.DailyAmountLimit).HasPrecision(30, 10);
        entity.Property(r => r.MonthlyAmountLimit).HasPrecision(30, 10);
        entity.HasIndex(r => new { r.TenantId, r.Status });
        entity.Ignore(r => r.IsActive);
    }
}
