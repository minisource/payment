using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class AdminApprovalRequestConfiguration : IEntityTypeConfiguration<AdminApprovalRequest>
{
    public void Configure(EntityTypeBuilder<AdminApprovalRequest> entity)
    {
        entity.ToTable("AdminApprovalRequests");
        entity.HasKey(r => r.Id);
        entity.HasIndex(r => new { r.TenantId, r.Status, r.RequestedAt });
    }
}
