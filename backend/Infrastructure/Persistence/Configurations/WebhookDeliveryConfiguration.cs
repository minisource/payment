using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class WebhookDeliveryConfiguration : IEntityTypeConfiguration<WebhookDelivery>
{
    public void Configure(EntityTypeBuilder<WebhookDelivery> entity)
    {
        entity.ToTable("WebhookDeliveries");
        entity.HasKey(d => d.Id);
        entity.HasIndex(d => d.WebhookSubscriptionId);
        entity.HasIndex(d => d.OutboxEventId);
        entity.HasIndex(d => new { d.TenantId, d.Status, d.NextAttemptAt });
    }
}
