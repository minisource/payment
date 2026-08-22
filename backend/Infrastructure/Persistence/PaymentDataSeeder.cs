using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence;

/// <summary>
/// Seeds essential data into an empty PaymentDb database.
/// Idempotent — checks existence before inserting.
/// Controlled by appsettings: Migrations:AutoSeed (default: true).
/// </summary>
public class PaymentDataSeeder
{
    private readonly PaymentDbContext _ctx;
    private readonly ILogger<PaymentDataSeeder> _logger;

    public PaymentDataSeeder(PaymentDbContext ctx, ILogger<PaymentDataSeeder> logger)
    {
        _ctx = ctx;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Checking if seed data needed...");

        await SeedGatewayProvidersAsync(ct);
        await SeedDefaultGatewayConfigsAsync(ct);
        await SeedDefaultPaymentSettingsAsync(ct);
        await SeedDefaultRiskRulesAsync(ct);

        await _ctx.SaveChangesAsync(ct);
        _logger.LogInformation("Seed data check complete");
    }

    private async Task SeedGatewayProvidersAsync(CancellationToken ct)
    {
        if (await _ctx.GatewayProviders.AnyAsync(ct)) return;

        var providers = new[]
        {
            new GatewayProvider
            {
                Code = "zarinpal",
                DisplayName = "ZarinPal",
                SupportedCurrencies = new List<string> { "IRT", "IRR" },
                SupportedOperations = new List<string> { "payment", "verify", "refund" },
                Status = GatewayProviderStatus.Active,
            },
            new GatewayProvider
            {
                Code = "idpay",
                DisplayName = "IDPay",
                SupportedCurrencies = new List<string> { "IRT", "IRR" },
                SupportedOperations = new List<string> { "payment", "verify", "refund" },
                Status = GatewayProviderStatus.Active,
            },
            new GatewayProvider
            {
                Code = "payir",
                DisplayName = "Pay.ir",
                SupportedCurrencies = new List<string> { "IRT", "IRR" },
                SupportedOperations = new List<string> { "payment", "verify" },
                Status = GatewayProviderStatus.Active,
            },
            new GatewayProvider
            {
                Code = "zibal",
                DisplayName = "Zibal",
                SupportedCurrencies = new List<string> { "IRT", "IRR" },
                SupportedOperations = new List<string> { "payment", "verify", "refund" },
                Status = GatewayProviderStatus.Active,
            },
            new GatewayProvider
            {
                Code = "snapp",
                DisplayName = "SnappPay",
                SupportedCurrencies = new List<string> { "IRT", "IRR" },
                SupportedOperations = new List<string> { "payment", "verify", "refund" },
                Status = GatewayProviderStatus.Active,
            },
            new GatewayProvider
            {
                Code = "parbad_virtual",
                DisplayName = "Parbad Virtual",
                SupportedCurrencies = new List<string> { "IRT", "IRR" },
                SupportedOperations = new List<string> { "payment", "verify" },
                Status = GatewayProviderStatus.Active,
            },
        };

        await _ctx.GatewayProviders.AddRangeAsync(providers, ct);
        _logger.LogInformation("Seeded {Count} gateway providers", providers.Length);
    }

    private async Task SeedDefaultGatewayConfigsAsync(CancellationToken ct)
    {
        // Seed a default sandbox config for Parbad Virtual (idempotent)
        if (!await _ctx.GatewayConfigs.AnyAsync(c => c.ProviderCode == "parbad_virtual", ct))
        {
            await _ctx.GatewayConfigs.AddAsync(new GatewayConfig
            {
                Name = "Parbad Virtual (Sandbox)",
                ProviderCode = "parbad_virtual",
                TenantId = null,
                ApplicationCode = null,
                Environment = "sandbox",
                Status = GatewayConfigStatus.Active,
                SupportedCurrencies = new List<string> { "IRT" },
                Priority = 100,
                Weight = 1,
                IsDefault = true,
            }, ct);
            _logger.LogInformation("Seeded default Parbad Virtual gateway config");
        }
    }

    private async Task SeedDefaultPaymentSettingsAsync(CancellationToken ct)
    {
        if (await _ctx.PaymentSettings.AnyAsync(ct)) return;

        var settings = new PaymentSettings
        {
            Environment = "development",
            SupportedCurrencies = new List<string> { "IRT", "IRR" },
            DefaultCurrency = "IRT",
            MoneyMaxPrecision = 30,
            MoneyMaxScale = 10,
            DefaultWalletEnabled = true,
        };

        await _ctx.PaymentSettings.AddAsync(settings, ct);
        _logger.LogInformation("Seeded default payment settings");
    }

    private async Task SeedDefaultRiskRulesAsync(CancellationToken ct)
    {
        if (await _ctx.RiskRules.AnyAsync(ct)) return;

        var rules = new[]
        {
            new RiskRule
            {
                TenantId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                Name = "High Amount Cross-Check",
                OperationType = "payment",
                RuleType = "amount_threshold",
                Action = "flag",
                ThresholdAmount = 50000000,
                Currency = "IRR",
                Priority = 10,
                Status = "active",
            },
            new RiskRule
            {
                TenantId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                Name = "Rapid Transaction Check",
                OperationType = "payment",
                RuleType = "frequency",
                Action = "review",
                Priority = 20,
                Status = "active",
            },
        };

        await _ctx.RiskRules.AddRangeAsync(rules, ct);
        _logger.LogInformation("Seeded {Count} risk rules", rules.Length);
    }
}
