using Application.DTOs;

namespace Application.Features.PayoutAccounts;

/// <summary>
/// Service for managing payout accounts (user and admin).
/// </summary>
public interface IPayoutAccountService
{
    Task<PayoutAccountDto> CreateAsync(Guid tenantId, Guid ownerId, string ownerType, CreatePayoutAccountRequest request, CancellationToken ct);
    Task<List<PayoutAccountDto>> GetMyAccountsAsync(Guid tenantId, string ownerType, Guid ownerId, CancellationToken ct);
    Task<PayoutAccountDto> GetAsync(Guid accountId, CancellationToken ct);
    Task<PayoutAccountDto> UpdateAsync(Guid accountId, Guid tenantId, Guid ownerId, UpdatePayoutAccountRequest request, CancellationToken ct);
    Task SetDefaultAsync(Guid accountId, Guid tenantId, Guid ownerId, CancellationToken ct);
    Task SoftDeleteAsync(Guid accountId, Guid tenantId, Guid ownerId, CancellationToken ct);
    Task<List<PayoutAccountDto>> AdminListAsync(Guid? tenantId, Domain.Entities.PayoutAccountStatus? status, int skip, int take, CancellationToken ct);
    Task<PayoutAccountDto> AdminGetAsync(Guid accountId, CancellationToken ct);
    Task AdminVerifyAsync(Guid accountId, Guid actorUserId, CancellationToken ct);
    Task AdminRejectAsync(Guid accountId, Guid actorUserId, string reason, CancellationToken ct);
    Task AdminDisableAsync(Guid accountId, CancellationToken ct);
}
