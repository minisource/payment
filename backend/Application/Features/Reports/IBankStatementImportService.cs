using Domain.Entities;

namespace Application.Features.Reports;

/// <summary>
/// Service for importing and managing bank statement entries.
/// </summary>
public interface IBankStatementImportService
{
    Task<BankStatementImport> ImportCsvAsync(Guid tenantId, Guid userId, string fileName, string csvContent, CancellationToken ct);
    Task<BankStatementImport?> GetImportAsync(Guid importId, CancellationToken ct);
    Task<List<BankStatementImport>> ListImportsAsync(Guid tenantId, int skip, int take, CancellationToken ct);
    Task<List<BankStatementEntry>> GetEntriesAsync(Guid importId, int skip, int take, CancellationToken ct);
    Task MatchEntryAsync(Guid entryId, string entityType, string entityId, Guid matchedByUserId, CancellationToken ct);
    Task IgnoreEntryAsync(Guid entryId, CancellationToken ct);
}
