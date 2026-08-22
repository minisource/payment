using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class RefundGatewayAttemptConfiguration : IEntityTypeConfiguration<RefundGatewayAttempt>
{
    public void Configure(EntityTypeBuilder<RefundGatewayAttempt> entity)
    {
        entity.ToTable("RefundGatewayAttempts");
        entity.HasKey(a => a.Id);

        entity.Property(a => a.ProviderCode).HasMaxLength(100).IsRequired();
        entity.Property(a => a.GatewayName).HasMaxLength(200);
        entity.Property(a => a.ErrorCode).HasMaxLength(100);
        entity.Property(a => a.ErrorMessage).HasMaxLength(1000);
        entity.Property(a => a.GatewayRefundId).HasMaxLength(200);
        entity.Property(a => a.GatewayTrackingCode).HasMaxLength(200);
        entity.Property(a => a.GatewayStatus).HasMaxLength(100);

        entity.HasIndex(a => a.RefundRequestId);
        entity.HasIndex(a => new { a.RefundRequestId, a.AttemptNo }).IsUnique();
    }
}
