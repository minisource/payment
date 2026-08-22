using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class WalletConsistencyIssueConfiguration : IEntityTypeConfiguration<WalletConsistencyIssue>
{
    public void Configure(EntityTypeBuilder<WalletConsistencyIssue> entity)
    {
        entity.ToTable("WalletConsistencyIssues");
        entity.HasKey(i => i.Id);
        entity.HasIndex(i => new { i.TenantId, i.Status, i.CreatedAt });
    }
}
