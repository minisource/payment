namespace Application.Features.Wallets;

/// <summary>
/// Service for checking wallet data consistency (balance replay, hash chain, ordering, invariants).
/// </summary>
public interface IWalletConsistencyService
{
    Task<WalletConsistencyResult> CheckWalletAsync(Guid walletId, CancellationToken ct = default);
    Task<WalletConsistencyResult> CheckTenantWalletsAsync(Guid tenantId, CancellationToken ct = default);
}

public class WalletConsistencyResult
{
    public DateTime CheckedAt { get; set; }
    public string Scope { get; set; } = string.Empty;
    public string? ScopeId { get; set; }
    public bool Ok { get; set; } = true;
    public List<WalletConsistencyError> Errors { get; set; } = new();
}

public class WalletConsistencyError
{
    public string Code { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
