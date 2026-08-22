using Domain.Entities;
using Domain.Enums;

namespace Domain.Repositories;

public interface IPaymentTransactionRepository
{
    Task<PaymentTransaction?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PaymentTransaction?> GetByAuthorityAsync(string authority, CancellationToken ct = default);
    Task AddAsync(PaymentTransaction transaction, CancellationToken ct = default);
    Task UpdateAsync(PaymentTransaction transaction, CancellationToken ct = default);
    Task<List<PaymentTransaction>> GetByIntentIdAsync(Guid paymentIntentId, CancellationToken ct = default);
    Task<List<PaymentTransaction>> GetByTenantAsync(Guid tenantId, int skip = 0, int take = 50,
        PaymentTransactionStatus? status = null, string? providerCode = null,
        DateTime? from = null, DateTime? to = null, CancellationToken ct = default);
    Task<int> CountByTenantAsync(Guid tenantId, PaymentTransactionStatus? status = null, CancellationToken ct = default);
    Task<PaymentTransaction?> GetLatestForIntentAsync(Guid paymentIntentId, CancellationToken ct = default);
}
