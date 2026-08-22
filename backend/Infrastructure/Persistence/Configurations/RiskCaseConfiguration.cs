using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class RiskCaseConfiguration : IEntityTypeConfiguration<RiskCase>
{
    public void Configure(EntityTypeBuilder<RiskCase> entity)
    {
        entity.ToTable("RiskCases");
        entity.HasKey(c => c.Id);
        entity.HasIndex(c => new { c.TenantId, c.Status, c.OpenedAt });
    }
}
