using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class GatewayConfigConfiguration : IEntityTypeConfiguration<GatewayConfig>
{
    public void Configure(EntityTypeBuilder<GatewayConfig> entity)
    {
        entity.ToTable("GatewayConfigs");
        entity.HasKey(c => c.Id);
        entity.Property(c => c.ProviderCode).HasMaxLength(100).IsRequired();
        entity.Property(c => c.AdapterType).HasMaxLength(50);
        entity.Property(c => c.Name).HasMaxLength(200).IsRequired();
        entity.Property(c => c.Environment).HasMaxLength(50);
        entity.Property(c => c.ApplicationCode).HasMaxLength(100);
        entity.Property(c => c.MinAmount).HasPrecision(30, 10);
        entity.Property(c => c.MaxAmount).HasPrecision(30, 10);
        entity.Property(c => c.CallbackBaseUrl).HasMaxLength(500);

        entity.HasIndex(c => new { c.TenantId, c.ApplicationCode, c.Status });
        entity.HasIndex(c => new { c.ProviderCode, c.Status });
        entity.HasIndex(c => new { c.TenantId, c.ApplicationCode, c.Priority });

        entity.Ignore(c => c.Scope);
        entity.Ignore(c => c.IsUsable);
    }
}
