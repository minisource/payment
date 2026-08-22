using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class PaymentNotificationSettingConfiguration : IEntityTypeConfiguration<PaymentNotificationSetting>
{
    public void Configure(EntityTypeBuilder<PaymentNotificationSetting> entity)
    {
        entity.ToTable("PaymentNotificationSettings");
        entity.HasKey(e => e.Id);

        entity.Property(e => e.ApplicationCode).HasMaxLength(50);
        entity.Property(e => e.NotificationKey).HasMaxLength(200).IsRequired();
        entity.Property(e => e.Channel).HasMaxLength(20).IsRequired();
        entity.Property(e => e.RecipientType).HasMaxLength(20).IsRequired();

        // Unique constraint: one setting per tenant + key + channel + recipient
        entity.HasIndex(e => new { e.TenantId, e.ApplicationCode, e.NotificationKey, e.Channel, e.RecipientType })
            .IsUnique()
            .HasFilter("[DeletedAt] IS NULL");

        entity.HasIndex(e => new { e.TenantId, e.NotificationKey });
        entity.HasIndex(e => e.IsEnabled);
    }
}
