using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly PaymentDbContext _ctx;
    public AuditLogRepository(PaymentDbContext ctx) => _ctx = ctx;

    public async Task AddAsync(AuditLog log, CancellationToken ct = default)
    {
        // Auto-compute audit hash chain for tamper evidence
        if (string.IsNullOrEmpty(log.AuditHash))
        {
            var previousHash = await _ctx.AuditLogs
                .Where(a => a.TenantId == log.TenantId && !string.IsNullOrEmpty(a.AuditHash))
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => a.AuditHash)
                .FirstOrDefaultAsync(ct);
            log.PreviousAuditHash = previousHash;
            log.AuditHash = ComputeAuditHash(log, previousHash);
            log.HashAlgorithm = "SHA256";
            log.HashVersion = 1;
        }
        await _ctx.AuditLogs.AddAsync(log, ct);
    }

    private static string ComputeAuditHash(AuditLog log, string? previousHash)
    {
        var canonical = string.Join("|", new[]
        {
            log.TenantId?.ToString() ?? "", log.ActorUserId?.ToString() ?? "",
            log.Action, log.EntityType, log.EntityId,
            log.Reason ?? "", log.RequestId ?? "", log.CorrelationId ?? "",
            log.CreatedAt.ToString("O"), previousHash ?? "GENESIS"
        });
        return Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(canonical)));
    }

    public async Task<List<AuditLog>> GetByEntityAsync(string entityType, string entityId, int skip = 0, int take = 50, CancellationToken ct = default)
        => await _ctx.AuditLogs
            .Where(l => l.EntityType == entityType && l.EntityId == entityId)
            .OrderByDescending(l => l.CreatedAt).Skip(skip).Take(take).ToListAsync(ct);
}
