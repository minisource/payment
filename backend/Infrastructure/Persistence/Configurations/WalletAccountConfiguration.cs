using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class WalletAccountConfiguration : IEntityTypeConfiguration<WalletAccount>
{
    public void Configure(EntityTypeBuilder<WalletAccount> entity)
    {
        entity.ToTable("WalletAccounts");
        entity.HasKey(w => w.Id);

        entity.Property(w => w.AvailableBalance).HasPrecision(30, 10);
        entity.Property(w => w.LockedBalance).HasPrecision(30, 10);
        entity.Property(w => w.PendingBalance).HasPrecision(30, 10);
        entity.Property(w => w.Currency).HasMaxLength(10).IsRequired();
        entity.Property(w => w.OwnerType).HasMaxLength(50).IsRequired();
        entity.Property(w => w.Version).IsConcurrencyToken();

        entity.HasIndex(w => new { w.TenantId, w.OwnerType, w.OwnerId });
        entity.HasIndex(w => w.Status);

        entity.Ignore(w => w.DomainEvents);
        entity.Ignore(w => w.LedgerEntries);
        entity.Ignore(w => w.TotalBalance);
    }
}
