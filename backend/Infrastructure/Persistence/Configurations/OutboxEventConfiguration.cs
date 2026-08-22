using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class OutboxEventConfiguration : IEntityTypeConfiguration<OutboxEvent>
{
    public void Configure(EntityTypeBuilder<OutboxEvent> entity)
    {
        entity.ToTable("OutboxEvents");
        entity.HasKey(e => e.Id);
        entity.Property(e => e.ApplicationCode).HasMaxLength(100);
        entity.HasIndex(e => new { e.Status, e.OccurredAt });
        entity.HasIndex(e => new { e.Status, e.AvailableAt, e.NextRetryAt, e.OccurredAt });
        entity.HasIndex(e => new { e.TenantId, e.EventType, e.OccurredAt }).IsDescending(false, false, true);
        entity.HasIndex(e => new { e.TenantId, e.ApplicationCode, e.OccurredAt }).IsDescending(false, false, true);
    }
}
