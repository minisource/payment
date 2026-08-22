namespace Application.Features.Limits;

/// <summary>
/// Service for tracking and enforcing per-tenant/operator limit usage (daily/monthly count/amount).
/// </summary>
public interface ILimitUsageService
{
    Task TrackUsageAsync(Guid tenantId, string? ownerType, Guid? ownerId, string operationType,
        string? currency, decimal amount, CancellationToken ct);
    Task<(bool allowed, string? reason)> CheckLimitsAsync(Guid tenantId, string? ownerType, Guid? ownerId,
        string operationType, string? currency, decimal amount, CancellationToken ct);
}
