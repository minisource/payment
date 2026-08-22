using Application.DTOs;

namespace Application.Features.Refunds;

/// <summary>
/// Service for refund lifecycle management with strict pre-hold policy.
/// </summary>
public interface IRefundService
{
    /// <summary>Create a new refund request.</summary>
    Task<RefundRequestDto> CreateAsync(Guid tenantId, CreateRefundRequest req, string requestedByUserId, string? applicationCode = null, CancellationToken ct = default);

    /// <summary>Move refund to pending review.</summary>
    Task<RefundRequestDto> SubmitForReviewAsync(Guid refundId, CancellationToken ct = default);

    /// <summary>Admin approves refund.</summary>
    Task<RefundRequestDto> ApproveAsync(Guid refundId, string approvedByUserId, ApproveRefundRequest req, CancellationToken ct = default);

    /// <summary>Admin rejects refund.</summary>
    Task<RefundRequestDto> RejectAsync(Guid refundId, string rejectedByUserId, RejectRefundRequest req, CancellationToken ct = default);

    /// <summary>Cancel refund.</summary>
    Task<RefundRequestDto> CancelAsync(Guid refundId, CancellationToken ct = default);

    /// <summary>Process refund: create wallet hold, call gateway refund, capture/release hold.</summary>
    Task<RefundRequestDto> ProcessAsync(Guid refundId, string processedByUserId, CancellationToken ct = default);

    /// <summary>Retry a previously failed gateway refund.</summary>
    Task<RefundRequestDto> RetryAsync(Guid refundId, string processedByUserId, CancellationToken ct = default);

    /// <summary>Mark refund as requiring manual review.</summary>
    Task<RefundRequestDto> MarkManualReviewAsync(Guid refundId, string failureCode, string failureMessage, CancellationToken ct = default);
}

/// <summary>
/// Read-only queries for refunds.
/// </summary>
public interface IRefundQueryService
{
    Task<RefundRequestDto?> GetByIdAsync(Guid refundId, CancellationToken ct = default);
    Task<(List<RefundRequestDto> Items, int Total)> ListAsync(
        Guid? tenantId,
        string? status = null,
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
}
