using Domain.Entities;

namespace Domain.Gateways;

/// <summary>
/// Service for managing wallet holds (reservations) used by the refund system.
/// Implements strict pre-hold policy: gateway refund is called only after successful hold.
/// </summary>
public interface IWalletHoldService
{
    /// <summary>
    /// Creates a wallet hold/reservation. Atomically reserves the amount from available balance.
    /// If insufficient available balance, returns failed result.
    /// Gateway refund API must NOT be called if this fails.
    /// </summary>
    Task<CreateWalletHoldResult> CreateHoldAsync(
        CreateWalletHoldRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Captures (permanently debits) a previously created hold.
    /// Converts the hold into a final wallet debit.
    /// </summary>
    Task<WalletHoldActionResult> CaptureHoldAsync(
        CaptureWalletHoldRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Releases a hold, returning the reserved amount back to available balance.
    /// Used when gateway refund fails or refund is cancelled.
    /// </summary>
    Task<WalletHoldActionResult> ReleaseHoldAsync(
        ReleaseWalletHoldRequest request,
        CancellationToken cancellationToken = default);
}

// ─── DTOs ──────────────────────────────────────────────────────

public sealed record CreateWalletHoldRequest(
    Guid TenantId,
    Guid WalletAccountId,
    decimal Amount,
    string Currency,
    string Reason,
    string ReferenceType,
    Guid ReferenceId,
    string? ApplicationCode = null,
    string? CreatedByUserId = null,
    string? IdempotencyKey = null);

public sealed record CaptureWalletHoldRequest(
    Guid HoldId,
    string Reason,
    string? CapturedByUserId = null);

public sealed record ReleaseWalletHoldRequest(
    Guid HoldId,
    string Reason,
    string? ReleasedByUserId = null);

public sealed record CreateWalletHoldResult(
    bool IsSuccess,
    Guid? HoldId = null,
    WalletHold? Hold = null,
    string? ErrorCode = null,
    string? ErrorMessage = null);

public sealed record WalletHoldActionResult(
    bool IsSuccess,
    WalletHold? Hold = null,
    string? ErrorCode = null,
    string? ErrorMessage = null);
