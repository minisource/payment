using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class WithdrawalRequestConfiguration : IEntityTypeConfiguration<WithdrawalRequest>
{
    public void Configure(EntityTypeBuilder<WithdrawalRequest> entity)
    {
        entity.ToTable("WithdrawalRequests");
        entity.HasKey(w => w.Id);
        entity.Property(w => w.Amount).HasPrecision(30, 10).IsRequired();
        entity.Property(w => w.Currency).HasMaxLength(10).IsRequired();
        entity.Property(w => w.FeeAmount).HasPrecision(30, 10);
        entity.Property(w => w.NetAmount).HasPrecision(30, 10);
        entity.Property(w => w.OwnerType).HasMaxLength(50).IsRequired();
        entity.Property(w => w.ApplicationCode).HasMaxLength(100);
        entity.Property(w => w.IdempotencyKey).HasMaxLength(200);
        entity.Property(w => w.BankTrackingNumber).HasMaxLength(200);
        entity.Property(w => w.BankReferenceId).HasMaxLength(200);
        entity.HasIndex(w => new { w.TenantId, w.Status, w.CreatedAt }).IsDescending(false, false, true);
        entity.HasIndex(w => new { w.WalletId, w.CreatedAt }).IsDescending(false, true);
        entity.HasIndex(w => new { w.TenantId, w.OwnerType, w.OwnerId, w.CreatedAt }).IsDescending(false, false, false, true);
        entity.HasIndex(w => new { w.TenantId, w.IdempotencyKey }).IsUnique()
            .HasFilter("\"IdempotencyKey\" IS NOT NULL");
        entity.HasOne(w => w.PayoutAccount).WithMany().HasForeignKey(w => w.PayoutAccountId).OnDelete(DeleteBehavior.Restrict);
        entity.Ignore(w => w.DomainEvents);
    }
}
