using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class AdminApprovalPolicyConfiguration : IEntityTypeConfiguration<AdminApprovalPolicy>
{
    public void Configure(EntityTypeBuilder<AdminApprovalPolicy> entity)
    {
        entity.ToTable("AdminApprovalPolicies");
        entity.HasKey(p => p.Id);
        entity.Property(p => p.AmountThreshold).HasPrecision(30, 10);
    }
}
