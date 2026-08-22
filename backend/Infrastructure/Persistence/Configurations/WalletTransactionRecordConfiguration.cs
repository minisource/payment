using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class WalletTransactionRecordConfiguration : IEntityTypeConfiguration<WalletTransactionRecord>
{
    public void Configure(EntityTypeBuilder<WalletTransactionRecord> entity)
    {
        entity.ToTable("WalletTransactionRecords");
        entity.HasKey(t => t.Id);

        entity.Property(t => t.Amount).HasPrecision(30, 10).IsRequired();
        entity.Property(t => t.Currency).HasMaxLength(10).IsRequired();
        entity.Property(t => t.TransactionType).HasMaxLength(50).IsRequired();
        entity.Property(t => t.ReferenceType).HasMaxLength(100);
        entity.Property(t => t.ReferenceId).HasMaxLength(200);
        entity.Property(t => t.IdempotencyKey).HasMaxLength(200);

        entity.HasIndex(t => new { t.TenantId, t.CreatedAt }).IsDescending(false, true);
        entity.HasIndex(t => t.SourceWalletId);
        entity.HasIndex(t => t.DestinationWalletId);
        entity.HasIndex(t => new { t.TenantId, t.IdempotencyKey }).IsUnique()
            .HasFilter("\"IdempotencyKey\" IS NOT NULL");

        entity.Ignore(t => t.DomainEvents);
        entity.Ignore(t => t.LedgerEntries);
    }
}
