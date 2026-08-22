using Domain.Entities;

namespace Application.Features.Approvals;

/// <summary>
/// Service for managing admin approval requests (maker-checker workflow).
/// </summary>
public interface IAdminApprovalService
{
    Task<AdminApprovalRequest> RequestApprovalAsync(Guid tenantId, Guid requestedByUserId, string operationType,
        string entityType, string? entityId, object requestPayload, CancellationToken ct);
    Task<AdminApprovalRequest?> GetRequestAsync(Guid id, CancellationToken ct);
    Task<List<AdminApprovalRequest>> GetPendingAsync(Guid tenantId, int skip, int take, CancellationToken ct);
    Task ApproveAsync(Guid id, Guid approvedByUserId, string? reason, CancellationToken ct);
    Task RejectAsync(Guid id, Guid rejectedByUserId, string reason, CancellationToken ct);
    Task ExecuteAsync(Guid id, Guid executedByUserId, CancellationToken ct);
    Task CancelAsync(Guid id, CancellationToken ct);
}
