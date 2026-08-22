using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class GatewayProviderConfiguration : IEntityTypeConfiguration<GatewayProvider>
{
    public void Configure(EntityTypeBuilder<GatewayProvider> entity)
    {
        entity.ToTable("GatewayProviders");
        entity.HasKey(p => p.Id);
        entity.Property(p => p.Code).HasMaxLength(100).IsRequired();
        entity.HasIndex(p => p.Code).IsUnique();
        entity.Property(p => p.DisplayName).HasMaxLength(200).IsRequired();
        entity.Property(p => p.AdapterType).HasMaxLength(50);
    }
}
