using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class EventRoutingRuleConfiguration : IEntityTypeConfiguration<EventRoutingRule>
{
    public void Configure(EntityTypeBuilder<EventRoutingRule> entity)
    {
        entity.ToTable("EventRoutingRules");
        entity.HasKey(r => r.Id);
        entity.Property(r => r.EventType).HasMaxLength(150).IsRequired();
        entity.Property(r => r.Target).HasMaxLength(50).IsRequired();
        entity.HasIndex(r => new { r.TenantId, r.ApplicationCode, r.Target, r.Enabled });
    }
}
