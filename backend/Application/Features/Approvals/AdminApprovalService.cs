using System.Text.Json;
using Domain.Entities;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Minisource.Common.Domain;
using Minisource.Common.Exceptions;

namespace Application.Features.Approvals;

public class AdminApprovalService : IAdminApprovalService
{
    private readonly ISettingsRepository _settingsRepo;
    private readonly IAuditLogRepository _auditRepo;
    private readonly Infrastructure.Persistence.PaymentDbContext _ctx;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<AdminApprovalService> _logger;

    public AdminApprovalService(ISettingsRepository settingsRepo, IAuditLogRepository auditRepo,
        Infrastructure.Persistence.PaymentDbContext ctx, IUnitOfWork uow, ILogger<AdminApprovalService> logger)
    { _settingsRepo = settingsRepo; _auditRepo = auditRepo; _ctx = ctx; _uow = uow; _logger = logger; }

    public async Task<AdminApprovalRequest> RequestApprovalAsync(Guid tenantId, Guid requestedByUserId, string operationType,
        string entityType, string? entityId, object requestPayload, CancellationToken ct)
    {
        var request = new AdminApprovalRequest
        {
            TenantId = tenantId, OperationType = operationType,
            EntityType = entityType, EntityId = entityId,
            RequestedByUserId = requestedByUserId,
            RequestPayload = JsonSerializer.Serialize(requestPayload),
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        };

        await _ctx.AdminApprovalRequests.AddAsync(request, ct);
        await _auditRepo.AddAsync(new AuditLog { TenantId = tenantId, ActorUserId = requestedByUserId, Action = "admin_approval.requested", EntityType = "AdminApprovalRequest", EntityId = request.Id.ToString() }, ct);
        await _uow.SaveChangesAsync(ct);
        _logger.LogInformation("Admin approval request {Id} created for {Operation}", request.Id, operationType);
        return request;
    }

    public async Task<AdminApprovalRequest?> GetRequestAsync(Guid id, CancellationToken ct)
        => await _ctx.AdminApprovalRequests.FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<List<AdminApprovalRequest>> GetPendingAsync(Guid tenantId, int skip, int take, CancellationToken ct)
        => await _ctx.AdminApprovalRequests
            .Where(r => r.TenantId == tenantId && r.Status == "pending")
            .OrderByDescending(r => r.RequestedAt)
            .Skip(skip).Take(take).ToListAsync(ct);

    public async Task ApproveAsync(Guid id, Guid approvedByUserId, string? reason, CancellationToken ct)
    {
        var req = await GetRequestAsync(id, ct) ?? throw new NotFoundException("AdminApprovalRequest", id);
        if (req.RequestedByUserId == approvedByUserId)
            throw new BusinessException("Cannot approve own request", "admin_approval_self_approval_not_allowed");
        req.Approve(approvedByUserId);
        await _auditRepo.AddAsync(new AuditLog { TenantId = req.TenantId, ActorUserId = approvedByUserId, Action = "admin_approval.approved", EntityType = "AdminApprovalRequest", EntityId = id.ToString(), Reason = reason }, ct);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task RejectAsync(Guid id, Guid rejectedByUserId, string reason, CancellationToken ct)
    {
        var req = await GetRequestAsync(id, ct) ?? throw new NotFoundException("AdminApprovalRequest", id);
        req.Reject(rejectedByUserId, reason);
        await _auditRepo.AddAsync(new AuditLog { TenantId = req.TenantId, ActorUserId = rejectedByUserId, Action = "admin_approval.rejected", EntityType = "AdminApprovalRequest", EntityId = id.ToString(), Reason = reason }, ct);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task ExecuteAsync(Guid id, Guid executedByUserId, CancellationToken ct)
    {
        var req = await GetRequestAsync(id, ct) ?? throw new NotFoundException("AdminApprovalRequest", id);
        if (req.Status != "approved") throw new BusinessException("Request must be approved first", "admin_approval_invalid_state");
        req.Execute(executedByUserId);
        await _auditRepo.AddAsync(new AuditLog { TenantId = req.TenantId, ActorUserId = executedByUserId, Action = "admin_approval.executed", EntityType = "AdminApprovalRequest", EntityId = id.ToString() }, ct);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task CancelAsync(Guid id, CancellationToken ct)
    {
        var req = await GetRequestAsync(id, ct) ?? throw new NotFoundException("AdminApprovalRequest", id);
        req.Cancel();
        await _auditRepo.AddAsync(new AuditLog { TenantId = req.TenantId, Action = "admin_approval.cancelled", EntityType = "AdminApprovalRequest", EntityId = id.ToString() }, ct);
        await _uow.SaveChangesAsync(ct);
        _logger.LogInformation("Admin approval request {Id} cancelled", id);
    }
}
