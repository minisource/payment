using Application.DTOs;

namespace Application.Features.Payments;

/// <summary>
/// Payment service interface for processing payments (initiate + verify).
/// For query operations, use <see cref="IPaymentQueryService"/>.
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
}