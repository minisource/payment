using System.Text.Json;
using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Minisource.Common.Domain;

namespace Application.Features.Reports;

public class BankStatementImportService : IBankStatementImportService
{
    private readonly PaymentDbContext _ctx;
    private readonly IAuditLogRepository _auditRepo;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<BankStatementImportService> _logger;

    public BankStatementImportService(PaymentDbContext ctx, IAuditLogRepository auditRepo,
        IUnitOfWork uow, ILogger<BankStatementImportService> logger)
    { _ctx = ctx; _auditRepo = auditRepo; _uow = uow; _logger = logger; }

    public async Task<BankStatementImport> ImportCsvAsync(Guid tenantId, Guid userId, string fileName, string csvContent, CancellationToken ct)
    {
        var import = new BankStatementImport
        {
            TenantId = tenantId, Source = "manual_csv", OriginalFileName = fileName,
            ImportedByUserId = userId, Status = "uploaded", TotalRows = 0
        };
        await _ctx.BankStatementImports.AddAsync(import, ct);

        var lines = csvContent.Replace("\r\n", "\n").Split('\n', StringSplitOptions.RemoveEmptyEntries);
        import.TotalRows = lines.Length;
        int parsed = 0, failed = 0;

        // Skip header line
        for (int i = 1; i < lines.Length; i++)
        {
            try
            {
                var entry = ParseCsvLine(tenantId, import.Id, lines[i]);
                if (entry != null)
                {
                    await _ctx.BankStatementEntries.AddAsync(entry, ct);
                    parsed++;
                }
                else { failed++; }
            }
            catch { failed++; }
        }

        import.ParsedRows = parsed;
        import.FailedRows = failed;
        import.Status = "parsed";
        import.ParsedAt = DateTime.UtcNow;

        await _auditRepo.AddAsync(new AuditLog
        {
            TenantId = tenantId, ActorUserId = userId,
            Action = "bank_statement.imported", EntityType = "BankStatementImport", EntityId = import.Id.ToString(),
            Reason = $"{parsed} rows parsed, {failed} failed"
        }, ct);

        await _uow.SaveChangesAsync(ct);
        _logger.LogInformation("Bank statement import {Id}: {Parsed}/{Total} rows parsed", import.Id, parsed, import.TotalRows);
        return import;
    }

    public async Task<BankStatementImport?> GetImportAsync(Guid importId, CancellationToken ct)
        => await _ctx.BankStatementImports.FirstOrDefaultAsync(i => i.Id == importId, ct);

    public async Task<List<BankStatementImport>> ListImportsAsync(Guid tenantId, int skip, int take, CancellationToken ct)
        => await _ctx.BankStatementImports
            .Where(i => i.TenantId == tenantId)
            .OrderByDescending(i => i.CreatedAt).Skip(skip).Take(take).ToListAsync(ct);

    public async Task<List<BankStatementEntry>> GetEntriesAsync(Guid importId, int skip, int take, CancellationToken ct)
        => await _ctx.BankStatementEntries
            .Where(e => e.ImportId == importId)
            .OrderByDescending(e => e.TransactionDate).Skip(skip).Take(take).ToListAsync(ct);

    public async Task MatchEntryAsync(Guid entryId, string entityType, string entityId, Guid matchedByUserId, CancellationToken ct)
    {
        var entry = await _ctx.BankStatementEntries.FirstOrDefaultAsync(e => e.Id == entryId, ct)
            ?? throw new Minisource.Common.Exceptions.NotFoundException("BankStatementEntry", entryId);
        if (entry.MatchStatus == "matched")
            throw new Minisource.Common.Exceptions.BusinessException("Entry already matched", "bank_statement_entry_already_matched");

        entry.MatchStatus = "matched";
        entry.MatchedEntityType = entityType;
        entry.MatchedEntityId = entityId;
        entry.MatchedAt = DateTime.UtcNow;
        entry.MatchedByUserId = matchedByUserId;

        await _auditRepo.AddAsync(new AuditLog
        {
            TenantId = entry.TenantId, ActorUserId = matchedByUserId,
            Action = "bank_statement.entry_matched", EntityType = "BankStatementEntry", EntityId = entryId.ToString(),
            Reason = $"Matched to {entityType}:{entityId}"
        }, ct);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task IgnoreEntryAsync(Guid entryId, CancellationToken ct)
    {
        var entry = await _ctx.BankStatementEntries.FirstOrDefaultAsync(e => e.Id == entryId, ct)
            ?? throw new Minisource.Common.Exceptions.NotFoundException("BankStatementEntry", entryId);
        entry.MatchStatus = "ignored";
        await _uow.SaveChangesAsync(ct);
    }

    private static BankStatementEntry? ParseCsvLine(Guid tenantId, Guid importId, string line)
    {
        var cols = line.Split(',', StringSplitOptions.TrimEntries);
        if (cols.Length < 4) return null;

        // Expected CSV: date, amount, currency, direction, description, tracking_number, counterparty
        if (!DateTime.TryParse(cols[0], out var date)) return null;
        if (!decimal.TryParse(cols[1], out var amount)) return null;
        var currency = cols.Length > 2 ? cols[2].Trim() : "IRT";
        var direction = cols.Length > 3 ? cols[3].Trim().ToLowerInvariant() : "credit";
        if (direction != "credit" && direction != "debit") direction = "credit";
        var description = cols.Length > 4 ? cols[4].Trim() : null;
        var trackingNumber = cols.Length > 5 ? cols[5].Trim() : null;
        var counterparty = cols.Length > 6 ? cols[6].Trim() : null;

        return new BankStatementEntry
        {
            TenantId = tenantId, ImportId = importId,
            TransactionDate = date.ToUniversalTime(),
            Amount = Math.Abs(amount), Currency = currency, Direction = direction,
            BankTrackingNumber = trackingNumber, Description = description,
            CounterpartyName = counterparty,
            RawRowSafe = JsonSerializer.Serialize(new { line_truncated = line[..Math.Min(line.Length, 200)] })
        };
    }
}
