using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Minisource.Common.Domain;
using PaymentEntity = Domain.Entities.Payment;
using PaymentAttemptEntity = Domain.Entities.PaymentAttempt;
using PaymentLogEntity = Domain.Entities.PaymentLog;
using WalletEntity = Domain.Entities.Wallet;
using WalletTransactionEntity = Domain.Entities.WalletTransaction;
using ParbadPaymentEntity = Parbad.Storage.Abstractions.Models.Payment;
using ParbadTransactionEntity = Parbad.Storage.Abstractions.Models.Transaction;

namespace Infrastructure.Data;

public class PaymentDbContext : DbContext
{
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options) { }

    public DbSet<PaymentEntity> Payments { get; set; }
    public DbSet<PaymentAttemptEntity> PaymentAttempts { get; set; }
    public DbSet<PaymentLogEntity> PaymentLogs { get; set; }
    public DbSet<WalletEntity> Wallets { get; set; }
    public DbSet<WalletTransactionEntity> WalletTransactions { get; set; }

    // Parbad storage entities
    public DbSet<ParbadPaymentEntity> ParbadPayments { get; set; }
    public DbSet<ParbadTransactionEntity> ParbadTransactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Payment entity configuration
        modelBuilder.Entity<PaymentEntity>(entity =>
        {
            entity.HasKey(p => p.Id);

            entity.Property(p => p.Amount)
                .HasPrecision(18, 2);

            entity.Property(p => p.CreditApplied)
                .HasPrecision(18, 2);

            entity.Property(p => p.AmountDue)
                .HasPrecision(18, 2);

            entity.Property(p => p.Currency)
                .HasMaxLength(3);

            entity.Property(p => p.Gateway)
                .HasMaxLength(50);

            entity.Property(p => p.TransactionReference)
                .HasMaxLength(100);

            entity.Property(p => p.IdempotencyKey)
                .HasMaxLength(100);

            entity.HasIndex(p => p.TrackingNumber)
                .IsUnique();

            entity.HasIndex(p => p.IdempotencyKey)
                .IsUnique()
                .HasFilter("\"IdempotencyKey\" IS NOT NULL");

            entity.HasIndex(p => p.UserId);

            entity.HasIndex(p => p.Status);

            entity.HasIndex(p => p.CreatedAt);

            entity.HasMany(p => p.Attempts)
                .WithOne(a => a.Payment)
                .HasForeignKey(a => a.PaymentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(p => p.Logs)
                .WithOne(l => l.Payment)
                .HasForeignKey(l => l.PaymentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Ignore domain events (not persisted)
            entity.Ignore(p => p.DomainEvents);
        });

        // PaymentAttempt entity configuration
        modelBuilder.Entity<PaymentAttemptEntity>(entity =>
        {
            entity.HasKey(a => a.Id);

            entity.Property(a => a.ErrorCode)
                .HasMaxLength(50);

            entity.HasIndex(a => new { a.PaymentId, a.AttemptNumber })
                .IsUnique();
        });

        // PaymentLog entity configuration
        modelBuilder.Entity<PaymentLogEntity>(entity =>
        {
            entity.HasKey(l => l.Id);

            entity.Property(l => l.Action)
                .HasMaxLength(50);

            entity.Property(l => l.Actor)
                .HasMaxLength(100);

            entity.Property(l => l.IpAddress)
                .HasMaxLength(45);

            entity.HasIndex(l => l.Timestamp);
        });

        // Wallet entity configuration
        modelBuilder.Entity<WalletEntity>(entity =>
        {
            entity.HasKey(w => w.Id);

            entity.Property(w => w.Balance)
                .HasPrecision(18, 2);

            entity.Property(w => w.Currency)
                .HasMaxLength(3);

            entity.Property(w => w.UserId)
                .HasMaxLength(100);

            entity.HasIndex(w => w.UserId)
                .IsUnique();

            entity.HasMany(w => w.Transactions)
                .WithOne(t => t.Wallet)
                .HasForeignKey(t => t.WalletId)
                .OnDelete(DeleteBehavior.Cascade);

            // Ignore domain events (not persisted)
            entity.Ignore(w => w.DomainEvents);
        });

        // WalletTransaction entity configuration
        modelBuilder.Entity<WalletTransactionEntity>(entity =>
        {
            entity.HasKey(t => t.Id);

            entity.Property(t => t.Amount)
                .HasPrecision(18, 2);

            entity.Property(t => t.BalanceAfter)
                .HasPrecision(18, 2);

            entity.Property(t => t.Description)
                .HasMaxLength(500);

            entity.Property(t => t.ReferenceId)
                .HasMaxLength(100);

            entity.Property(t => t.ReferenceType)
                .HasMaxLength(50);

            entity.Property(t => t.ReversalReason)
                .HasMaxLength(500);

            entity.HasIndex(t => t.WalletId);

            entity.HasIndex(t => new { t.ReferenceId, t.ReferenceType });

            entity.HasIndex(t => t.CreatedAt);
        });

        // Parbad Payment entity configuration
        modelBuilder.Entity<ParbadPaymentEntity>(entity =>
        {
            entity.ToTable("ParbadPayments");
            entity.HasKey(p => p.Id);

            entity.Property(p => p.Amount)
                .HasPrecision(18, 2);
        });

        // Parbad Transaction entity configuration
        modelBuilder.Entity<ParbadTransactionEntity>(entity =>
        {
            entity.ToTable("ParbadTransactions");
            entity.HasKey(t => t.Id);

            entity.Property(t => t.Amount)
                .HasPrecision(18, 2);

            entity.HasOne<ParbadPaymentEntity>()
                .WithMany()
                .HasForeignKey(t => t.PaymentId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}