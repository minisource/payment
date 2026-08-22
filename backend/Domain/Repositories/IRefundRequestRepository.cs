using Domain.Entities;
using Domain.Enums;

namespace Domain.Repositories;

/// <summary>
/// Repository for refund request persistence.
/// </summary>
public interface IRefundRequestRepository
{
    Task<RefundRequest?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<RefundRequest?> GetByIdForUpdateAsync(Guid id, CancellationToken ct = default);
    Task<RefundRequest?> GetByIdempotencyKeyAsync(Guid tenantId, string idempotencyKey, CancellationToken ct = default);
    Task<List<RefundRequest>> GetByPaymentTransactionAsync(Guid transactionId, CancellationToken ct = default);
    Task<(List<RefundRequest> Items, int Total)> ListAsync(
        Guid? tenantId,
        RefundRequestStatus? status = null,
        Guid? paymentTransactionId = null,
        Guid? walletId = null,
        string? providerCode = null,
        string? currency = null,
        DateTime? from = null,
        DateTime? to = null,
        string? query = null,
        string? applicationCode = null,
        int skip = 0,
        int take = 50,
        CancellationToken ct = default);
    Task AddAsync(RefundRequest refund, CancellationToken ct = default);
    Task UpdateAsync(RefundRequest refund, CancellationToken ct = default);
    Task<decimal> GetTotalRefundedAsync(Guid transactionId, CancellationToken ct = default);
}
