using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace payment.Controllers.Admin;

[Route("api/v1/admin")]
[Authorize(Policy = "WalletAdmin")]
public class AdminAuditController : PaymentControllerBase
{
    private readonly PaymentDbContext _db;

    public AdminAuditController(PaymentDbContext db) => _db = db;

    private Guid? TenantFilter => GetEffectiveTenantId();

    [HttpGet("audit/logs")]
    public async Task<ActionResult> ListAuditLogs(
        [FromQuery] Guid? tenantId,
        [FromQuery] string? action,
        [FromQuery] string? entityType,
        [FromQuery] string? entityId,
        [FromQuery] string? actorUserId,
        [FromQuery] string? dateFrom,
        [FromQuery] string? dateTo,
        [FromQuery] string? hashStatus,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 25,
        CancellationToken ct = default)
    {
        var tid = tenantId ?? TenantFilter;
        var q = _db.AuditLogs.AsQueryable();

        if (tid.HasValue) q = q.Where(l => l.TenantId == tid.Value);
        if (!string.IsNullOrEmpty(action)) q = q.Where(l => l.Action == action);
        if (!string.IsNullOrEmpty(entityType)) q = q.Where(l => l.EntityType == entityType);
        if (!string.IsNullOrEmpty(entityId)) q = q.Where(l => l.EntityId == entityId);
        if (!string.IsNullOrEmpty(actorUserId) && Guid.TryParse(actorUserId, out var uid))
            q = q.Where(l => l.ActorUserId == uid);
        if (!string.IsNullOrEmpty(dateFrom) && DateTime.TryParse(dateFrom, out var from))
            q = q.Where(l => l.CreatedAt >= from);
        if (!string.IsNullOrEmpty(dateTo) && DateTime.TryParse(dateTo, out var to))
            q = q.Where(l => l.CreatedAt < to);

        if (!string.IsNullOrEmpty(hashStatus))
        {
            q = hashStatus switch
            {
                "valid" => q.Where(l => l.AuditHash != null),
                "invalid" => q.Where(l => l.AuditHash == null),
                _ => q
            };
        }

        var total = await q.CountAsync(ct);
        var items = await q.OrderByDescending(l => l.CreatedAt).Skip(skip).Take(take)
            .Select(l => new
            {
                l.Id, l.TenantId,
                actorType = "user",
                l.ActorUserId, l.Action,
                l.EntityType, l.EntityId,
                l.IpAddress, l.UserAgent,
                l.RequestId, l.CorrelationId,
                hashStatus = l.AuditHash != null ? "valid" : "unverified",
                l.CreatedAt
            }).ToListAsync(ct);

        return Ok(new { items, total });
    }

    [HttpGet("audit/logs/{auditLogId:guid}")]
    public async Task<ActionResult> GetAuditLog(Guid auditLogId, CancellationToken ct)
    {
        var log = await _db.AuditLogs.FirstOrDefaultAsync(l => l.Id == auditLogId, ct);
        if (log == null) return NotFound(new { error = new { code = "NOT_FOUND", message = "Audit log not found" } });

        return Ok(new
        {
            log.Id, log.TenantId,
            actorType = "user",
            log.ActorUserId, log.Action,
            log.EntityType, log.EntityId,
            log.IpAddress, log.UserAgent,
            log.RequestId, log.CorrelationId,
            hashStatus = log.AuditHash != null ? "valid" : "unverified",
            previousHash = log.PreviousAuditHash,
            entryHash = log.AuditHash,
            hashAlgorithm = log.HashAlgorithm,
            hashVersion = log.HashVersion,
            log.CreatedAt
        });
    }

    [HttpPost("audit/verify-hash-chain")]
    public async Task<ActionResult> VerifyHashChain([FromBody] VerifyHashChainRequest? request, CancellationToken ct)
    {
        var q = _db.AuditLogs.AsQueryable();
        if (request?.TenantId.HasValue == true) q = q.Where(l => l.TenantId == request.TenantId.Value);
        if (request?.AuditLogId != null) q = q.Where(l => l.Id == request.AuditLogId.Value);

        var logs = await q.OrderBy(l => l.CreatedAt).Take(200).ToListAsync(ct);

        bool valid = true;
        string? previousHash = null;
        foreach (var log in logs)
        {
            if (previousHash != null && log.PreviousAuditHash != previousHash)
            {
                valid = false; break;
            }
            previousHash = log.AuditHash;
        }

        return Ok(new { valid, message = valid ? "Hash chain is valid" : "Hash chain verification failed" });
    }
}

public record VerifyHashChainRequest(Guid? TenantId = null, Guid? AuditLogId = null);
