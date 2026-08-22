using Application.DTOs;

namespace Application.Features.Payments;

/// <summary>
/// Service for querying payments (read operations only).
/// </summary>
public interface IPaymentQueryService
{
    /// <summary>
    /// Gets a payment by ID.
    /// </summary>
    Task<PaymentDto> GetPaymentAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets paginated list of payments with filters.
    /// </summary>
    Task<PagedResult<PaymentDto>> GetPaymentsAsync(
        string? status,
        DateTime? from,
        DateTime? to,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets payment logs for a specific payment.
    /// </summary>
    Task<List<PaymentLogDto>> GetPaymentLogsAsync(Guid paymentId, CancellationToken cancellationToken = default);
}
