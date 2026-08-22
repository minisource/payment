using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class WalletConsistencyCheckConfiguration : IEntityTypeConfiguration<WalletConsistencyCheck>
{
    public void Configure(EntityTypeBuilder<WalletConsistencyCheck> entity)
    {
        entity.ToTable("WalletConsistencyChecks");
        entity.HasKey(c => c.Id);
        entity.HasIndex(c => new { c.TenantId, c.Status, c.StartedAt });
    }
}
