using Domain.Entities;
using Domain.Enums;
using PaymentEntity = Domain.Entities.Payment;

namespace Domain.Repositories;

/// <summary>
/// Repository interface for Payment aggregate.
/// </summary>
public interface IPaymentRepository
{
    /// <summary>
    /// Gets a payment by ID with all related data.
    /// </summary>
    Task<PaymentEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a payment by ID with logs loaded.
    /// </summary>
    Task<PaymentEntity?> GetByIdWithLogsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a payment by tracking number.
    /// </summary>
    Task<PaymentEntity?> GetByTrackingNumberAsync(long trackingNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a payment by idempotency key.
    /// </summary>
    Task<PaymentEntity?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new payment.
    /// </summary>
    Task AddAsync(PaymentEntity payment, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing payment.
    /// </summary>
    Task UpdateAsync(PaymentEntity payment, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the tracking number for a payment.
    /// </summary>
    Task UpdateTrackingNumberAsync(Guid paymentId, long trackingNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets payments with pagination and filtering.
    /// </summary>
    Task<List<PaymentEntity>> GetPaymentsAsync(
        string? userId,
        string? status,
        DateTime? from,
        DateTime? to,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets total count for pagination.
    /// </summary>
    Task<int> GetTotalCountAsync(
        string? userId,
        string? status,
        DateTime? from,
        DateTime? to,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets payments by user ID.
    /// </summary>
    Task<List<PaymentEntity>> GetByUserIdAsync(
        string userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a new unique tracking number.
    /// </summary>
    Task<long> GenerateTrackingNumberAsync(CancellationToken cancellationToken = default);
}