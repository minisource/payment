using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class FinancialReconciliationBatchConfiguration : IEntityTypeConfiguration<FinancialReconciliationBatch>
{
    public void Configure(EntityTypeBuilder<FinancialReconciliationBatch> entity)
    {
        entity.ToTable("ReconciliationBatches");
        entity.HasKey(b => b.Id);
        entity.HasIndex(b => new { b.TenantId, b.ReconciliationType, b.CreatedAt });
    }
}
