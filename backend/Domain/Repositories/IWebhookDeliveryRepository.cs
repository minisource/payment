using Domain.Entities;

namespace Domain.Repositories;

public interface IWebhookDeliveryRepository
{
    Task<WebhookDelivery?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<WebhookDelivery>> ListAsync(Guid? tenantId, Guid? subscriptionId, string? status, int skip, int take, CancellationToken ct = default);
    Task<int> CountAsync(Guid? tenantId, Guid? subscriptionId, string? status, CancellationToken ct = default);
}
