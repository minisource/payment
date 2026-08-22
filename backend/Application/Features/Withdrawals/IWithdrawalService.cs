using Application.DTOs;

namespace Application.Features.Withdrawals;

/// <summary>
/// Service for managing withdrawal requests (user and admin).
/// </summary>
public interface IWithdrawalService
{
    Task<WithdrawalRequestDto> CreateAsync(Guid tenantId, Guid ownerId, string ownerType, CreateWithdrawalRequest request,
        string? applicationCode, string? idempotencyKey, CancellationToken ct);
    Task<List<WithdrawalRequestDto>> GetMyRequestsAsync(Guid tenantId, string ownerType, Guid ownerId, int skip, int take, CancellationToken ct);
    Task<WithdrawalRequestDto> GetAsync(Guid withdrawalId, CancellationToken ct);
    Task<WithdrawalRequestDto> CancelAsync(Guid withdrawalId, Guid tenantId, Guid ownerId, string reason, CancellationToken ct);
    Task<List<WithdrawalRequestDto>> AdminListAsync(Guid? tenantId, Domain.Entities.WithdrawalStatus? status, int skip, int take, CancellationToken ct);
    Task<WithdrawalRequestDto> AdminGetAsync(Guid withdrawalId, CancellationToken ct);
    Task<WithdrawalRequestDto> ApproveAsync(Guid withdrawalId, Guid actorUserId, string? note, CancellationToken ct);
    Task<WithdrawalRequestDto> RejectAsync(Guid withdrawalId, Guid actorUserId, string reason, CancellationToken ct);
    Task<WithdrawalRequestDto> RequestMoreInfoAsync(Guid withdrawalId, Guid actorUserId, string reason, CancellationToken ct);
    Task<WithdrawalRequestDto> MarkProcessingAsync(Guid withdrawalId, Guid actorUserId, string? note, CancellationToken ct);
    Task<WithdrawalRequestDto> MarkPaidAsync(Guid withdrawalId, Guid actorUserId, AdminWithdrawalMarkPaidRequest request, CancellationToken ct);
    Task<WithdrawalRequestDto> MarkFailedAsync(Guid withdrawalId, Guid actorUserId, AdminWithdrawalMarkFailedRequest request, CancellationToken ct);
    Task<WithdrawalSummaryDto> GetSummaryAsync(Guid? tenantId, CancellationToken ct);
    Task<List<WithdrawalRequestDto>> GetPendingAsync(Guid? tenantId, int skip, int take, CancellationToken ct);
}
