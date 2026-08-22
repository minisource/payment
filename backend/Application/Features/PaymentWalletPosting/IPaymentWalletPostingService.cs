using Domain.Entities;

namespace Application.Features.PaymentWalletPosting;

/// <summary>
/// Service for posting wallet transactions after payment verification.
/// </summary>
public interface IPaymentWalletPostingService
{
    Task<WalletPostingResult> PostVerifiedPaymentAsync(PaymentIntent intent, PaymentTransaction transaction, CancellationToken ct);
}

/// <summary>
/// Result of a wallet posting operation.
/// </summary>
public sealed record WalletPostingResult(
    bool Success,
    Guid? WalletTransactionId = null,
    Guid? CreditLedgerEntryId = null,
    Guid? TransferRecordId = null,
    string? ErrorMessage = null,
    string? ErrorCode = null);
