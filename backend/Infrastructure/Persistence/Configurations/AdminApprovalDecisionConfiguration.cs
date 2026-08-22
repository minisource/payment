using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class AdminApprovalDecisionConfiguration : IEntityTypeConfiguration<AdminApprovalDecision>
{
    public void Configure(EntityTypeBuilder<AdminApprovalDecision> entity)
    {
        entity.ToTable("AdminApprovalDecisions");
        entity.HasKey(d => d.Id);
    }
}
