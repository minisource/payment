using Application.DTOs;

namespace Application.Services;

/// <summary>
/// Payment service interface for processing payments.
/// </summary>
public interface IPaymentService
{
    /// <summary>
    /// Initiates a new payment.
    /// </summary>
    Task<PayResponse> InitiatePaymentAsync(PayRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifies a payment after gateway callback.
    /// </summary>
    Task<VerifyResponse> VerifyPaymentAsync(CancellationToken cancellationToken = default);

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