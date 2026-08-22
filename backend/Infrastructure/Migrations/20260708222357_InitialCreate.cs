using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdminApprovalDecisions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApprovalRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Decision = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DecidedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DecidedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminApprovalDecisions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AdminApprovalPolicies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OperationType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Enabled = table.Column<bool>(type: "bit", nullable: false),
                    AmountThreshold = table.Column<decimal>(type: "decimal(30,10)", precision: 30, scale: 10, nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequiredApprovals = table.Column<int>(type: "int", nullable: false),
                    RequireDifferentUser = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminApprovalPolicies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AdminApprovalRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OperationType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RequestedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ApprovedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RejectedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequiredApprovals = table.Column<int>(type: "int", nullable: false),
                    ApprovalCount = table.Column<int>(type: "int", nullable: false),
                    RequestPayload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RiskEvaluationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminApprovalRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ActorUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ActorClientId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Action = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BeforeSnapshot = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AfterSnapshot = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequestId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CorrelationId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PreviousAuditHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AuditHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HashAlgorithm = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HashVersion = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BankStatementEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ImportId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(30,10)", precision: 30, scale: 10, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Direction = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BankTrackingNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BankReferenceId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CounterpartyName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CounterpartyAccountMasked = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RawRowSafe = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MatchedEntityType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MatchedEntityId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MatchStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MatchedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MatchedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankStatementEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BankStatementImports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Source = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BankName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AccountIdentifierMasked = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OriginalFileName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImportedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TotalRows = table.Column<int>(type: "int", nullable: false),
                    ParsedRows = table.Column<int>(type: "int", nullable: false),
                    FailedRows = table.Column<int>(type: "int", nullable: false),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ParsedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankStatementImports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EventRoutingRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApplicationCode = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    EventType = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    EventVersion = table.Column<int>(type: "int", nullable: true),
                    Target = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Enabled = table.Column<bool>(type: "bit", nullable: false),
                    Filter = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventRoutingRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FeeRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OperationType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FeeType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FixedAmount = table.Column<decimal>(type: "decimal(30,10)", precision: 30, scale: 10, nullable: true),
                    PercentRate = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: true),
                    MinFee = table.Column<decimal>(type: "decimal(30,10)", precision: 30, scale: 10, nullable: true),
                    MaxFee = table.Column<decimal>(type: "decimal(30,10)", precision: 30, scale: 10, nullable: true),
                    Payer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeeRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GatewayConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApplicationCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ProviderCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AdapterType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Environment = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    Weight = table.Column<int>(type: "int", nullable: false),
                    SupportedCurrencies = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MinAmount = table.Column<decimal>(type: "decimal(30,10)", precision: 30, scale: 10, nullable: true),
                    MaxAmount = table.Column<decimal>(type: "decimal(30,10)", precision: 30, scale: 10, nullable: true),
                    ConfigJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EncryptedSecrets = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CallbackBaseUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    HealthStatus = table.Column<int>(type: "int", nullable: false),
                    LastSuccessAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastFailureAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FailureCount = table.Column<int>(type: "int", nullable: false),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DisabledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GatewayConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GatewayProviders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AdapterType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SupportedCurrencies = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SupportedOperations = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequiredConfigSchema = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OptionalConfigSchema = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GatewayProviders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GatewayRoutingPolicies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApplicationCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Strategy = table.Column<int>(type: "int", nullable: false),
                    FallbackEnabled = table.Column<bool>(type: "bit", nullable: false),
                    RandomizeSamePriority = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GatewayRoutingPolicies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IdempotencyRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdempotencyKey = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RequestHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Operation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ResponseStatusCode = table.Column<int>(type: "int", nullable: true),
                    ResponseBody = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResourceType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResourceId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LockedUntil = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedByClientId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdempotencyRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LimitRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OperationType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MinAmount = table.Column<decimal>(type: "decimal(30,10)", precision: 30, scale: 10, nullable: true),
                    MaxAmount = table.Column<decimal>(type: "decimal(30,10)", precision: 30, scale: 10, nullable: true),
                    DailyAmountLimit = table.Column<decimal>(type: "decimal(30,10)", precision: 30, scale: 10, nullable: true),
                    MonthlyAmountLimit = table.Column<decimal>(type: "decimal(30,10)", precision: 30, scale: 10, nullable: true),
                    DailyCountLimit = table.Column<int>(type: "int", nullable: true),
                    MonthlyCountLimit = table.Column<int>(type: "int", nullable: true),
                    RequiresKyc = table.Column<bool>(type: "bit", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LimitRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OutboxEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApplicationCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EventType = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EventVersion = table.Column<int>(type: "int", nullable: false),
                    AggregateType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AggregateId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Headers = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AvailableAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LockedUntil = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LockedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    MaxRetryCount = table.Column<int>(type: "int", nullable: false),
                    NextRetryAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastErrorAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeadLetteredAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ParbadPayments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TrackingNumber = table.Column<long>(type: "bigint", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TransactionCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GatewayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GatewayAccountName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    IsPaid = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParbadPayments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaymentIntents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(30,10)", precision: 30, scale: 10, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    GrossAmount = table.Column<decimal>(type: "decimal(30,10)", precision: 30, scale: 10, nullable: true),
                    FeeAmount = table.Column<decimal>(type: "decimal(30,10)", precision: 30, scale: 10, nullable: false),
                    NetAmount = table.Column<decimal>(type: "decimal(30,10)", precision: 30, scale: 10, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Purpose = table.Column<int>(type: "int", nullable: false),
                    PayerUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PayerWalletId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RecipientWalletId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    WalletBehavior = table.Column<int>(type: "int", nullable: false),
                    PaymentLinkId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ExternalReferenceType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ExternalReferenceId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CallbackUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReturnUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IdempotencyKey = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedByClientId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SucceededAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FailedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiredAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    WalletPostedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    WalletTransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    WalletPostingStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentIntents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaymentLimitUsages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OwnerType = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OperationType = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    WindowType = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    WindowStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    WindowEnd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AmountTotal = table.Column<decimal>(type: "decimal(30,10)", precision: 30, scale: 10, nullable: false),
                    CountTotal = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentLimitUsages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaymentLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    OwnerType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RecipientWalletId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TokenHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    PublicCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AmountType = table.Column<int>(type: "int", nullable: false),
                    FixedAmount = table.Column<decimal>(type: "decimal(30,10)", precision: 30, scale: 10, nullable: true),
                    SuggestedAmount = table.Column<decimal>(type: "decimal(30,10)", precision: 30, scale: 10, nullable: true),
                    MinAmount = table.Column<decimal>(type: "decimal(30,10)", precision: 30, scale: 10, nullable: true),
                    MaxAmount = table.Column<decimal>(type: "decimal(30,10)", precision: 30, scale: 10, nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    AllowAnonymousPayer = table.Column<bool>(type: "bit", nullable: false),
                    RequirePayerName = table.Column<bool>(type: "bit", nullable: false),
                    RequirePayerMobile = table.Column<bool>(type: "bit", nullable: false),
                    RequireDescription = table.Column<bool>(type: "bit", nullable: false),
                    SuccessMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FailureMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReturnUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsageLimit = table.Column<int>(type: "int", nullable: true),
                    SuccessfulPaymentCount = table.Column<int>(type: "int", nullable: false),
                    TotalPaidAmount = table.Column<decimal>(type: "decimal(30,10)", precision: 30, scale: 10, nullable: false),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedByClientId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PausedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DisabledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentLinks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrackingNumber = table.Column<long>(type: "bigint", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Gateway = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TransactionReference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CallbackUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReturnUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    IdempotencyKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreditApplied = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AmountDue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaymentSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Environment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SupportedCurrencies = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DefaultCurrency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MoneyMaxPrecision = table.Column<int>(type: "int", nullable: false),
                    MoneyMaxScale = table.Column<int>(type: "int", nullable: false),
                    DefaultWalletEnabled = table.Column<bool>(type: "bit", nullable: false),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PayoutAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OwnerType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BankName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    BankCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CardNumberMasked = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CardNumberHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Iban = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IbanHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    AccountNumberMasked = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AccountNumberHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    HolderName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    HolderNationalIdHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    VerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VerifiedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RejectedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DisabledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedByClientId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayoutAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PayoutRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    WithdrawalRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PayoutAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WalletId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(30,10)", precision: 30, scale: 10, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    FeeAmount = table.Column<decimal>(type: "decimal(30,10)", precision: 30, scale: 10, nullable: false),
                    NetAmount = table.Column<decimal>(type: "decimal(30,10)", precision: 30, scale: 10, nullable: false),
                    PayoutMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    BankTrackingNumber = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    BankReferenceId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PaidAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FailedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FailureReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayoutRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReconciliationBatches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReconciliationType = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TotalChecked = table.Column<int>(type: "int", nullable: false),
                    MatchedCount = table.Column<int>(type: "int", nullable: false),
                    MismatchCount = table.Column<int>(type: "int", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TriggeredByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Summary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReconciliationBatches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReconciliationItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PrimaryEntityType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PrimaryEntityId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RelatedEntityType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RelatedEntityId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExpectedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ActualAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IssueCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResolvedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ResolutionNote = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReconciliationItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RefundRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PaymentIntentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PaymentTransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WalletId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RequestedByUserId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RequestedByAdminId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RequestSource = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(30,10)", precision: 30, scale: 10, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    AdminNote = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RejectReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    GatewayConfigId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProviderCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    GatewayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    GatewayRefundId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    GatewayTrackingCode = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    GatewayReferenceId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    WalletHoldId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApprovedByUserId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProcessedByUserId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FailedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FailureCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FailureMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SafeGatewayResponseJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdempotencyKey = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CorrelationId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RequestId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefundRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RiskCases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RiskEvaluationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CaseType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OwnerType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AssignedToUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OpenedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResolutionNote = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResolvedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiskCases", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RiskEvaluations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RiskCaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OperationType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    OwnerType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RiskScore = table.Column<int>(type: "int", nullable: false),
                    RiskLevel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Decision = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EvaluatedRules = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Reasons = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiskEvaluations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RiskRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OperationType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RuleType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ThresholdAmount = table.Column<decimal>(type: "decimal(30,10)", precision: 30, scale: 10, nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiskRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TenantPaymentSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WalletEnabled = table.Column<bool>(type: "bit", nullable: false),
                    GatewayEnabled = table.Column<bool>(type: "bit", nullable: false),
                    WithdrawalsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    ManualAdminAdjustmentsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    DefaultCurrency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SupportedCurrencies = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MinWalletAdjustmentAmount = table.Column<decimal>(type: "decimal(30,10)", precision: 30, scale: 10, nullable: true),
                    MaxWalletAdjustmentAmount = table.Column<decimal>(type: "decimal(30,10)", precision: 30, scale: 10, nullable: true),
                    DailyWalletCreditLimit = table.Column<decimal>(type: "decimal(30,10)", precision: 30, scale: 10, nullable: true),
                    DailyWalletDebitLimit = table.Column<decimal>(type: "decimal(30,10)", precision: 30, scale: 10, nullable: true),
                    KycRequiredForWithdrawal = table.Column<bool>(type: "bit", nullable: false),
                    AmlChecksEnabled = table.Column<bool>(type: "bit", nullable: false),
                    MinWithdrawalAmount = table.Column<decimal>(type: "decimal(30,10)", precision: 30, scale: 10, nullable: true),
                    MaxWithdrawalAmount = table.Column<decimal>(type: "decimal(30,10)", precision: 30, scale: 10, nullable: true),
                    WithdrawalRequiresVerifiedPayoutAccount = table.Column<bool>(type: "bit", nullable: false),
                    WithdrawalRequiresAdminApproval = table.Column<bool>(type: "bit", nullable: false),
                    WithdrawalAutoExpireHours = table.Column<int>(type: "int", nullable: true),
                    WithdrawalDailyCountLimit = table.Column<int>(type: "int", nullable: true),
                    WithdrawalMonthlyCountLimit = table.Column<int>(type: "int", nullable: true),
                    WithdrawalDailyAmountLimit = table.Column<decimal>(type: "decimal(30,10)", precision: 30, scale: 10, nullable: true),
                    WithdrawalMonthlyAmountLimit = table.Column<decimal>(type: "decimal(30,10)", precision: 30, scale: 10, nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantPaymentSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WalletAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OwnerType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    AvailableBalance = table.Column<decimal>(type: "decimal(30,10)", precision: 30, scale: 10, nullable: false),
                    LockedBalance = table.Column<decimal>(type: "decimal(30,10)", precision: 30, scale: 10, nullable: false),
                    PendingBalance = table.Column<decimal>(type: "decimal(30,10)", precision: 30, scale: 10, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WalletAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WalletConsistencyChecks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WalletId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Scope = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CheckedWalletCount = table.Column<int>(type: "int", nullable: false),
                    IssueCount = table.Column<int>(type: "int", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TriggeredByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Summary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WalletConsistencyChecks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WalletConsistencyIssues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CheckId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WalletId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Severity = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IssueCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EntityId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResolvedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ResolutionNote = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WalletConsistencyIssues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WalletHolds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    WalletAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(30,10)", precision: 30, scale: 10, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ReferenceType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ReferenceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    MetadataJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CapturedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReleasedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WalletHolds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Wallets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wallets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WalletTransactionRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TransactionType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(30,10)", precision: 30, scale: 10, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    SourceWalletId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DestinationWalletId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReferenceType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ReferenceId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IdempotencyKey = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedByClientId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PostedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReversedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FailedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WalletTransactionRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WebhookDeliveries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OutboxEventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WebhookSubscriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EventVersion = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AttemptCount = table.Column<int>(type: "int", nullable: false),
                    MaxAttempts = table.Column<int>(type: "int", nullable: false),
                    NextAttemptAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastAttemptAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastStatusCode = table.Column<int>(type: "int", nullable: true),
                    LastErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResponseBodySafe = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeliveredAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FailedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebhookDeliveries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WebhookSubscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Url = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    SecretHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    EncryptedSecret = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EventTypes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MaxRetryCount = table.Column<int>(type: "int", nullable: false),
                    TimeoutSeconds = table.Column<int>(type: "int", nullable: false),
                    Headers = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DisabledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebhookSubscriptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GatewayRoutingRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PolicyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GatewayConfigId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    Weight = table.Column<int>(type: "int", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    MinAmount = table.Column<decimal>(type: "decimal(30,10)", precision: 30, scale: 10, nullable: true),
                    MaxAmount = table.Column<decimal>(type: "decimal(30,10)", precision: 30, scale: 10, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GatewayRoutingRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GatewayRoutingRules_GatewayRoutingPolicies_PolicyId",
                        column: x => x.PolicyId,
                        principalTable: "GatewayRoutingPolicies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ParbadTransactions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Type = table.Column<byte>(type: "tinyint", nullable: false),
                    IsSucceed = table.Column<bool>(type: "bit", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdditionalData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaymentId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParbadTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParbadTransactions_ParbadPayments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "ParbadPayments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PaymentIntentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GatewayConfigId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProviderCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AdapterType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(30,10)", precision: 30, scale: 10, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Authority = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    GatewayReferenceId = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TraceNumber = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Rrn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CardPanMasked = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    GatewayResponseCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    GatewayResponseMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CallbackReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FailedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiredAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CallbackPayloadSafe = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VerifyPayloadSafe = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdempotencyKey = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentTransactions_GatewayConfigs_GatewayConfigId",
                        column: x => x.GatewayConfigId,
                        principalTable: "GatewayConfigs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaymentTransactions_PaymentIntents_PaymentIntentId",
                        column: x => x.PaymentIntentId,
                        principalTable: "PaymentIntents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentAttempts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AttemptNumber = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ProviderResponse = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ErrorCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentAttempts_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Payments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Actor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentLogs_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Payments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WithdrawalRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    WalletId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PayoutAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OwnerType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(30,10)", precision: 30, scale: 10, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    FeeAmount = table.Column<decimal>(type: "decimal(30,10)", precision: 30, scale: 10, nullable: false),
                    NetAmount = table.Column<decimal>(type: "decimal(30,10)", precision: 30, scale: 10, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserNote = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdempotencyKey = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LockedWalletTransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReleaseWalletTransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CaptureWalletTransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApprovedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovalNote = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RejectedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RejectedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CancelledByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MoreInfoRequestedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MoreInfoRequestedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MoreInfoReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaidByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PaidAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PaidNote = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BankTrackingNumber = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    BankReferenceId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedByClientId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WithdrawalRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WithdrawalRequests_PayoutAccounts_PayoutAccountId",
                        column: x => x.PayoutAccountId,
                        principalTable: "PayoutAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RefundGatewayAttempts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RefundRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GatewayConfigId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProviderCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    GatewayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AttemptNo = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RequestPayloadSafe = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResponsePayloadSafe = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GatewayRefundId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    GatewayTrackingCode = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    GatewayStatus = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    HttpStatusCode = table.Column<int>(type: "int", nullable: true),
                    ErrorCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FinishedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefundGatewayAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefundGatewayAttempts_RefundRequests_RefundRequestId",
                        column: x => x.RefundRequestId,
                        principalTable: "RefundRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WalletLedgerEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WalletAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WalletTransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EntryType = table.Column<int>(type: "int", nullable: false),
                    Direction = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(30,10)", precision: 30, scale: 10, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    BalanceAvailableBefore = table.Column<decimal>(type: "decimal(30,10)", precision: 30, scale: 10, nullable: false),
                    BalanceAvailableAfter = table.Column<decimal>(type: "decimal(30,10)", precision: 30, scale: 10, nullable: false),
                    BalanceLockedBefore = table.Column<decimal>(type: "decimal(30,10)", precision: 30, scale: 10, nullable: false),
                    BalanceLockedAfter = table.Column<decimal>(type: "decimal(30,10)", precision: 30, scale: 10, nullable: false),
                    BalancePendingBefore = table.Column<decimal>(type: "decimal(30,10)", precision: 30, scale: 10, nullable: false),
                    BalancePendingAfter = table.Column<decimal>(type: "decimal(30,10)", precision: 30, scale: 10, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReferenceType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReferenceId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedByClientId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PreviousEntryHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntryHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HashAlgorithm = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HashVersion = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WalletLedgerEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WalletLedgerEntries_WalletAccounts_WalletAccountId",
                        column: x => x.WalletAccountId,
                        principalTable: "WalletAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WalletTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WalletId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ReferenceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ReferenceType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BalanceAfter = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IsReversed = table.Column<bool>(type: "bit", nullable: false),
                    ReversalTransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReversalReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ReversedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WalletTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WalletTransactions_Wallets_WalletId",
                        column: x => x.WalletId,
                        principalTable: "Wallets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdminApprovalRequests_TenantId_Status_RequestedAt",
                table: "AdminApprovalRequests",
                columns: new[] { "TenantId", "Status", "RequestedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EntityType_EntityId_CreatedAt",
                table: "AuditLogs",
                columns: new[] { "EntityType", "EntityId", "CreatedAt" },
                descending: new[] { false, false, true });

            migrationBuilder.CreateIndex(
                name: "IX_BankStatementEntries_TenantId_TransactionDate",
                table: "BankStatementEntries",
                columns: new[] { "TenantId", "TransactionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_EventRoutingRules_TenantId_ApplicationCode_Target_Enabled",
                table: "EventRoutingRules",
                columns: new[] { "TenantId", "ApplicationCode", "Target", "Enabled" });

            migrationBuilder.CreateIndex(
                name: "IX_FeeRules_TenantId_Status",
                table: "FeeRules",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_GatewayConfigs_ProviderCode_Status",
                table: "GatewayConfigs",
                columns: new[] { "ProviderCode", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_GatewayConfigs_TenantId_ApplicationCode_Priority",
                table: "GatewayConfigs",
                columns: new[] { "TenantId", "ApplicationCode", "Priority" });

            migrationBuilder.CreateIndex(
                name: "IX_GatewayConfigs_TenantId_ApplicationCode_Status",
                table: "GatewayConfigs",
                columns: new[] { "TenantId", "ApplicationCode", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_GatewayProviders_Code",
                table: "GatewayProviders",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GatewayRoutingPolicies_TenantId_ApplicationCode_Status",
                table: "GatewayRoutingPolicies",
                columns: new[] { "TenantId", "ApplicationCode", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_GatewayRoutingRules_PolicyId_Status_Priority",
                table: "GatewayRoutingRules",
                columns: new[] { "PolicyId", "Status", "Priority" });

            migrationBuilder.CreateIndex(
                name: "IX_IdempotencyRecords_TenantId_IdempotencyKey",
                table: "IdempotencyRecords",
                columns: new[] { "TenantId", "IdempotencyKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LimitRules_TenantId_Status",
                table: "LimitRules",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_OutboxEvents_Status_AvailableAt_NextRetryAt_OccurredAt",
                table: "OutboxEvents",
                columns: new[] { "Status", "AvailableAt", "NextRetryAt", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_OutboxEvents_Status_OccurredAt",
                table: "OutboxEvents",
                columns: new[] { "Status", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_OutboxEvents_TenantId_ApplicationCode_OccurredAt",
                table: "OutboxEvents",
                columns: new[] { "TenantId", "ApplicationCode", "OccurredAt" },
                descending: new[] { false, false, true });

            migrationBuilder.CreateIndex(
                name: "IX_OutboxEvents_TenantId_EventType_OccurredAt",
                table: "OutboxEvents",
                columns: new[] { "TenantId", "EventType", "OccurredAt" },
                descending: new[] { false, false, true });

            migrationBuilder.CreateIndex(
                name: "IX_ParbadTransactions_PaymentId",
                table: "ParbadTransactions",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentAttempts_PaymentId_AttemptNumber",
                table: "PaymentAttempts",
                columns: new[] { "PaymentId", "AttemptNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentIntents_TenantId_CreatedAt",
                table: "PaymentIntents",
                columns: new[] { "TenantId", "CreatedAt" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentIntents_TenantId_ExternalReferenceType_ExternalReferenceId",
                table: "PaymentIntents",
                columns: new[] { "TenantId", "ExternalReferenceType", "ExternalReferenceId" });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentIntents_TenantId_IdempotencyKey",
                table: "PaymentIntents",
                columns: new[] { "TenantId", "IdempotencyKey" },
                unique: true,
                filter: "\"IdempotencyKey\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentIntents_TenantId_Status",
                table: "PaymentIntents",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentLimitUsages_TenantId_OwnerType_OwnerId_OperationType_Currency_WindowType_WindowStart",
                table: "PaymentLimitUsages",
                columns: new[] { "TenantId", "OwnerType", "OwnerId", "OperationType", "Currency", "WindowType", "WindowStart" },
                unique: true,
                filter: "[OwnerType] IS NOT NULL AND [OwnerId] IS NOT NULL AND [Currency] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentLinks_RecipientWalletId",
                table: "PaymentLinks",
                column: "RecipientWalletId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentLinks_TenantId_CreatedAt",
                table: "PaymentLinks",
                columns: new[] { "TenantId", "CreatedAt" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentLinks_TenantId_OwnerType_OwnerId",
                table: "PaymentLinks",
                columns: new[] { "TenantId", "OwnerType", "OwnerId" });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentLinks_TenantId_Status",
                table: "PaymentLinks",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentLinks_TokenHash",
                table: "PaymentLinks",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentLogs_PaymentId",
                table: "PaymentLogs",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentLogs_Timestamp",
                table: "PaymentLogs",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_CreatedAt",
                table: "Payments",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_IdempotencyKey",
                table: "Payments",
                column: "IdempotencyKey",
                unique: true,
                filter: "\"IdempotencyKey\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_Status",
                table: "Payments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_TrackingNumber",
                table: "Payments",
                column: "TrackingNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_UserId",
                table: "Payments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_Authority",
                table: "PaymentTransactions",
                column: "Authority");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_GatewayConfigId",
                table: "PaymentTransactions",
                column: "GatewayConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_PaymentIntentId_CreatedAt",
                table: "PaymentTransactions",
                columns: new[] { "PaymentIntentId", "CreatedAt" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_TenantId_CreatedAt",
                table: "PaymentTransactions",
                columns: new[] { "TenantId", "CreatedAt" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_TenantId_IdempotencyKey",
                table: "PaymentTransactions",
                columns: new[] { "TenantId", "IdempotencyKey" },
                unique: true,
                filter: "\"IdempotencyKey\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_TenantId_Status",
                table: "PaymentTransactions",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_PayoutAccounts_TenantId_CardNumberHash",
                table: "PayoutAccounts",
                columns: new[] { "TenantId", "CardNumberHash" });

            migrationBuilder.CreateIndex(
                name: "IX_PayoutAccounts_TenantId_IbanHash",
                table: "PayoutAccounts",
                columns: new[] { "TenantId", "IbanHash" });

            migrationBuilder.CreateIndex(
                name: "IX_PayoutAccounts_TenantId_OwnerType_OwnerId",
                table: "PayoutAccounts",
                columns: new[] { "TenantId", "OwnerType", "OwnerId" });

            migrationBuilder.CreateIndex(
                name: "IX_PayoutAccounts_TenantId_Status",
                table: "PayoutAccounts",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_PayoutRecords_TenantId_Status_CreatedAt",
                table: "PayoutRecords",
                columns: new[] { "TenantId", "Status", "CreatedAt" },
                descending: new[] { false, false, true });

            migrationBuilder.CreateIndex(
                name: "IX_PayoutRecords_WithdrawalRequestId",
                table: "PayoutRecords",
                column: "WithdrawalRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_ReconciliationBatches_TenantId_ReconciliationType_CreatedAt",
                table: "ReconciliationBatches",
                columns: new[] { "TenantId", "ReconciliationType", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ReconciliationItems_BatchId_Status",
                table: "ReconciliationItems",
                columns: new[] { "BatchId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_RefundGatewayAttempts_RefundRequestId",
                table: "RefundGatewayAttempts",
                column: "RefundRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_RefundGatewayAttempts_RefundRequestId_AttemptNo",
                table: "RefundGatewayAttempts",
                columns: new[] { "RefundRequestId", "AttemptNo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefundRequests_PaymentTransactionId",
                table: "RefundRequests",
                column: "PaymentTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_RefundRequests_TenantId_CreatedAt",
                table: "RefundRequests",
                columns: new[] { "TenantId", "CreatedAt" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_RefundRequests_TenantId_IdempotencyKey",
                table: "RefundRequests",
                columns: new[] { "TenantId", "IdempotencyKey" },
                unique: true,
                filter: "\"IdempotencyKey\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_RefundRequests_TenantId_Status",
                table: "RefundRequests",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_RefundRequests_WalletHoldId",
                table: "RefundRequests",
                column: "WalletHoldId");

            migrationBuilder.CreateIndex(
                name: "IX_RiskCases_TenantId_Status_OpenedAt",
                table: "RiskCases",
                columns: new[] { "TenantId", "Status", "OpenedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_RiskEvaluations_TenantId_EntityType_EntityId",
                table: "RiskEvaluations",
                columns: new[] { "TenantId", "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_RiskRules_TenantId_Status",
                table: "RiskRules",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_TenantPaymentSettings_TenantId",
                table: "TenantPaymentSettings",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WalletAccounts_Status",
                table: "WalletAccounts",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_WalletAccounts_TenantId_OwnerType_OwnerId",
                table: "WalletAccounts",
                columns: new[] { "TenantId", "OwnerType", "OwnerId" });

            migrationBuilder.CreateIndex(
                name: "IX_WalletConsistencyChecks_TenantId_Status_StartedAt",
                table: "WalletConsistencyChecks",
                columns: new[] { "TenantId", "Status", "StartedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_WalletConsistencyIssues_TenantId_Status_CreatedAt",
                table: "WalletConsistencyIssues",
                columns: new[] { "TenantId", "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_WalletHolds_ExpiresAt",
                table: "WalletHolds",
                column: "ExpiresAt",
                filter: "\"ExpiresAt\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_WalletHolds_ReferenceType_ReferenceId",
                table: "WalletHolds",
                columns: new[] { "ReferenceType", "ReferenceId" });

            migrationBuilder.CreateIndex(
                name: "IX_WalletHolds_TenantId_Status",
                table: "WalletHolds",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_WalletHolds_WalletAccountId",
                table: "WalletHolds",
                column: "WalletAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletLedgerEntries_TenantId_CreatedAt",
                table: "WalletLedgerEntries",
                columns: new[] { "TenantId", "CreatedAt" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_WalletLedgerEntries_WalletAccountId_CreatedAt",
                table: "WalletLedgerEntries",
                columns: new[] { "WalletAccountId", "CreatedAt" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_WalletLedgerEntries_WalletTransactionId",
                table: "WalletLedgerEntries",
                column: "WalletTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_UserId",
                table: "Wallets",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactionRecords_DestinationWalletId",
                table: "WalletTransactionRecords",
                column: "DestinationWalletId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactionRecords_SourceWalletId",
                table: "WalletTransactionRecords",
                column: "SourceWalletId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactionRecords_TenantId_CreatedAt",
                table: "WalletTransactionRecords",
                columns: new[] { "TenantId", "CreatedAt" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactionRecords_TenantId_IdempotencyKey",
                table: "WalletTransactionRecords",
                columns: new[] { "TenantId", "IdempotencyKey" },
                unique: true,
                filter: "\"IdempotencyKey\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_CreatedAt",
                table: "WalletTransactions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_ReferenceId_ReferenceType",
                table: "WalletTransactions",
                columns: new[] { "ReferenceId", "ReferenceType" });

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_WalletId",
                table: "WalletTransactions",
                column: "WalletId");

            migrationBuilder.CreateIndex(
                name: "IX_WebhookDeliveries_OutboxEventId",
                table: "WebhookDeliveries",
                column: "OutboxEventId");

            migrationBuilder.CreateIndex(
                name: "IX_WebhookDeliveries_TenantId_Status_NextAttemptAt",
                table: "WebhookDeliveries",
                columns: new[] { "TenantId", "Status", "NextAttemptAt" });

            migrationBuilder.CreateIndex(
                name: "IX_WebhookDeliveries_WebhookSubscriptionId",
                table: "WebhookDeliveries",
                column: "WebhookSubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_WebhookSubscriptions_TenantId_ApplicationCode_Status",
                table: "WebhookSubscriptions",
                columns: new[] { "TenantId", "ApplicationCode", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_WithdrawalRequests_PayoutAccountId",
                table: "WithdrawalRequests",
                column: "PayoutAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_WithdrawalRequests_TenantId_IdempotencyKey",
                table: "WithdrawalRequests",
                columns: new[] { "TenantId", "IdempotencyKey" },
                unique: true,
                filter: "\"IdempotencyKey\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_WithdrawalRequests_TenantId_OwnerType_OwnerId_CreatedAt",
                table: "WithdrawalRequests",
                columns: new[] { "TenantId", "OwnerType", "OwnerId", "CreatedAt" },
                descending: new[] { false, false, false, true });

            migrationBuilder.CreateIndex(
                name: "IX_WithdrawalRequests_TenantId_Status_CreatedAt",
                table: "WithdrawalRequests",
                columns: new[] { "TenantId", "Status", "CreatedAt" },
                descending: new[] { false, false, true });

            migrationBuilder.CreateIndex(
                name: "IX_WithdrawalRequests_WalletId_CreatedAt",
                table: "WithdrawalRequests",
                columns: new[] { "WalletId", "CreatedAt" },
                descending: new[] { false, true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdminApprovalDecisions");

            migrationBuilder.DropTable(
                name: "AdminApprovalPolicies");

            migrationBuilder.DropTable(
                name: "AdminApprovalRequests");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "BankStatementEntries");

            migrationBuilder.DropTable(
                name: "BankStatementImports");

            migrationBuilder.DropTable(
                name: "EventRoutingRules");

            migrationBuilder.DropTable(
                name: "FeeRules");

            migrationBuilder.DropTable(
                name: "GatewayProviders");

            migrationBuilder.DropTable(
                name: "GatewayRoutingRules");

            migrationBuilder.DropTable(
                name: "IdempotencyRecords");

            migrationBuilder.DropTable(
                name: "LimitRules");

            migrationBuilder.DropTable(
                name: "OutboxEvents");

            migrationBuilder.DropTable(
                name: "ParbadTransactions");

            migrationBuilder.DropTable(
                name: "PaymentAttempts");

            migrationBuilder.DropTable(
                name: "PaymentLimitUsages");

            migrationBuilder.DropTable(
                name: "PaymentLinks");

            migrationBuilder.DropTable(
                name: "PaymentLogs");

            migrationBuilder.DropTable(
                name: "PaymentSettings");

            migrationBuilder.DropTable(
                name: "PaymentTransactions");

            migrationBuilder.DropTable(
                name: "PayoutRecords");

            migrationBuilder.DropTable(
                name: "ReconciliationBatches");

            migrationBuilder.DropTable(
                name: "ReconciliationItems");

            migrationBuilder.DropTable(
                name: "RefundGatewayAttempts");

            migrationBuilder.DropTable(
                name: "RiskCases");

            migrationBuilder.DropTable(
                name: "RiskEvaluations");

            migrationBuilder.DropTable(
                name: "RiskRules");

            migrationBuilder.DropTable(
                name: "TenantPaymentSettings");

            migrationBuilder.DropTable(
                name: "WalletConsistencyChecks");

            migrationBuilder.DropTable(
                name: "WalletConsistencyIssues");

            migrationBuilder.DropTable(
                name: "WalletHolds");

            migrationBuilder.DropTable(
                name: "WalletLedgerEntries");

            migrationBuilder.DropTable(
                name: "WalletTransactionRecords");

            migrationBuilder.DropTable(
                name: "WalletTransactions");

            migrationBuilder.DropTable(
                name: "WebhookDeliveries");

            migrationBuilder.DropTable(
                name: "WebhookSubscriptions");

            migrationBuilder.DropTable(
                name: "WithdrawalRequests");

            migrationBuilder.DropTable(
                name: "GatewayRoutingPolicies");

            migrationBuilder.DropTable(
                name: "ParbadPayments");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "GatewayConfigs");

            migrationBuilder.DropTable(
                name: "PaymentIntents");

            migrationBuilder.DropTable(
                name: "RefundRequests");

            migrationBuilder.DropTable(
                name: "WalletAccounts");

            migrationBuilder.DropTable(
                name: "Wallets");

            migrationBuilder.DropTable(
                name: "PayoutAccounts");
        }
    }
}
