using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class WebhookSubscriptionConfiguration : IEntityTypeConfiguration<WebhookSubscription>
{
    public void Configure(EntityTypeBuilder<WebhookSubscription> entity)
    {
        entity.ToTable("WebhookSubscriptions");
        entity.HasKey(s => s.Id);
        entity.Property(s => s.Name).HasMaxLength(300).IsRequired();
        entity.Property(s => s.Url).HasMaxLength(1000).IsRequired();
        entity.Property(s => s.ApplicationCode).HasMaxLength(100);
        entity.Property(s => s.SecretHash).HasMaxLength(128);
        entity.HasIndex(s => new { s.TenantId, s.ApplicationCode, s.Status });
    }
}
