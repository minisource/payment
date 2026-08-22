using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class BankStatementEntryConfiguration : IEntityTypeConfiguration<BankStatementEntry>
{
    public void Configure(EntityTypeBuilder<BankStatementEntry> entity)
    {
        entity.ToTable("BankStatementEntries");
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Amount).HasPrecision(30, 10);
        entity.HasIndex(e => new { e.TenantId, e.TransactionDate });
    }
}
