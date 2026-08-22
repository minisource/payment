using Domain.Entities;

namespace Domain.Repositories;

public interface IGatewayProviderRepository
{
    Task<GatewayProvider?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<List<GatewayProvider>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(GatewayProvider provider, CancellationToken ct = default);
    Task UpdateAsync(GatewayProvider provider, CancellationToken ct = default);
}
