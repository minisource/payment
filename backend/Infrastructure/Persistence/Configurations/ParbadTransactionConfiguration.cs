using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ParbadTransactionEntity = Parbad.Storage.Abstractions.Models.Transaction;

namespace Infrastructure.Persistence.Configurations;

public class ParbadTransactionConfiguration : IEntityTypeConfiguration<ParbadTransactionEntity>
{
    public void Configure(EntityTypeBuilder<ParbadTransactionEntity> entity)
    {
        entity.ToTable("ParbadTransactions");
        entity.HasKey(t => t.Id);

        entity.Property(t => t.Amount)
            .HasPrecision(18, 2);

        entity.HasOne<Parbad.Storage.Abstractions.Models.Payment>()
            .WithMany()
            .HasForeignKey(t => t.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
