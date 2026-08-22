using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> entity)
    {
        entity.ToTable("AuditLogs");
        entity.HasKey(l => l.Id);
        entity.Property(l => l.EntityType).HasMaxLength(100).IsRequired();
        entity.Property(l => l.EntityId).HasMaxLength(100).IsRequired();
        entity.Property(l => l.Action).HasMaxLength(150).IsRequired();
        entity.HasIndex(l => new { l.EntityType, l.EntityId, l.CreatedAt }).IsDescending(false, false, true);
    }
}
