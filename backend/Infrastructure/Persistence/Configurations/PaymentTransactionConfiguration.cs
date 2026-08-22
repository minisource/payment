using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class PaymentTransactionConfiguration : IEntityTypeConfiguration<PaymentTransaction>
{
    public void Configure(EntityTypeBuilder<PaymentTransaction> entity)
    {
        entity.ToTable("PaymentTransactions");
        entity.HasKey(t => t.Id);

        entity.Property(t => t.Amount).HasPrecision(30, 10).IsRequired();
        entity.Property(t => t.Currency).HasMaxLength(10).IsRequired();
        entity.Property(t => t.ProviderCode).HasMaxLength(100).IsRequired();
        entity.Property(t => t.AdapterType).HasMaxLength(50);
        entity.Property(t => t.ApplicationCode).HasMaxLength(100);
        entity.Property(t => t.Authority).HasMaxLength(500);
        entity.Property(t => t.GatewayReferenceId).HasMaxLength(500);
        entity.Property(t => t.TraceNumber).HasMaxLength(200);
        entity.Property(t => t.Rrn).HasMaxLength(200);
        entity.Property(t => t.CardPanMasked).HasMaxLength(50);
        entity.Property(t => t.GatewayResponseCode).HasMaxLength(50);
        entity.Property(t => t.IdempotencyKey).HasMaxLength(200);

        entity.HasIndex(t => new { t.PaymentIntentId, t.CreatedAt }).IsDescending(false, true);
        entity.HasIndex(t => new { t.TenantId, t.CreatedAt }).IsDescending(false, true);
        entity.HasIndex(t => new { t.TenantId, t.Status });
        entity.HasIndex(t => t.Authority);
        entity.HasIndex(t => new { t.TenantId, t.IdempotencyKey }).IsUnique()
            .HasFilter("\"IdempotencyKey\" IS NOT NULL");

        entity.HasOne(t => t.GatewayConfig)
            .WithMany()
            .HasForeignKey(t => t.GatewayConfigId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
