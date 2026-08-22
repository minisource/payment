using Minisource.Common.Domain;

namespace Domain.Entities;

public class BankStatementImport : Entity<Guid>
{
    public Guid TenantId { get; set; }
    public string Source { get; set; } = "manual_csv";
    public string? BankName { get; set; }
    public string? AccountIdentifierMasked { get; set; }
    public string Status { get; set; } = "uploaded";
    public Guid? FileId { get; set; }
    public string? OriginalFileName { get; set; }
    public Guid? ImportedByUserId { get; set; }
    public int TotalRows { get; set; }
    public int ParsedRows { get; set; }
    public int FailedRows { get; set; }
    public string? Metadata { get; set; }
    public DateTime? ParsedAt { get; set; }
    public BankStatementImport() { Id = Guid.NewGuid(); }
}

public class BankStatementEntry : Entity<Guid>
{
    public Guid TenantId { get; set; }
    public Guid ImportId { get; set; }
    public DateTime TransactionDate { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "IRT";
    public string Direction { get; set; } = "credit";
    public string? BankTrackingNumber { get; set; }
    public string? BankReferenceId { get; set; }
    public string? Description { get; set; }
    public string? CounterpartyName { get; set; }
    public string? CounterpartyAccountMasked { get; set; }
    public string? RawRowSafe { get; set; }
    public string? MatchedEntityType { get; set; }
    public string? MatchedEntityId { get; set; }
    public string MatchStatus { get; set; } = "unmatched";
    public DateTime? MatchedAt { get; set; }
    public Guid? MatchedByUserId { get; set; }
    public BankStatementEntry() { Id = Guid.NewGuid(); }
}
