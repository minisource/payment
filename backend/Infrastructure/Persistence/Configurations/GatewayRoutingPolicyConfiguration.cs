using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class GatewayRoutingPolicyConfiguration : IEntityTypeConfiguration<GatewayRoutingPolicy>
{
    public void Configure(EntityTypeBuilder<GatewayRoutingPolicy> entity)
    {
        entity.ToTable("GatewayRoutingPolicies");
        entity.HasKey(p => p.Id);
        entity.Property(p => p.Name).HasMaxLength(200).IsRequired();
        entity.Property(p => p.ApplicationCode).HasMaxLength(100);

        entity.HasIndex(p => new { p.TenantId, p.ApplicationCode, p.Status });

        entity.HasMany(p => p.Rules)
            .WithOne()
            .HasForeignKey(r => r.PolicyId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.Ignore(p => p.IsActive);
    }
}
