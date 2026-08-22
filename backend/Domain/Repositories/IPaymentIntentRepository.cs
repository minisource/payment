using Domain.Entities;
using Domain.Enums;

namespace Domain.Repositories;

public interface IPaymentIntentRepository
{
    Task<PaymentIntent?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PaymentIntent?> GetByIdWithTransactionsAsync(Guid id, CancellationToken ct = default);
    Task<PaymentIntent?> GetByIdempotencyKeyAsync(Guid tenantId, string idempotencyKey, CancellationToken ct = default);
    Task AddAsync(PaymentIntent intent, CancellationToken ct = default);
    Task UpdateAsync(PaymentIntent intent, CancellationToken ct = default);
    Task<List<PaymentIntent>> GetByTenantAsync(Guid tenantId, int skip = 0, int take = 50,
        PaymentIntentStatus? status = null, string? applicationCode = null,
        Guid? payerUserId = null, CancellationToken ct = default);
    Task<int> CountByTenantAsync(Guid tenantId, PaymentIntentStatus? status = null,
        string? applicationCode = null, CancellationToken ct = default);
    Task<List<PaymentIntent>> GetByExternalReferenceAsync(Guid tenantId, string referenceType, string referenceId, CancellationToken ct = default);
}
