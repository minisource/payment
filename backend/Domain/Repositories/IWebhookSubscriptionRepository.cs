using Domain.Entities;

namespace Domain.Repositories;

public interface IWebhookSubscriptionRepository
{
    Task<WebhookSubscription?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<WebhookSubscription>> ListAsync(Guid? tenantId, string? applicationCode, string? status, int skip, int take, CancellationToken ct = default);
    Task<int> CountAsync(Guid? tenantId, string? applicationCode, string? status, CancellationToken ct = default);
    Task<int> CountActiveAsync(Guid? tenantId, CancellationToken ct = default);
    Task AddAsync(WebhookSubscription sub, CancellationToken ct = default);
}
