using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WalletEntity = Domain.Entities.Wallet;

namespace Infrastructure.Persistence.Configurations;

public class WalletConfiguration : IEntityTypeConfiguration<WalletEntity>
{
    public void Configure(EntityTypeBuilder<WalletEntity> entity)
    {
        entity.HasKey(w => w.Id);

        entity.Property(w => w.Balance)
            .HasPrecision(18, 2);

        entity.Property(w => w.Currency)
            .HasMaxLength(3);

        entity.Property(w => w.UserId)
            .HasMaxLength(100);

        entity.HasIndex(w => w.UserId)
            .IsUnique();

        entity.HasMany(w => w.Transactions)
            .WithOne(t => t.Wallet)
            .HasForeignKey(t => t.WalletId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore domain events (not persisted)
        entity.Ignore(w => w.DomainEvents);
    }
}
