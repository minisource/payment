using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class PaymentLimitUsageConfiguration : IEntityTypeConfiguration<PaymentLimitUsage>
{
    public void Configure(EntityTypeBuilder<PaymentLimitUsage> entity)
    {
        entity.ToTable("PaymentLimitUsages");
        entity.HasKey(u => u.Id);
        entity.Property(u => u.AmountTotal).HasPrecision(30, 10);
        entity.HasIndex(u => new { u.TenantId, u.OwnerType, u.OwnerId, u.OperationType, u.Currency, u.WindowType, u.WindowStart }).IsUnique();
    }
}
