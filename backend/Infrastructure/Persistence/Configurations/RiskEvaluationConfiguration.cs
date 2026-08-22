using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class RiskEvaluationConfiguration : IEntityTypeConfiguration<RiskEvaluation>
{
    public void Configure(EntityTypeBuilder<RiskEvaluation> entity)
    {
        entity.ToTable("RiskEvaluations");
        entity.HasKey(e => e.Id);
        entity.HasIndex(e => new { e.TenantId, e.EntityType, e.EntityId });
    }
}
