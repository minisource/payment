namespace payment.Options;

/// <summary>
/// Configuration for standalone (no-auth) mode fallback values.
/// Only used when Auth.Enabled = false.
/// </summary>
public class PaymentStandaloneOptions
{
    public const string SectionName = "Payment:Standalone";

    /// <summary>Whether standalone mode is enabled.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>Default tenant ID to use when no tenant header is provided.</summary>
    public string DefaultTenantId { get; set; } = "00000000-0000-0000-0000-000000000001";

    /// <summary>Default application code to use when no application header is provided.</summary>
    public string DefaultApplicationCode { get; set; } = "payment";
}

/// <summary>
/// Security configuration for the Payment service.
/// </summary>
public class PaymentSecurityOptions
{
    public const string SectionName = "Payment:Security";

    /// <summary>Whether to allow auth-disabled mode in production.</summary>
    public bool AllowAuthDisabledInProduction { get; set; } = false;
}
