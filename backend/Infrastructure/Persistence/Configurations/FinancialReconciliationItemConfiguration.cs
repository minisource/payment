using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class FinancialReconciliationItemConfiguration : IEntityTypeConfiguration<FinancialReconciliationItem>
{
    public void Configure(EntityTypeBuilder<FinancialReconciliationItem> entity)
    {
        entity.ToTable("ReconciliationItems");
        entity.HasKey(i => i.Id);
        entity.HasIndex(i => new { i.BatchId, i.Status });
    }
}
