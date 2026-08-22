using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class PayoutAccountConfiguration : IEntityTypeConfiguration<PayoutAccount>
{
    public void Configure(EntityTypeBuilder<PayoutAccount> entity)
    {
        entity.ToTable("PayoutAccounts");
        entity.HasKey(a => a.Id);
        entity.Property(a => a.AccountType).HasMaxLength(50).IsRequired();
        entity.Property(a => a.OwnerType).HasMaxLength(50).IsRequired();
        entity.Property(a => a.Currency).HasMaxLength(10).IsRequired();
        entity.Property(a => a.BankName).HasMaxLength(200);
        entity.Property(a => a.BankCode).HasMaxLength(50);
        entity.Property(a => a.CardNumberMasked).HasMaxLength(50);
        entity.Property(a => a.CardNumberHash).HasMaxLength(128);
        entity.Property(a => a.Iban).HasMaxLength(200);
        entity.Property(a => a.IbanHash).HasMaxLength(128);
        entity.Property(a => a.AccountNumberMasked).HasMaxLength(50);
        entity.Property(a => a.AccountNumberHash).HasMaxLength(128);
        entity.Property(a => a.HolderName).HasMaxLength(200).IsRequired();
        entity.Property(a => a.HolderNationalIdHash).HasMaxLength(128);
        entity.HasIndex(a => new { a.TenantId, a.OwnerType, a.OwnerId });
        entity.HasIndex(a => new { a.TenantId, a.Status });
        entity.HasIndex(a => new { a.TenantId, a.IbanHash });
        entity.HasIndex(a => new { a.TenantId, a.CardNumberHash });
        entity.Ignore(a => a.DomainEvents);
        entity.Ignore(a => a.CanBeUsed);
    }
}
