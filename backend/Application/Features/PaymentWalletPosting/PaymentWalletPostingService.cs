using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using Microsoft.Extensions.Logging;
using Minisource.Common.Domain;
using Minisource.Common.Exceptions;
using Minisource.Common.Locking;

namespace Application.Features.PaymentWalletPosting;

public class PaymentWalletPostingService : IPaymentWalletPostingService
{
    private readonly IWalletAccountRepository _walletRepo;
    private readonly IWalletTransactionRecordRepository _txnRecordRepo;
    private readonly IAuditLogRepository _auditRepo;
    private readonly IDistributedLockService _lockService;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<PaymentWalletPostingService> _logger;

    public PaymentWalletPostingService(
        IWalletAccountRepository walletRepo,
        IWalletTransactionRecordRepository txnRecordRepo,
        IAuditLogRepository auditRepo,
        IDistributedLockService lockService,
        IUnitOfWork uow,
        ILogger<PaymentWalletPostingService> logger)
    {
        _walletRepo = walletRepo;
        _txnRecordRepo = txnRecordRepo;
        _auditRepo = auditRepo;
        _lockService = lockService;
        _uow = uow;
        _logger = logger;
    }

    public async Task<WalletPostingResult> PostVerifiedPaymentAsync(
        PaymentIntent intent, PaymentTransaction transaction, CancellationToken ct)
    {
        // Idempotent check
        if (intent.WalletPostingStatus == "posted")
            return new WalletPostingResult(true, intent.WalletTransactionId, ErrorMessage: "Already posted");

        if (intent.WalletBehavior == WalletBehavior.None)
        {
            intent.MarkWalletPosted(Guid.Empty);
            return new WalletPostingResult(true, Guid.Empty);
        }

        WalletPostingResult result;
        switch (intent.WalletBehavior)
        {
            case WalletBehavior.CreditRecipientWallet:
                result = await PostCreditToWalletAsync(intent, transaction, intent.RecipientWalletId!.Value, ct);
                break;
            case WalletBehavior.CreditPayerWallet:
                result = await PostCreditToWalletAsync(intent, transaction, intent.PayerWalletId!.Value, ct);
                break;
            case WalletBehavior.CreditPayerThenTransferToRecipient:
                result = await PostCreditThenTransferAsync(intent, transaction, ct);
                break;
            default:
                return new WalletPostingResult(false, ErrorMessage: "Unknown wallet behavior");
        }

        if (result.Success)
        {
            intent.MarkWalletPosted(result.WalletTransactionId!.Value);
        }

        return result;
    }

    private async Task<WalletPostingResult> PostCreditToWalletAsync(
        PaymentIntent intent, PaymentTransaction transaction, Guid walletId, CancellationToken ct)
    {
        var lockKey = $"payment:tenant:{intent.TenantId}:wallet:{walletId}:currency:{intent.Currency}";
        await using var walletLock = await _lockService.AcquireAsync(lockKey, TimeSpan.FromSeconds(30), ct);
        if (!walletLock.IsAcquired)
            return new WalletPostingResult(false, ErrorCode: "wallet_locked", ErrorMessage: "Unable to acquire lock");

        await using var dbTxn = await _uow.BeginTransactionAsync(ct);
        try
        {
            var wallet = await _walletRepo.GetByIdForUpdateAsync(walletId, ct)
                ?? throw new NotFoundException("WalletAccount", walletId);

            if (wallet.TenantId != intent.TenantId)
                throw new ForbiddenException("Wallet does not belong to tenant");

            if (wallet.Currency != intent.Currency)
                throw new ValidationException("Currency", $"Currency mismatch: wallet is {wallet.Currency}");

            // Create wallet transaction record
            var record = WalletTransactionRecord.Create(
                intent.TenantId, "gateway_deposit", transaction.Amount, wallet.Currency,
                description: $"Gateway payment via {transaction.ProviderCode}",
                createdByUserId: intent.CreatedByUserId,
                referenceType: "payment_intent",
                referenceId: intent.Id.ToString(),
                destinationWalletId: wallet.Id);

            // Post credit
            var entry = wallet.PostCredit(transaction.Amount, LedgerEntryType.GatewayDeposit,
                record.Id, $"Gateway payment: {transaction.GatewayReferenceId}",
                "payment_intent", intent.Id.ToString(), intent.CreatedByUserId);

            record.AddLedgerEntry(entry);
            record.Post();

            await _walletRepo.UpdateAsync(wallet, ct);
            await _txnRecordRepo.AddAsync(record, ct);

            // Audit
            await _auditRepo.AddAsync(new AuditLog
            {
                TenantId = intent.TenantId, ActorUserId = intent.CreatedByUserId,
                Action = "payment_wallet.posted", EntityType = "WalletAccount",
                EntityId = wallet.Id.ToString(),
                Reason = $"Gateway payment: intent={intent.Id}, txn={transaction.Id}"
            }, ct);

            await _uow.SaveChangesAsync(ct);
            await dbTxn.CommitAsync(ct);

            return new WalletPostingResult(true, record.Id, entry.Id);
        }
        catch (Exception ex)
        {
            await dbTxn.RollbackAsync(ct);
            _logger.LogError(ex, "Wallet posting failed for intent {IntentId}", intent.Id);
            return new WalletPostingResult(false, ErrorCode: "wallet_posting_failed", ErrorMessage: ex.Message);
        }
    }

    private async Task<WalletPostingResult> PostCreditThenTransferAsync(
        PaymentIntent intent, PaymentTransaction transaction, CancellationToken ct)
    {
        // Step 1: Credit payer wallet
        var creditResult = await PostCreditToWalletAsync(
            intent, transaction, intent.PayerWalletId!.Value, ct);

        if (!creditResult.Success)
            return creditResult;

        // Step 2: Transfer from payer to recipient
        var payerLockKey = $"payment:tenant:{intent.TenantId}:wallet:{intent.PayerWalletId}:currency:{intent.Currency}";
        var recipientLockKey = $"payment:tenant:{intent.TenantId}:wallet:{intent.RecipientWalletId}:currency:{intent.Currency}";

        // Acquire locks in order
        var lockKeys = new[] { payerLockKey, recipientLockKey }.OrderBy(k => k).ToArray();

        await using var lock1 = await _lockService.AcquireAsync(lockKeys[0], TimeSpan.FromSeconds(30), ct);
        await using var lock2 = await _lockService.AcquireAsync(lockKeys[1], TimeSpan.FromSeconds(30), ct);

        if (!lock1.IsAcquired || !lock2.IsAcquired)
            return new WalletPostingResult(false, ErrorCode: "wallet_locked",
                ErrorMessage: "Unable to acquire locks for transfer");

        await using var dbTxn = await _uow.BeginTransactionAsync(ct);
        try
        {
            var payerWallet = await _walletRepo.GetByIdForUpdateAsync(intent.PayerWalletId!.Value, ct)
                ?? throw new NotFoundException("WalletAccount", intent.PayerWalletId.Value);
            var recipientWallet = await _walletRepo.GetByIdForUpdateAsync(intent.RecipientWalletId!.Value, ct)
                ?? throw new NotFoundException("WalletAccount", intent.RecipientWalletId.Value);

            if (payerWallet.TenantId != intent.TenantId || recipientWallet.TenantId != intent.TenantId)
                throw new ForbiddenException("Wallets do not belong to tenant");

            if (payerWallet.Currency != recipientWallet.Currency)
                throw new BusinessException("Currency mismatch for transfer", "wallet_currency_mismatch");

            if (payerWallet.AvailableBalance < transaction.Amount)
                throw new BusinessException($"Insufficient balance for transfer. Available: {payerWallet.AvailableBalance}",
                    "wallet_insufficient_balance");

            // Create transfer record
            var transferRecord = WalletTransactionRecord.Create(
                intent.TenantId, "payment_wallet_transfer", transaction.Amount, payerWallet.Currency,
                description: $"Transfer for payment intent {intent.Id}",
                createdByUserId: intent.CreatedByUserId,
                referenceType: "payment_intent", referenceId: intent.Id.ToString(),
                sourceWalletId: payerWallet.Id, destinationWalletId: recipientWallet.Id);

            // Debit payer
            var debitEntry = payerWallet.PostDebit(transaction.Amount, LedgerEntryType.WalletTransfer,
                false, transferRecord.Id, "Transfer to recipient",
                "payment_intent", intent.Id.ToString(), intent.CreatedByUserId);

            // Credit recipient
            var creditEntry = recipientWallet.PostCredit(transaction.Amount, LedgerEntryType.WalletTransfer,
                transferRecord.Id, "Transfer from payer",
                "payment_intent", intent.Id.ToString(), intent.CreatedByUserId);

            transferRecord.AddLedgerEntry(debitEntry);
            transferRecord.AddLedgerEntry(creditEntry);
            transferRecord.Post();

            await _walletRepo.UpdateAsync(payerWallet, ct);
            await _walletRepo.UpdateAsync(recipientWallet, ct);
            await _txnRecordRepo.AddAsync(transferRecord, ct);

            await _auditRepo.AddAsync(new AuditLog
            {
                TenantId = intent.TenantId, ActorUserId = intent.CreatedByUserId,
                Action = "payment_wallet.transferred", EntityType = "WalletAccount",
                EntityId = payerWallet.Id.ToString(),
                Reason = $"Transfer {transferRecord.Id}: {payerWallet.Id} -> {recipientWallet.Id}, amount={transaction.Amount}",
                AfterSnapshot = System.Text.Json.JsonSerializer.Serialize(new
                {
                    fromWallet = payerWallet.Id, toWallet = recipientWallet.Id,
                    amount = transaction.Amount, currency = payerWallet.Currency
                })
            }, ct);

            await _uow.SaveChangesAsync(ct);
            await dbTxn.CommitAsync(ct);

            return new WalletPostingResult(true, creditResult.WalletTransactionId, creditResult.CreditLedgerEntryId,
                transferRecord.Id);
        }
        catch (Exception ex)
        {
            await dbTxn.RollbackAsync(ct);
            _logger.LogError(ex, "Transfer posting failed for intent {IntentId}", intent.Id);
            return new WalletPostingResult(false, ErrorCode: "wallet_posting_failed", ErrorMessage: ex.Message);
        }
    }
}
