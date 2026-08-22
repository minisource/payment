using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class PaymentLinkConfiguration : IEntityTypeConfiguration<PaymentLink>
{
    public void Configure(EntityTypeBuilder<PaymentLink> entity)
    {
        entity.ToTable("PaymentLinks");
        entity.HasKey(l => l.Id);
        entity.Property(l => l.TokenHash).HasMaxLength(128).IsRequired();
        entity.Property(l => l.PublicCode).HasMaxLength(50);
        entity.Property(l => l.Title).HasMaxLength(300).IsRequired();
        entity.Property(l => l.OwnerType).HasMaxLength(50).IsRequired();
        entity.Property(l => l.Currency).HasMaxLength(10).IsRequired();
        entity.Property(l => l.ApplicationCode).HasMaxLength(100);
        entity.Property(l => l.FixedAmount).HasPrecision(30, 10);
        entity.Property(l => l.SuggestedAmount).HasPrecision(30, 10);
        entity.Property(l => l.MinAmount).HasPrecision(30, 10);
        entity.Property(l => l.MaxAmount).HasPrecision(30, 10);
        entity.Property(l => l.TotalPaidAmount).HasPrecision(30, 10);
        entity.Property(l => l.ReturnUrl).HasMaxLength(500);

        entity.HasIndex(l => l.TokenHash).IsUnique();
        entity.HasIndex(l => new { l.TenantId, l.OwnerType, l.OwnerId });
        entity.HasIndex(l => new { l.TenantId, l.Status });
        entity.HasIndex(l => l.RecipientWalletId);
        entity.HasIndex(l => new { l.TenantId, l.CreatedAt }).IsDescending(false, true);

        entity.Ignore(l => l.DomainEvents);
        entity.Ignore(l => l.CanBePaid);
        entity.Ignore(l => l.IsExpired);
    }
}
