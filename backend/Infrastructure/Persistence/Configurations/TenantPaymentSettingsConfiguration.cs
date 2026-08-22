using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class TenantPaymentSettingsConfiguration : IEntityTypeConfiguration<TenantPaymentSettings>
{
    public void Configure(EntityTypeBuilder<TenantPaymentSettings> entity)
    {
        entity.ToTable("TenantPaymentSettings");
        entity.HasKey(s => s.Id);
        entity.HasIndex(s => s.TenantId).IsUnique();
        entity.Property(s => s.MinWalletAdjustmentAmount).HasPrecision(30, 10);
        entity.Property(s => s.MaxWalletAdjustmentAmount).HasPrecision(30, 10);
        entity.Property(s => s.DailyWalletCreditLimit).HasPrecision(30, 10);
        entity.Property(s => s.DailyWalletDebitLimit).HasPrecision(30, 10);
        entity.Property(s => s.MinWithdrawalAmount).HasPrecision(30, 10);
        entity.Property(s => s.MaxWithdrawalAmount).HasPrecision(30, 10);
        entity.Property(s => s.WithdrawalDailyAmountLimit).HasPrecision(30, 10);
        entity.Property(s => s.WithdrawalMonthlyAmountLimit).HasPrecision(30, 10);
    }
}
