using Domain.Entities;
using Domain.Enums;

namespace Domain.Repositories;

public interface IPaymentLinkRepository
{
    Task<PaymentLink?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PaymentLink?> GetByTokenHashAsync(string tokenHash, CancellationToken ct = default);
    Task AddAsync(PaymentLink link, CancellationToken ct = default);
    Task UpdateAsync(PaymentLink link, CancellationToken ct = default);
    Task<List<PaymentLink>> GetByOwnerAsync(Guid tenantId, string ownerType, Guid ownerId, int skip = 0, int take = 50, CancellationToken ct = default);
    Task<List<PaymentLink>> GetByTenantAsync(Guid tenantId, int skip = 0, int take = 50, PaymentLinkStatus? status = null, string? applicationCode = null, CancellationToken ct = default);
    Task<int> CountByTenantAsync(Guid tenantId, PaymentLinkStatus? status = null, CancellationToken ct = default);
    Task<List<PaymentLink>> GetByRecipientWalletAsync(Guid walletId, int skip = 0, int take = 50, CancellationToken ct = default);
}
