using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class WalletLedgerEntryConfiguration : IEntityTypeConfiguration<WalletLedgerEntry>
{
    public void Configure(EntityTypeBuilder<WalletLedgerEntry> entity)
    {
        entity.ToTable("WalletLedgerEntries");
        entity.HasKey(e => e.Id);

        entity.Property(e => e.Amount).HasPrecision(30, 10).IsRequired();
        entity.Property(e => e.Currency).HasMaxLength(10).IsRequired();
        entity.Property(e => e.BalanceAvailableBefore).HasPrecision(30, 10);
        entity.Property(e => e.BalanceAvailableAfter).HasPrecision(30, 10);
        entity.Property(e => e.BalanceLockedBefore).HasPrecision(30, 10);
        entity.Property(e => e.BalanceLockedAfter).HasPrecision(30, 10);
        entity.Property(e => e.BalancePendingBefore).HasPrecision(30, 10);
        entity.Property(e => e.BalancePendingAfter).HasPrecision(30, 10);

        entity.HasIndex(e => new { e.WalletAccountId, e.CreatedAt }).IsDescending(false, true);
        entity.HasIndex(e => e.WalletTransactionId);
        entity.HasIndex(e => new { e.TenantId, e.CreatedAt }).IsDescending(false, true);
    }
}
