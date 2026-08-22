using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class PaymentIntentConfiguration : IEntityTypeConfiguration<PaymentIntent>
{
    public void Configure(EntityTypeBuilder<PaymentIntent> entity)
    {
        entity.ToTable("PaymentIntents");
        entity.HasKey(p => p.Id);

        entity.Property(p => p.Amount).HasPrecision(30, 10).IsRequired();
        entity.Property(p => p.Currency).HasMaxLength(10).IsRequired();
        entity.Property(p => p.GrossAmount).HasPrecision(30, 10);
        entity.Property(p => p.FeeAmount).HasPrecision(30, 10);
        entity.Property(p => p.NetAmount).HasPrecision(30, 10);
        entity.Property(p => p.ExternalReferenceType).HasMaxLength(100);
        entity.Property(p => p.ExternalReferenceId).HasMaxLength(200);
        entity.Property(p => p.IdempotencyKey).HasMaxLength(200);
        entity.Property(p => p.ApplicationCode).HasMaxLength(100);
        entity.Property(p => p.WalletPostingStatus).HasMaxLength(50);

        entity.HasIndex(p => new { p.TenantId, p.CreatedAt }).IsDescending(false, true);
        entity.HasIndex(p => new { p.TenantId, p.Status });
        entity.HasIndex(p => new { p.TenantId, p.ExternalReferenceType, p.ExternalReferenceId });
        entity.HasIndex(p => new { p.TenantId, p.IdempotencyKey }).IsUnique()
            .HasFilter("\"IdempotencyKey\" IS NOT NULL");

        entity.HasMany(p => p.Transactions)
            .WithOne(t => t.PaymentIntent)
            .HasForeignKey(t => t.PaymentIntentId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.Ignore(p => p.DomainEvents);
        entity.Ignore(p => p.CanStart);
        entity.Ignore(p => p.IsExpired);
    }
}
