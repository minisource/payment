using Application.DTOs;

namespace Application.Features.Gateways;

/// <summary>
/// Service for managing gateway providers and their configurations.
/// </summary>
public interface IGatewayConfigService
{
    Task<GatewayProviderDto> CreateProviderAsync(CreateGatewayProviderRequest request, CancellationToken ct);
    Task<GatewayProviderDto> UpdateProviderAsync(string code, UpdateGatewayProviderRequest request, CancellationToken ct);
    Task<GatewayProviderDto?> GetProviderAsync(string code, CancellationToken ct);
    Task<List<GatewayProviderDto>> GetAllProvidersAsync(CancellationToken ct);

    Task<GatewayConfigDto> CreateConfigAsync(CreateGatewayConfigRequest request, CancellationToken ct);
    Task<GatewayConfigDto> UpdateConfigAsync(Guid configId, UpdateGatewayConfigRequest request, CancellationToken ct);
    Task<GatewayConfigDto> GetConfigAsync(Guid configId, CancellationToken ct);
    Task<List<GatewayConfigDto>> GetAllConfigsAsync(Guid? tenantId, string? applicationCode, int skip, int take, CancellationToken ct);
    Task EnableConfigAsync(Guid configId, CancellationToken ct);
    Task DisableConfigAsync(Guid configId, CancellationToken ct);
    Task SoftDeleteConfigAsync(Guid configId, CancellationToken ct);
    Task<GatewayConfigTestResultDto> TestConfigAsync(Guid configId, CancellationToken ct);
}
