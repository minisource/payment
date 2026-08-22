using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentLogEntity = Domain.Entities.PaymentLog;

namespace Infrastructure.Persistence.Configurations;

public class PaymentLogConfiguration : IEntityTypeConfiguration<PaymentLogEntity>
{
    public void Configure(EntityTypeBuilder<PaymentLogEntity> entity)
    {
        entity.HasKey(l => l.Id);

        entity.Property(l => l.Action)
            .HasMaxLength(50);

        entity.Property(l => l.Actor)
            .HasMaxLength(100);

        entity.Property(l => l.IpAddress)
            .HasMaxLength(45);

        entity.HasIndex(l => l.Timestamp);
    }
}
