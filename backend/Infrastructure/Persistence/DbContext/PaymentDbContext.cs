using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Minisource.Common.Domain;
using Minisource.Common.Extensions;
using PaymentEntity = Domain.Entities.Payment;
using PaymentAttemptEntity = Domain.Entities.PaymentAttempt;
using PaymentLogEntity = Domain.Entities.PaymentLog;
using WalletEntity = Domain.Entities.Wallet;
using WalletTransactionEntity = Domain.Entities.WalletTransaction;
using ParbadPaymentEntity = Parbad.Storage.Abstractions.Models.Payment;
using ParbadTransactionEntity = Parbad.Storage.Abstractions.Models.Transaction;

namespace Infrastructure.Persistence;

public class PaymentDbContext : DbContext
{
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options) { }

    public DbSet<PaymentEntity> Payments { get; set; }
    public DbSet<PaymentAttemptEntity> PaymentAttempts { get; set; }
    public DbSet<PaymentLogEntity> PaymentLogs { get; set; }
    public DbSet<WalletEntity> Wallets { get; set; }
    public DbSet<WalletTransactionEntity> WalletTransactions { get; set; }

    // New wallet foundation
    public DbSet<WalletAccount> WalletAccounts { get; set; }
    public DbSet<WalletLedgerEntry> WalletLedgerEntries { get; set; }
    public DbSet<WalletTransactionRecord> WalletTransactionRecords { get; set; }
    public DbSet<PaymentSettings> PaymentSettings { get; set; }
    public DbSet<TenantPaymentSettings> TenantPaymentSettings { get; set; }
    public DbSet<FeeRule> FeeRules { get; set; }
    public DbSet<LimitRule> LimitRules { get; set; }
    public DbSet<RiskRule> RiskRules { get; set; }
    public DbSet<IdempotencyRecord> IdempotencyRecords { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<OutboxEvent> OutboxEvents { get; set; }

    // Payment intents & gateway entities
    public DbSet<PaymentIntent> PaymentIntents { get; set; }
    public DbSet<PaymentTransaction> PaymentTransactionsDb { get; set; }
    public DbSet<GatewayProvider> GatewayProviders { get; set; }
    public DbSet<GatewayConfig> GatewayConfigs { get; set; }
    public DbSet<GatewayRoutingPolicy> GatewayRoutingPolicies { get; set; }
    public DbSet<GatewayRoutingRule> GatewayRoutingRules { get; set; }

    // Public payment links
    public DbSet<PaymentLink> PaymentLinks { get; set; }

    // Withdrawal & payout entities
    public DbSet<PayoutAccount> PayoutAccounts { get; set; }
    public DbSet<WithdrawalRequest> WithdrawalRequests { get; set; }
    public DbSet<PayoutRecord> PayoutRecords { get; set; }

    // Financial security entities
    public DbSet<WalletConsistencyCheck> WalletConsistencyChecks { get; set; }
    public DbSet<WalletConsistencyIssue> WalletConsistencyIssues { get; set; }
    public DbSet<FinancialReconciliationBatch> ReconciliationBatches { get; set; }
    public DbSet<FinancialReconciliationItem> ReconciliationItems { get; set; }
    public DbSet<BankStatementImport> BankStatementImports { get; set; }
    public DbSet<BankStatementEntry> BankStatementEntries { get; set; }
    public DbSet<RiskEvaluation> RiskEvaluations { get; set; }
    public DbSet<RiskCase> RiskCases { get; set; }
    public DbSet<PaymentLimitUsage> PaymentLimitUsages { get; set; }
    public DbSet<AdminApprovalRequest> AdminApprovalRequests { get; set; }
    public DbSet<AdminApprovalDecision> AdminApprovalDecisions { get; set; }
    public DbSet<AdminApprovalPolicy> AdminApprovalPolicies { get; set; }

    // Webhooks & event routing entities
    public DbSet<WebhookSubscription> WebhookSubscriptions { get; set; }
    public DbSet<WebhookDelivery> WebhookDeliveries { get; set; }
    public DbSet<EventRoutingRule> EventRoutingRules { get; set; }

    // Notification settings
    public DbSet<PaymentNotificationSetting> PaymentNotificationSettings { get; set; }

    // Refund entities
    public DbSet<RefundRequest> RefundRequests { get; set; }
    public DbSet<RefundGatewayAttempt> RefundGatewayAttempts { get; set; }
    public DbSet<WalletHold> WalletHolds { get; set; }

    // Parbad storage entities
    public DbSet<ParbadPaymentEntity> ParbadPayments { get; set; }
    public DbSet<ParbadTransactionEntity> ParbadTransactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PaymentDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        CleanTrackedStrings();
        return await base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        CleanTrackedStrings();
        return base.SaveChanges();
    }

    /// <summary>
    /// Automatically cleans string properties on all tracked entities before saving.
    /// Trims whitespace, normalizes Persian characters, converts digits to English.
    /// </summary>
    private void CleanTrackedStrings()
    {
        foreach (var entry in ChangeTracker.Entries<Entity<Guid>>())
        {
            if (entry.State is not (EntityState.Added or EntityState.Modified))
                continue;

            foreach (var property in entry.Entity.GetType().GetProperties())
            {
                if (property.PropertyType != typeof(string) || !property.CanWrite)
                    continue;

                var originalValue = (string?)property.GetValue(entry.Entity);
                var cleanedValue = originalValue.CleanString();

                if (!string.Equals(originalValue, cleanedValue, StringComparison.Ordinal))
                {
                    property.SetValue(entry.Entity, cleanedValue);
                }
            }
        }
    }
}