using Application.DTOs;
using Minisource.Common.Response;

namespace Application.Features.Wallets;

/// <summary>
/// Service for admin wallet operations (credit, debit, query).
/// </summary>
public interface IAdminWalletService
{
    Task<AdminAdjustmentResponse> CreditWalletAsync(Guid tenantId, Guid walletId, Guid actorUserId, AdminCreditWalletRequest request, string? idempotencyKey, CancellationToken ct = default);
    Task<AdminAdjustmentResponse> DebitWalletAsync(Guid tenantId, Guid walletId, Guid actorUserId, AdminDebitWalletRequest request, string? idempotencyKey, CancellationToken ct = default);
    Task<WalletAccountDto> GetWalletAsync(Guid walletId, CancellationToken ct = default);
    Task<List<WalletAccountDto>> GetUserWalletsAsync(Guid tenantId, string ownerType, Guid ownerId, CancellationToken ct = default);
    Task<List<WalletAccountDto>> GetAdminWalletsAsync(Guid tenantId, string? ownerType, Guid? ownerId, string? currency, string? status, int skip, int take, CancellationToken ct = default);
    Task<PagedResponse<LedgerEntryDto>> GetLedgerAsync(Guid walletId, int skip, int take, CancellationToken ct = default);
    Task<PagedResponse<WalletTransactionRecordDto>> GetTransactionsAsync(Guid tenantId, string? transactionType, Guid? walletId, int skip, int take, CancellationToken ct = default);
    Task<WalletTransactionRecordDto> GetTransactionAsync(Guid transactionId, CancellationToken ct = default);
    Task<PagedResponse<LedgerEntryDto>> ListAllLedgerAsync(Guid? tenantId, Guid? walletId, string? entryType, string? direction, string? currency, decimal? amountMin, decimal? amountMax, string? referenceType, string? referenceId, string? dateFrom, string? dateTo, string? query, int skip, int take, CancellationToken ct = default);
}
