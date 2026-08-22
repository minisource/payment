using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentEntity = Domain.Entities.Payment;

namespace Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<PaymentEntity>
{
    public void Configure(EntityTypeBuilder<PaymentEntity> entity)
    {
        entity.HasKey(p => p.Id);

        entity.Property(p => p.Amount)
            .HasPrecision(18, 2);

        entity.Property(p => p.CreditApplied)
            .HasPrecision(18, 2);

        entity.Property(p => p.AmountDue)
            .HasPrecision(18, 2);

        entity.Property(p => p.Currency)
            .HasMaxLength(3);

        entity.Property(p => p.Gateway)
            .HasMaxLength(50);

        entity.Property(p => p.TransactionReference)
            .HasMaxLength(100);

        entity.Property(p => p.IdempotencyKey)
            .HasMaxLength(100);

        entity.HasIndex(p => p.TrackingNumber)
            .IsUnique();

        entity.HasIndex(p => p.IdempotencyKey)
            .IsUnique()
            .HasFilter("\"IdempotencyKey\" IS NOT NULL");

        entity.HasIndex(p => p.UserId);

        entity.HasIndex(p => p.Status);

        entity.HasIndex(p => p.CreatedAt);

        entity.HasMany(p => p.Attempts)
            .WithOne(a => a.Payment)
            .HasForeignKey(a => a.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasMany(p => p.Logs)
            .WithOne(l => l.Payment)
            .HasForeignKey(l => l.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore domain events (not persisted)
        entity.Ignore(p => p.DomainEvents);
    }
}
