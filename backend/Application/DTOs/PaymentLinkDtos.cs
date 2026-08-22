namespace Application.DTOs;

// ═══ Payment Link CRUD DTOs ═════════════════════════════

public class CreatePaymentLinkRequest
{
    public Guid RecipientWalletId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string AmountType { get; set; } = "open";
    public decimal? FixedAmount { get; set; }
    public decimal? SuggestedAmount { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public string Currency { get; set; } = "IRT";
    public bool AllowAnonymousPayer { get; set; } = true;
    public bool RequirePayerName { get; set; }
    public bool RequirePayerMobile { get; set; }
    public bool RequireDescription { get; set; }
    public string? SuccessMessage { get; set; }
    public string? FailureMessage { get; set; }
    public string? ReturnUrl { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public int? UsageLimit { get; set; }
    public string? PublicCode { get; set; }
    public string? Metadata { get; set; }
}

public class UpdatePaymentLinkRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? AmountType { get; set; }
    public decimal? FixedAmount { get; set; }
    public decimal? SuggestedAmount { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public bool? AllowAnonymousPayer { get; set; }
    public bool? RequirePayerName { get; set; }
    public bool? RequirePayerMobile { get; set; }
    public bool? RequireDescription { get; set; }
    public string? SuccessMessage { get; set; }
    public string? FailureMessage { get; set; }
    public string? ReturnUrl { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public int? UsageLimit { get; set; }
    public string? Metadata { get; set; }
}

public class PaymentLinkDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string OwnerType { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
    public Guid RecipientWalletId { get; set; }
    public string? PublicCode { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string AmountType { get; set; } = string.Empty;
    public decimal? FixedAmount { get; set; }
    public decimal? SuggestedAmount { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool AllowAnonymousPayer { get; set; }
    public bool RequirePayerName { get; set; }
    public bool RequirePayerMobile { get; set; }
    public bool RequireDescription { get; set; }
    public string? SuccessMessage { get; set; }
    public string? FailureMessage { get; set; }
    public string? ReturnUrl { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public int? UsageLimit { get; set; }
    public int SuccessfulPaymentCount { get; set; }
    public decimal TotalPaidAmount { get; set; }
    public string? PublicUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreatePaymentLinkResponse
{
    public Guid Id { get; set; }
    public string PublicUrl { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

// ═══ Public Payment Link DTOs ═══════════════════════════

public class PublicPaymentLinkResponse
{
    public string PaymentLinkId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string AmountType { get; set; } = string.Empty;
    public decimal? FixedAmount { get; set; }
    public decimal? SuggestedAmount { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool RequirePayerName { get; set; }
    public bool RequirePayerMobile { get; set; }
    public bool RequireDescription { get; set; }
    public bool GatewayEnabled { get; set; } = true;
    public DateTime? ExpiresAt { get; set; }
    public string? SuccessMessage { get; set; }
}

public class PublicPayRequest
{
    public decimal? Amount { get; set; }
    public string? PayerName { get; set; }
    public string? PayerMobile { get; set; }
    public string? PayerDescription { get; set; }
    public string? PreferredProviderCode { get; set; }
    public string? Metadata { get; set; }
}

public class PublicPayResponse
{
    public string PaymentIntentId { get; set; } = string.Empty;
    public string PaymentTransactionId { get; set; } = string.Empty;
    public string Status { get; set; } = "redirect_required";
    public string? RedirectUrl { get; set; }
}
