using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class RefundRequestConfiguration : IEntityTypeConfiguration<RefundRequest>
{
    public void Configure(EntityTypeBuilder<RefundRequest> entity)
    {
        entity.ToTable("RefundRequests");
        entity.HasKey(r => r.Id);

        entity.Property(r => r.Amount).HasPrecision(30, 10).IsRequired();
        entity.Property(r => r.Currency).HasMaxLength(10).IsRequired();
        entity.Property(r => r.Reason).HasMaxLength(1000).IsRequired();
        entity.Property(r => r.AdminNote).HasMaxLength(1000);
        entity.Property(r => r.RejectReason).HasMaxLength(1000);
        entity.Property(r => r.ProviderCode).HasMaxLength(100).IsRequired();
        entity.Property(r => r.GatewayName).HasMaxLength(200);
        entity.Property(r => r.RequestSource).HasMaxLength(20).IsRequired();
        entity.Property(r => r.RequestedByUserId).HasMaxLength(200).IsRequired();
        entity.Property(r => r.RequestedByAdminId).HasMaxLength(200);
        entity.Property(r => r.ApprovedByUserId).HasMaxLength(200);
        entity.Property(r => r.ProcessedByUserId).HasMaxLength(200);
        entity.Property(r => r.ApplicationCode).HasMaxLength(100);
        entity.Property(r => r.IdempotencyKey).HasMaxLength(200);
        entity.Property(r => r.CorrelationId).HasMaxLength(200);
        entity.Property(r => r.RequestId).HasMaxLength(200);
        entity.Property(r => r.FailureCode).HasMaxLength(100);
        entity.Property(r => r.FailureMessage).HasMaxLength(1000);
        entity.Property(r => r.GatewayRefundId).HasMaxLength(200);
        entity.Property(r => r.GatewayTrackingCode).HasMaxLength(200);
        entity.Property(r => r.GatewayReferenceId).HasMaxLength(200);

        entity.HasIndex(r => new { r.TenantId, r.Status });
        entity.HasIndex(r => new { r.TenantId, r.CreatedAt }).IsDescending(false, true);
        entity.HasIndex(r => r.PaymentTransactionId);
        entity.HasIndex(r => r.WalletHoldId);
        entity.HasIndex(r => new { r.TenantId, r.IdempotencyKey }).IsUnique()
            .HasFilter("\"IdempotencyKey\" IS NOT NULL");

        // Children
        entity.HasMany(r => r.Attempts)
            .WithOne(a => a.RefundRequest)
            .HasForeignKey(a => a.RefundRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.Ignore(r => r.DomainEvents);
        entity.Ignore(r => r.CanProcess);
        entity.Ignore(r => r.CanRetry);
        entity.Ignore(r => r.IsTerminal);
        entity.Ignore(r => r.HasActiveHold);
    }
}
