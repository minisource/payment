using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class IdempotencyRecordConfiguration : IEntityTypeConfiguration<IdempotencyRecord>
{
    public void Configure(EntityTypeBuilder<IdempotencyRecord> entity)
    {
        entity.ToTable("IdempotencyRecords");
        entity.HasKey(r => r.Id);
        entity.Property(r => r.IdempotencyKey).HasMaxLength(200).IsRequired();
        entity.Property(r => r.Operation).HasMaxLength(100).IsRequired();
        entity.HasIndex(r => new { r.TenantId, r.IdempotencyKey }).IsUnique();
    }
}
