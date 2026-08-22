using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class PayoutRecordConfiguration : IEntityTypeConfiguration<PayoutRecord>
{
    public void Configure(EntityTypeBuilder<PayoutRecord> entity)
    {
        entity.ToTable("PayoutRecords");
        entity.HasKey(p => p.Id);
        entity.Property(p => p.Amount).HasPrecision(30, 10).IsRequired();
        entity.Property(p => p.Currency).HasMaxLength(10).IsRequired();
        entity.Property(p => p.FeeAmount).HasPrecision(30, 10);
        entity.Property(p => p.NetAmount).HasPrecision(30, 10);
        entity.Property(p => p.PayoutMethod).HasMaxLength(50);
        entity.Property(p => p.BankTrackingNumber).HasMaxLength(200);
        entity.Property(p => p.BankReferenceId).HasMaxLength(200);
        entity.Property(p => p.ApplicationCode).HasMaxLength(100);
        entity.HasIndex(p => p.WithdrawalRequestId);
        entity.HasIndex(p => new { p.TenantId, p.Status, p.CreatedAt }).IsDescending(false, false, true);
    }
}
