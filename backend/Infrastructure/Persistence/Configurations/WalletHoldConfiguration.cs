using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class WalletHoldConfiguration : IEntityTypeConfiguration<WalletHold>
{
    public void Configure(EntityTypeBuilder<WalletHold> entity)
    {
        entity.ToTable("WalletHolds");
        entity.HasKey(h => h.Id);

        entity.Property(h => h.Amount).HasPrecision(30, 10).IsRequired();
        entity.Property(h => h.Currency).HasMaxLength(10).IsRequired();
        entity.Property(h => h.ReferenceType).HasMaxLength(50).IsRequired();
        entity.Property(h => h.Reason).HasMaxLength(500).IsRequired();
        entity.Property(h => h.ApplicationCode).HasMaxLength(100);
        entity.Property(h => h.CreatedByUserId).HasMaxLength(200);
        entity.Property(h => h.MetadataJson);

        entity.HasIndex(h => new { h.TenantId, h.Status });
        entity.HasIndex(h => new { h.ReferenceType, h.ReferenceId });
        entity.HasIndex(h => h.WalletAccountId);
        entity.HasIndex(h => h.ExpiresAt).HasFilter("\"ExpiresAt\" IS NOT NULL");

        entity.Ignore(h => h.DomainEvents);
        entity.Ignore(h => h.IsActive);
    }
}
