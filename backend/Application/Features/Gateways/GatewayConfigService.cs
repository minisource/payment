using Application.DTOs;
using Domain.Entities;
using Domain.Enums;
using Domain.Gateways;
using Domain.Repositories;
using Microsoft.Extensions.Logging;
using Minisource.Common.Domain;
using Minisource.Common.Exceptions;

namespace Application.Features.Gateways;

public class GatewayConfigService : IGatewayConfigService
{
    private readonly IGatewayProviderRepository _providerRepo;
    private readonly IGatewayConfigRepository _configRepo;
    private readonly IGatewaySecretProtector _secretProtector;
    private readonly IAuditLogRepository _auditRepo;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<GatewayConfigService> _logger;

    public GatewayConfigService(
        IGatewayProviderRepository providerRepo,
        IGatewayConfigRepository configRepo,
        IGatewaySecretProtector secretProtector,
        IAuditLogRepository auditRepo,
        IUnitOfWork uow,
        ILogger<GatewayConfigService> logger)
    {
        _providerRepo = providerRepo;
        _configRepo = configRepo;
        _secretProtector = secretProtector;
        _auditRepo = auditRepo;
        _uow = uow;
        _logger = logger;
    }

    public async Task<GatewayProviderDto> CreateProviderAsync(CreateGatewayProviderRequest request, CancellationToken ct)
    {
        var existing = await _providerRepo.GetByCodeAsync(request.Code, ct);
        if (existing != null)
            throw new ConflictException($"Provider '{request.Code}' already exists");

        var provider = new GatewayProvider
        {
            Code = request.Code.ToLowerInvariant(),
            DisplayName = request.DisplayName,
            AdapterType = request.AdapterType,
            SupportedCurrencies = request.SupportedCurrencies,
            RequiredConfigSchema = request.RequiredConfigSchema,
            OptionalConfigSchema = request.OptionalConfigSchema,
            Metadata = request.Metadata
        };

        await _providerRepo.AddAsync(provider, ct);
        await _uow.SaveChangesAsync(ct);

        await _auditRepo.AddAsync(new AuditLog
        {
            Action = "gateway_provider.created", EntityType = "GatewayProvider",
            EntityId = provider.Code, AfterSnapshot = System.Text.Json.JsonSerializer.Serialize(provider)
        }, ct);
        await _uow.SaveChangesAsync(ct);

        return MapProvider(provider);
    }

    public async Task<GatewayProviderDto> UpdateProviderAsync(string code, UpdateGatewayProviderRequest request, CancellationToken ct)
    {
        var provider = await _providerRepo.GetByCodeAsync(code, ct)
            ?? throw new NotFoundException("GatewayProvider", code);

        if (request.DisplayName != null) provider.DisplayName = request.DisplayName;
        if (request.Status != null && Enum.TryParse<GatewayProviderStatus>(request.Status, true, out var s))
            provider.Status = s;
        if (request.SupportedCurrencies != null) provider.SupportedCurrencies = request.SupportedCurrencies;
        if (request.RequiredConfigSchema != null) provider.RequiredConfigSchema = request.RequiredConfigSchema;
        if (request.OptionalConfigSchema != null) provider.OptionalConfigSchema = request.OptionalConfigSchema;

        await _providerRepo.UpdateAsync(provider, ct);
        await _uow.SaveChangesAsync(ct);

        await _auditRepo.AddAsync(new AuditLog
        {
            Action = "gateway_provider.updated", EntityType = "GatewayProvider", EntityId = code
        }, ct);
        await _uow.SaveChangesAsync(ct);

        return MapProvider(provider);
    }

    public async Task<GatewayProviderDto?> GetProviderAsync(string code, CancellationToken ct)
    {
        var provider = await _providerRepo.GetByCodeAsync(code, ct);
        return provider == null ? null : MapProvider(provider);
    }

    public async Task<List<GatewayProviderDto>> GetAllProvidersAsync(CancellationToken ct)
    {
        var providers = await _providerRepo.GetAllAsync(ct);
        return providers.Select(MapProvider).ToList();
    }

    public async Task<GatewayConfigDto> CreateConfigAsync(CreateGatewayConfigRequest request, CancellationToken ct)
    {
        var provider = await _providerRepo.GetByCodeAsync(request.ProviderCode, ct)
            ?? throw new BusinessException("Provider not found", "gateway_provider_not_found");

        if (provider.Status != GatewayProviderStatus.Active)
            throw new BusinessException("Provider is not active", "gateway_provider_disabled");

        // Encrypt secrets using provider schema
        var encryptedSecrets = await _secretProtector.EncryptSecretsAsync(
            request.ProviderCode, request.Config, provider.RequiredConfigSchema, ct);

        // Redact secrets from config_json
        var redactedConfig = _secretProtector.RedactSecrets(request.Config, null, provider.RequiredConfigSchema);

        var config = new GatewayConfig
        {
            TenantId = request.TenantId,
            ApplicationCode = request.ApplicationCode,
            ProviderCode = request.ProviderCode,
            AdapterType = request.AdapterType,
            Name = request.Name,
            Environment = request.Environment,
            IsDefault = request.IsDefault,
            Priority = request.Priority,
            Weight = request.Weight,
            SupportedCurrencies = request.SupportedCurrencies,
            MinAmount = request.MinAmount,
            MaxAmount = request.MaxAmount,
            ConfigJson = redactedConfig,
            EncryptedSecrets = encryptedSecrets,
            CallbackBaseUrl = request.CallbackBaseUrl,
            Metadata = request.Metadata
        };

        await _configRepo.AddAsync(config, ct);
        await _uow.SaveChangesAsync(ct);

        await _auditRepo.AddAsync(new AuditLog
        {
            TenantId = request.TenantId,
            Action = "gateway_config.created", EntityType = "GatewayConfig",
            EntityId = config.Id.ToString(), AfterSnapshot = System.Text.Json.JsonSerializer.Serialize(new {
                config.Name, config.ProviderCode, config.Environment, config.Status, config.Scope
            })
        }, ct);
        await _uow.SaveChangesAsync(ct);

        return MapConfig(config);
    }

    public async Task<GatewayConfigDto> UpdateConfigAsync(Guid configId, UpdateGatewayConfigRequest request, CancellationToken ct)
    {
        var config = await _configRepo.GetByIdAsync(configId, ct)
            ?? throw new NotFoundException("GatewayConfig", configId);

        if (request.Name != null) config.Name = request.Name;
        if (request.Status != null && Enum.TryParse<GatewayConfigStatus>(request.Status, true, out var s))
            config.Status = s;
        if (request.IsDefault.HasValue) config.IsDefault = request.IsDefault.Value;
        if (request.Priority.HasValue) config.Priority = request.Priority.Value;
        if (request.Weight.HasValue) config.Weight = request.Weight.Value;
        if (request.SupportedCurrencies != null) config.SupportedCurrencies = request.SupportedCurrencies;
        if (request.MinAmount.HasValue) config.MinAmount = request.MinAmount;
        if (request.MaxAmount.HasValue) config.MaxAmount = request.MaxAmount;
        if (request.Config != null)
        {
            var provider = await _providerRepo.GetByCodeAsync(config.ProviderCode, ct);
            if (provider != null)
            {
                config.EncryptedSecrets = await _secretProtector.EncryptSecretsAsync(
                    config.ProviderCode, request.Config, provider.RequiredConfigSchema, ct);
                config.ConfigJson = _secretProtector.RedactSecrets(request.Config, null, provider.RequiredConfigSchema);
            }
        }
        if (request.CallbackBaseUrl != null) config.CallbackBaseUrl = request.CallbackBaseUrl;
        if (request.Metadata != null) config.Metadata = request.Metadata;

        await _configRepo.UpdateAsync(config, ct);
        await _uow.SaveChangesAsync(ct);

        await _auditRepo.AddAsync(new AuditLog
        {
            TenantId = config.TenantId,
            Action = "gateway_config.updated", EntityType = "GatewayConfig", EntityId = configId.ToString()
        }, ct);
        await _uow.SaveChangesAsync(ct);

        return MapConfig(config);
    }

    public async Task<GatewayConfigDto> GetConfigAsync(Guid configId, CancellationToken ct)
    {
        var config = await _configRepo.GetByIdAsync(configId, ct)
            ?? throw new NotFoundException("GatewayConfig", configId);
        return MapConfig(config);
    }

    public async Task<List<GatewayConfigDto>> GetAllConfigsAsync(Guid? tenantId, string? applicationCode, int skip, int take, CancellationToken ct)
    {
        var configs = await _configRepo.GetAllForAdminAsync(tenantId, applicationCode, skip, take, ct);
        return configs.Select(MapConfig).ToList();
    }

    public async Task EnableConfigAsync(Guid configId, CancellationToken ct)
    {
        var config = await _configRepo.GetByIdAsync(configId, ct)
            ?? throw new NotFoundException("GatewayConfig", configId);
        config.Enable();
        await _configRepo.UpdateAsync(config, ct);
        await _uow.SaveChangesAsync(ct);

        await _auditRepo.AddAsync(new AuditLog { TenantId = config.TenantId, Action = "gateway_config.enabled", EntityType = "GatewayConfig", EntityId = configId.ToString() }, ct);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task DisableConfigAsync(Guid configId, CancellationToken ct)
    {
        var config = await _configRepo.GetByIdAsync(configId, ct)
            ?? throw new NotFoundException("GatewayConfig", configId);
        config.Disable();
        await _configRepo.UpdateAsync(config, ct);
        await _uow.SaveChangesAsync(ct);

        await _auditRepo.AddAsync(new AuditLog { TenantId = config.TenantId, Action = "gateway_config.disabled", EntityType = "GatewayConfig", EntityId = configId.ToString() }, ct);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task SoftDeleteConfigAsync(Guid configId, CancellationToken ct)
    {
        var config = await _configRepo.GetByIdAsync(configId, ct)
            ?? throw new NotFoundException("GatewayConfig", configId);
        config.SoftDelete();
        await _configRepo.UpdateAsync(config, ct);
        await _uow.SaveChangesAsync(ct);

        await _auditRepo.AddAsync(new AuditLog { TenantId = config.TenantId, Action = "gateway_config.deleted", EntityType = "GatewayConfig", EntityId = configId.ToString() }, ct);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task<GatewayConfigTestResultDto> TestConfigAsync(Guid configId, CancellationToken ct)
    {
        var config = await _configRepo.GetByIdAsync(configId, ct)
            ?? throw new NotFoundException("GatewayConfig", configId);

        var provider = await _providerRepo.GetByCodeAsync(config.ProviderCode, ct);

        var issues = new List<string>();
        var status = "healthy";

        // Check 1: Provider exists and is active
        if (provider == null)
        {
            issues.Add($"Provider '{config.ProviderCode}' not found in catalog");
            return new GatewayConfigTestResultDto(false, "unhealthy", config.ProviderCode,
                "Provider not found", DateTime.UtcNow, null);
        }

        if (provider.Status != GatewayProviderStatus.Active)
        {
            issues.Add($"Provider '{config.ProviderCode}' is {provider.Status.ToString().ToLower()}");
            status = "degraded";
        }

        // Check 2: Config status
        if (config.Status != GatewayConfigStatus.Active)
        {
            issues.Add($"Config is {config.Status.ToString().ToLower()}");
            status = "degraded";
        }

        // Check 3: Secrets configured
        var hasSecrets = !string.IsNullOrWhiteSpace(config.EncryptedSecrets);
        if (!hasSecrets)
        {
            issues.Add("No credentials configured — secret fields are empty");
            status = status == "healthy" ? "degraded" : status;
        }

        // Check 4: Supported currencies
        if (config.SupportedCurrencies.Count == 0)
        {
            issues.Add("No supported currencies configured");
            status = status == "healthy" ? "degraded" : status;
        }

        // Check 5: Health history
        if (config.FailureCount > 0)
        {
            issues.Add($"{config.FailureCount} previous failure(s) — last failure: {config.LastFailureAt:yyyy-MM-dd}");
            status = "degraded";
        }

        var message = issues.Count > 0
            ? string.Join("; ", issues)
            : "All checks passed — gateway config is healthy";

        _logger.LogInformation("Gateway config {ConfigId} test: {Status} — {Message}", configId, status, message);

        return new GatewayConfigTestResultDto(
            status == "healthy",
            status,
            config.ProviderCode,
            message,
            DateTime.UtcNow,
            issues.Count > 0 ? issues : null);
    }

    private static GatewayProviderDto MapProvider(GatewayProvider p) => new()
    {
        Id = p.Id, Code = p.Code, DisplayName = p.DisplayName,
        AdapterType = p.AdapterType, Status = p.Status.ToString(),
        SupportedCurrencies = p.SupportedCurrencies,
        RequiredConfigSchema = p.RequiredConfigSchema,
        OptionalConfigSchema = p.OptionalConfigSchema,
        CreatedAt = p.CreatedAt, UpdatedAt = p.UpdatedAt
    };

    public static GatewayConfigDto MapConfig(GatewayConfig c) => new()
    {
        Id = c.Id, TenantId = c.TenantId, ApplicationCode = c.ApplicationCode,
        ProviderCode = c.ProviderCode, AdapterType = c.AdapterType,
        Name = c.Name, Environment = c.Environment, Status = c.Status.ToString(),
        IsDefault = c.IsDefault, Priority = c.Priority, Weight = c.Weight,
        SupportedCurrencies = c.SupportedCurrencies, MinAmount = c.MinAmount, MaxAmount = c.MaxAmount,
        ConfigJson = c.ConfigJson, CallbackBaseUrl = c.CallbackBaseUrl,
        HealthStatus = c.HealthStatus.ToString(), FailureCount = c.FailureCount,
        LastSuccessAt = c.LastSuccessAt, LastFailureAt = c.LastFailureAt,
        CreatedAt = c.CreatedAt, UpdatedAt = c.UpdatedAt
    };
}
