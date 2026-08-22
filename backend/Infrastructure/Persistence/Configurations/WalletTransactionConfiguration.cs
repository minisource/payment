using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WalletTransactionEntity = Domain.Entities.WalletTransaction;

namespace Infrastructure.Persistence.Configurations;

public class WalletTransactionConfiguration : IEntityTypeConfiguration<WalletTransactionEntity>
{
    public void Configure(EntityTypeBuilder<WalletTransactionEntity> entity)
    {
        entity.HasKey(t => t.Id);

        entity.Property(t => t.Amount)
            .HasPrecision(18, 2);

        entity.Property(t => t.BalanceAfter)
            .HasPrecision(18, 2);

        entity.Property(t => t.Description)
            .HasMaxLength(500);

        entity.Property(t => t.ReferenceId)
            .HasMaxLength(100);

        entity.Property(t => t.ReferenceType)
            .HasMaxLength(50);

        entity.Property(t => t.ReversalReason)
            .HasMaxLength(500);

        entity.HasIndex(t => t.WalletId);

        entity.HasIndex(t => new { t.ReferenceId, t.ReferenceType });

        entity.HasIndex(t => t.CreatedAt);
    }
}
