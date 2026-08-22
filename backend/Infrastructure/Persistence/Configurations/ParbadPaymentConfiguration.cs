using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ParbadPaymentEntity = Parbad.Storage.Abstractions.Models.Payment;

namespace Infrastructure.Persistence.Configurations;

public class ParbadPaymentConfiguration : IEntityTypeConfiguration<ParbadPaymentEntity>
{
    public void Configure(EntityTypeBuilder<ParbadPaymentEntity> entity)
    {
        entity.ToTable("ParbadPayments");
        entity.HasKey(p => p.Id);

        entity.Property(p => p.Amount)
            .HasPrecision(18, 2);
    }
}
