namespace Application.DTOs;

// ═══════════════════════════════════════════════════
// Payment Intent DTOs
// ═══════════════════════════════════════════════════

public class CreatePaymentIntentRequest
{
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "IRT";
    public string Purpose { get; set; } = "wallet_topup";
    public Guid? PayerUserId { get; set; }
    public Guid? PayerWalletId { get; set; }
    public Guid? RecipientWalletId { get; set; }
    public string WalletBehavior { get; set; } = "none";
    public string? ExternalReferenceType { get; set; }
    public string? ExternalReferenceId { get; set; }
    public string? Description { get; set; }
    public string? ReturnUrl { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string? Metadata { get; set; }
}

public class StartPaymentIntentRequest
{
    public string? PreferredProviderCode { get; set; }
    public Guid? GatewayConfigId { get; set; }
    public string? Metadata { get; set; }
}

public class PaymentIntentDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string? ApplicationCode { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "IRT";
    public decimal? GrossAmount { get; set; }
    public decimal FeeAmount { get; set; }
    public decimal? NetAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
    public string WalletBehavior { get; set; } = string.Empty;
    public Guid? PayerUserId { get; set; }
    public Guid? PayerWalletId { get; set; }
    public Guid? RecipientWalletId { get; set; }
    public string? ExternalReferenceType { get; set; }
    public string? ExternalReferenceId { get; set; }
    public string? Description { get; set; }
    public string? ReturnUrl { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string? IdempotencyKey { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? SucceededAt { get; set; }
    public DateTime? FailedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string WalletPostingStatus { get; set; } = "not_required";
    public DateTime? WalletPostedAt { get; set; }
    public Guid? WalletTransactionId { get; set; }
}

public class PaymentTransactionDto
{
    public Guid Id { get; set; }
    public Guid PaymentIntentId { get; set; }
    public Guid GatewayConfigId { get; set; }
    public Guid TenantId { get; set; }
    public string ProviderCode { get; set; } = string.Empty;
    public string AdapterType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Authority { get; set; }
    public string? GatewayReferenceId { get; set; }
    public string? TraceNumber { get; set; }
    public string? Rrn { get; set; }
    public string? CardPanMasked { get; set; }
    public string? GatewayResponseCode { get; set; }
    public string? GatewayResponseMessage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public DateTime? FailedAt { get; set; }
}

public class StartPaymentIntentResponse
{
    public Guid PaymentIntentId { get; set; }
    public Guid PaymentTransactionId { get; set; }
    public string Status { get; set; } = "redirect_required";
    public string ProviderCode { get; set; } = string.Empty;
    public Guid GatewayConfigId { get; set; }
    public string? RedirectUrl { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

// ═══════════════════════════════════════════════════
// Gateway Provider DTOs
// ═══════════════════════════════════════════════════

public class GatewayProviderDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string AdapterType { get; set; } = "parbad";
    public string Status { get; set; } = string.Empty;
    public List<string> SupportedCurrencies { get; set; } = [];
    public string RequiredConfigSchema { get; set; } = "{}";
    public string? OptionalConfigSchema { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateGatewayProviderRequest
{
    public string Code { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string AdapterType { get; set; } = "parbad";
    public List<string> SupportedCurrencies { get; set; } = ["IRT"];
    public string RequiredConfigSchema { get; set; } = "{}";
    public string? OptionalConfigSchema { get; set; }
    public string? Metadata { get; set; }
}

public class UpdateGatewayProviderRequest
{
    public string? DisplayName { get; set; }
    public string? Status { get; set; }
    public List<string>? SupportedCurrencies { get; set; }
    public string? RequiredConfigSchema { get; set; }
    public string? OptionalConfigSchema { get; set; }
}

// ═══════════════════════════════════════════════════
// Gateway Config DTOs
// ═══════════════════════════════════════════════════

public class GatewayConfigDto
{
    public Guid Id { get; set; }
    public Guid? TenantId { get; set; }
    public string? ApplicationCode { get; set; }
    public string ProviderCode { get; set; } = string.Empty;
    public string AdapterType { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public int Priority { get; set; }
    public int Weight { get; set; }
    public List<string> SupportedCurrencies { get; set; } = [];
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public string ConfigJson { get; set; } = "{}";
    public string? CallbackBaseUrl { get; set; }
    public string HealthStatus { get; set; } = string.Empty;
    public int FailureCount { get; set; }
    public DateTime? LastSuccessAt { get; set; }
    public DateTime? LastFailureAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateGatewayConfigRequest
{
    public Guid? TenantId { get; set; }
    public string? ApplicationCode { get; set; }
    public string ProviderCode { get; set; } = string.Empty;
    public string AdapterType { get; set; } = "parbad";
    public string Name { get; set; } = string.Empty;
    public string Environment { get; set; } = "production";
    public bool IsDefault { get; set; }
    public int Priority { get; set; } = 100;
    public int Weight { get; set; } = 1;
    public List<string> SupportedCurrencies { get; set; } = ["IRT"];
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public string Config { get; set; } = "{}";   // Raw config including secret values
    public string? CallbackBaseUrl { get; set; }
    public string? Metadata { get; set; }
}

public class UpdateGatewayConfigRequest
{
    public string? Name { get; set; }
    public string? Status { get; set; }
    public bool? IsDefault { get; set; }
    public int? Priority { get; set; }
    public int? Weight { get; set; }
    public List<string>? SupportedCurrencies { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public string? Config { get; set; }
    public string? CallbackBaseUrl { get; set; }
    public string? Metadata { get; set; }
}

// ═══════════════════════════════════════════════════
// Routing DTOs
// ═══════════════════════════════════════════════════

public class GatewayRoutingPolicyDto
{
    public Guid Id { get; set; }
    public Guid? TenantId { get; set; }
    public string? ApplicationCode { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Strategy { get; set; } = string.Empty;
    public bool FallbackEnabled { get; set; }
    public bool RandomizeSamePriority { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<GatewayRoutingRuleDto> Rules { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class GatewayRoutingRuleDto
{
    public Guid Id { get; set; }
    public Guid PolicyId { get; set; }
    public Guid GatewayConfigId { get; set; }
    public int Priority { get; set; }
    public int Weight { get; set; }
    public string? Currency { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class CreateRoutingPolicyRequest
{
    public Guid? TenantId { get; set; }
    public string? ApplicationCode { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Strategy { get; set; } = "priority";
    public bool FallbackEnabled { get; set; } = true;
    public bool RandomizeSamePriority { get; set; }
    public List<CreateRoutingRuleRequest>? Rules { get; set; }
    public string? Metadata { get; set; }
}

public class CreateRoutingRuleRequest
{
    public Guid GatewayConfigId { get; set; }
    public int Priority { get; set; } = 100;
    public int Weight { get; set; } = 1;
    public string? Currency { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
}

public class UpdateRoutingRuleRequest
{
    public int? Priority { get; set; }
    public int? Weight { get; set; }
    public string? Currency { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public string? Status { get; set; }
}

// ═══════════════════════════════════════════════════
// Result Page DTOs
// ═══════════════════════════════════════════════════

public class PaymentResultDto
{
    public Guid PaymentIntentId { get; set; }
    public Guid? PaymentTransactionId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string ProviderDisplayName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string? GatewayReferenceId { get; set; }
    public string? TraceNumber { get; set; }
    public string? ReturnUrl { get; set; }
    public DateTime? CompletedAt { get; set; }
}

// ═══════════════════════════════════════════════════
// Gateway selection result
// ═══════════════════════════════════════════════════

public sealed record GatewaySelectionRequest(
    Guid TenantId,
    string? ApplicationCode,
    decimal Amount,
    string Currency,
    string? PreferredProviderCode = null,
    Guid? GatewayConfigId = null);

public sealed record GatewaySelectionResult(
    Guid GatewayConfigId,
    string ProviderCode,
    string AdapterType,
    string Strategy,
    IReadOnlyList<Guid> FallbackGatewayConfigIds);

// ═══════════════════════════════════════════════════
// Gateway Config Test Result
// ═══════════════════════════════════════════════════

public record GatewayConfigTestResultDto(
    bool Success,
    string Status,
    string ProviderCode,
    string Message,
    DateTime CheckedAt,
    List<string>? Details);
