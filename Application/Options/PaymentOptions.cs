namespace Application.Options;

public class PaymentOptions
{
    public string DefaultGateway { get; set; } = string.Empty;
    public string RedirectFromGatewayToUrl { get; set; } = string.Empty;
    public string ShopCallbackUrl { get; set; } = string.Empty;
    public Dictionary<string, GatewayConfig> Gateways { get; set; } = new();
    public PollyConfig Polly { get; set; } = new();
}

public class GatewayConfig
{
    public string MerchantId { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string TerminalId { get; set; } = string.Empty;
    public bool IsSandbox { get; set; }
}

public class PollyConfig
{
    public int RetryCount { get; set; } = 3;
    public int TimeoutSeconds { get; set; } = 30;
}