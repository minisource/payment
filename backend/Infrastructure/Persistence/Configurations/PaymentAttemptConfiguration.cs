using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentAttemptEntity = Domain.Entities.PaymentAttempt;

namespace Infrastructure.Persistence.Configurations;

public class PaymentAttemptConfiguration : IEntityTypeConfiguration<PaymentAttemptEntity>
{
    public void Configure(EntityTypeBuilder<PaymentAttemptEntity> entity)
    {
        entity.HasKey(a => a.Id);

        entity.Property(a => a.ErrorCode)
            .HasMaxLength(50);

        entity.HasIndex(a => new { a.PaymentId, a.AttemptNumber })
            .IsUnique();
    }
}
