using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Minisource.Common.Domain;

namespace Application.Features.Reconciliation;

public class ReconciliationService : IReconciliationService
{
    private readonly PaymentDbContext _ctx;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<ReconciliationService> _logger;

    public ReconciliationService(PaymentDbContext ctx, IUnitOfWork uow, ILogger<ReconciliationService> logger)
    { _ctx = ctx; _uow = uow; _logger = logger; }

    public async Task<FinancialReconciliationBatch> ReconcileWalletBalancesAsync(Guid tenantId, Guid? triggeredByUserId, CancellationToken ct)
    {
        var batch = CreateBatch(tenantId, "wallet_balance", triggeredByUserId);
        await _ctx.ReconciliationBatches.AddAsync(batch, ct);

        var wallets = await _ctx.WalletAccounts.Where(w => w.TenantId == tenantId).ToListAsync(ct);
        int matched = 0, mismatched = 0;

        foreach (var wallet in wallets)
        {
            var credits = await _ctx.WalletLedgerEntries
                .Where(e => e.WalletAccountId == wallet.Id && e.Direction == LedgerDirection.Credit)
                .SumAsync(e => e.Amount, ct);
            var debits = await _ctx.WalletLedgerEntries
                .Where(e => e.WalletAccountId == wallet.Id && e.Direction == LedgerDirection.Debit)
                .SumAsync(e => e.Amount, ct);
            var expectedBalance = credits - debits;

            if (expectedBalance == wallet.AvailableBalance)
            {
                matched++;
            }
            else
            {
                mismatched++;
                await _ctx.ReconciliationItems.AddAsync(new FinancialReconciliationItem
                {
                    TenantId = tenantId, BatchId = batch.Id, ItemType = "wallet_balance",
                    Status = "mismatched", Severity = "high",
                    PrimaryEntityType = "WalletAccount", PrimaryEntityId = wallet.Id.ToString(),
                    ExpectedAmount = expectedBalance, ActualAmount = wallet.AvailableBalance,
                    Currency = wallet.Currency,
                    IssueCode = "balance_mismatch",
                    Message = $"Wallet {wallet.Id}: expected {expectedBalance}, actual {wallet.AvailableBalance}"
                }, ct);
            }
        }

        batch.Complete(matched, mismatched);
        await _uow.SaveChangesAsync(ct);
        _logger.LogInformation("Wallet balance reconciliation: {Matched} matched, {Mismatched} mismatched", matched, mismatched);
        return batch;
    }

    public async Task<FinancialReconciliationBatch> ReconcileGatewayPaymentsAsync(Guid tenantId, Guid? triggeredByUserId, CancellationToken ct)
    {
        var batch = CreateBatch(tenantId, "gateway_payments", triggeredByUserId);
        await _ctx.ReconciliationBatches.AddAsync(batch, ct);

        // Find payment transactions with verified status but no wallet posting
        var unposted = await _ctx.PaymentTransactionsDb
            .Where(t => t.TenantId == tenantId && t.Status == PaymentTransactionStatus.Verified)
            .ToListAsync(ct);

        int matched = 0, mismatched = 0;
        foreach (var txn in unposted)
        {
            var intent = await _ctx.PaymentIntents.FirstOrDefaultAsync(p => p.Id == txn.PaymentIntentId, ct);
            if (intent?.WalletPostingStatus == "posted")
            {
                matched++;
            }
            else
            {
                mismatched++;
                await _ctx.ReconciliationItems.AddAsync(new FinancialReconciliationItem
                {
                    TenantId = tenantId, BatchId = batch.Id, ItemType = "gateway_payment",
                    Status = "missing_wallet_posting", Severity = "high",
                    PrimaryEntityType = "PaymentTransaction", PrimaryEntityId = txn.Id.ToString(),
                    RelatedEntityType = "PaymentIntent", RelatedEntityId = txn.PaymentIntentId.ToString(),
                    ExpectedAmount = txn.Amount, Currency = txn.Currency,
                    IssueCode = "verified_without_wallet_posting",
                    Message = $"Transaction {txn.Id}: verified but wallet not posted"
                }, ct);
            }
        }

        batch.Complete(matched, mismatched);
        await _uow.SaveChangesAsync(ct);
        return batch;
    }

    public async Task<FinancialReconciliationBatch> ReconcileWithdrawalPayoutsAsync(Guid tenantId, Guid? triggeredByUserId, CancellationToken ct)
    {
        var batch = CreateBatch(tenantId, "withdrawal_payouts", triggeredByUserId);
        await _ctx.ReconciliationBatches.AddAsync(batch, ct);

        var paidWithdrawals = await _ctx.WithdrawalRequests
            .Where(w => w.TenantId == tenantId && w.Status == WithdrawalStatus.Paid)
            .ToListAsync(ct);

        int matched = 0, mismatched = 0;
        foreach (var wr in paidWithdrawals)
        {
            var hasPayout = await _ctx.PayoutRecords.AnyAsync(p => p.WithdrawalRequestId == wr.Id, ct);
            if (hasPayout)
            {
                matched++;
            }
            else
            {
                mismatched++;
                await _ctx.ReconciliationItems.AddAsync(new FinancialReconciliationItem
                {
                    TenantId = tenantId, BatchId = batch.Id, ItemType = "withdrawal_payout",
                    Status = "missing_payout_record", Severity = "critical",
                    PrimaryEntityType = "WithdrawalRequest", PrimaryEntityId = wr.Id.ToString(),
                    ExpectedAmount = wr.NetAmount, Currency = wr.Currency,
                    IssueCode = "paid_without_payout_record",
                    Message = $"Withdrawal {wr.Id}: marked paid but no payout record exists"
                }, ct);
            }
        }

        batch.Complete(matched, mismatched);
        await _uow.SaveChangesAsync(ct);
        return batch;
    }

    public async Task<FinancialReconciliationBatch> ReconcilePaymentWalletPostingAsync(Guid tenantId, Guid? triggeredByUserId, CancellationToken ct)
    {
        var batch = CreateBatch(tenantId, "payment_wallet_posting", triggeredByUserId);
        await _ctx.ReconciliationBatches.AddAsync(batch, ct);

        var succeededIntents = await _ctx.PaymentIntents
            .Where(p => p.TenantId == tenantId && p.Status == PaymentIntentStatus.Succeeded)
            .ToListAsync(ct);

        int matched = 0, mismatched = 0;
        foreach (var intent in succeededIntents)
        {
            if (intent.WalletPostingStatus == "posted")
            {
                matched++;
            }
            else
            {
                mismatched++;
                await _ctx.ReconciliationItems.AddAsync(new FinancialReconciliationItem
                {
                    TenantId = tenantId, BatchId = batch.Id, ItemType = "payment_wallet_posting",
                    Status = "missing_wallet_posting", Severity = "high",
                    PrimaryEntityType = "PaymentIntent", PrimaryEntityId = intent.Id.ToString(),
                    ExpectedAmount = intent.NetAmount, Currency = intent.Currency,
                    IssueCode = "succeeded_without_wallet_posting",
                    Message = $"Intent {intent.Id}: succeeded but wallet posting is '{intent.WalletPostingStatus}'"
                }, ct);
            }
        }

        batch.Complete(matched, mismatched);
        await _uow.SaveChangesAsync(ct);
        return batch;
    }

    private static FinancialReconciliationBatch CreateBatch(Guid tenantId, string type, Guid? triggeredByUserId) => new()
    {
        TenantId = tenantId, ReconciliationType = type,
        TriggeredByUserId = triggeredByUserId, StartedAt = DateTime.UtcNow
    };
}
