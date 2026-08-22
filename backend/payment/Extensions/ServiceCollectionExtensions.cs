using Application.Options;
using Infrastructure.Gateways;
using Infrastructure.Locking;
using Infrastructure.Payment;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.OpenApi.Models;
using Minisource.Common.Auth;
using Minisource.Common.Tenancy;
using Minisource.Sdk.Auth;
using Minisource.Sdk.Payment;
using payment.Abstractions;
using payment.Extensions;
using payment.Options;
using Presentaion;
using Presentaion.Middlewares;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Registers all API/presentation layer services with DI container.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPaymentApi(
        this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        // Options
        services.Configure<AuthOptions>(configuration.GetSection("Auth"));
        services.Configure<PaymentStandaloneOptions>(configuration.GetSection("Payment:Standalone"));
        services.Configure<PaymentSecurityOptions>(configuration.GetSection("Payment:Security"));

        // Forwarded headers for Traefik/reverse proxy
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
        });

        // Controllers & Swagger
        services.AddControllers();
        services.AddEndpointsApiExplorer();

        var authEnabled = configuration.GetValue<bool>("Auth:Enabled");

        services.AddSwaggerGen(options =>
        {
            if (authEnabled)
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "opaque",
                    In = ParameterLocation.Header,
                    Description = "OAuth token"
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            }
        });

        // Localization
        services.AddLocalization();
        services.AddScoped<IRequestLanguageContext, RequestLanguageContext>();
        var supportedCultures = new[] { new System.Globalization.CultureInfo("fa-IR"), new System.Globalization.CultureInfo("en-US") };
        services.Configure<RequestLocalizationOptions>(options =>
        {
            options.DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("fa-IR");
            options.SupportedCultures = supportedCultures;
            options.SupportedUICultures = supportedCultures;
        });

        // General options
        services.Configure<PaymentOptions>(configuration.GetSection("Payment"));
        services.Configure<RedisLockOptions>(configuration.GetSection("RedisLock"));

        services.AddHttpContextAccessor();
        services.AddHttpClient();
        services.AddHttpClient("NotifierProxy");

        // ── Actor Context & Permission Checker ──────────────────────────
        services.AddScoped<ICurrentActor, ClaimsActor>();
        services.AddScoped<AnonymousActor>();
        services.AddScoped<IPermissionChecker, DefaultPermissionChecker>();

        // ── Conditional Authentication ──────────────────────────────────
        if (authEnabled)
        {
            var authSection = configuration.GetSection("Auth");

            // OAuth introspection (external token validation)
            services.AddOAuthIntrospection(options =>
            {
                options.AuthServiceUrl = authSection["ServerUrl"] ?? "http://localhost:8080";
                options.IntrospectionPath = authSection["IntrospectionPath"] ?? "/api/v1/oauth/introspect";
                options.ClientId = authSection["ClientId"] ?? "payment-service";
                options.ClientSecret = authSection["ClientSecret"] ?? "";
                options.CacheSeconds = 300;
            });

            // Auth client for service-to-service calls
            services.AddAuthClient(options =>
            {
                options.BaseUrl = authSection["ServerUrl"] ?? "http://localhost:8080";
                options.ClientId = authSection["ClientId"] ?? "payment-service";
                options.ClientSecret = authSection["ClientSecret"] ?? "";
            });

            // Authorization policies
            services.AddAuthorization(options =>
            {
                options.AddPolicy("PaymentRead", policy => policy.RequireClaim("permission", "PAYMENT_READ"));
                options.AddPolicy("PaymentWrite", policy => policy.RequireClaim("permission", "PAYMENT_WRITE"));
                options.AddPolicy("PaymentManage", policy => policy.RequireClaim("permission", "PAYMENT_MANAGE"));
                options.AddPolicy("WalletRead", policy => policy.RequireClaim("permission", "WALLET_READ"));
                options.AddPolicy("WalletAdmin", policy => policy.RequireClaim("permission", "WALLET_MANAGE"));
                options.AddPolicy("WalletManage", policy => policy.RequireClaim("permission", "WALLET_MANAGE"));
                options.AddPolicy("PayoutRead", policy => policy.RequireClaim("permission", "PAYOUT_READ"));
                options.AddPolicy("PayoutManage", policy => policy.RequireClaim("permission", "PAYMENT_MANAGE"));
                options.AddPolicy("RiskView", policy => policy.RequireClaim("permission", "payment.risk.view"));
                options.AddPolicy("RiskAdmin", policy => policy.RequireClaim("permission", "payment.risk.manage"));
                options.AddPolicy("ComplianceView", policy => policy.RequireClaim("permission", "payment.compliance.view"));
                options.AddPolicy("ComplianceManage", policy => policy.RequireClaim("permission", "payment.compliance.manage"));
                options.AddPolicy("ApprovalView", policy => policy.RequireClaim("permission", "payment.approval.view"));
                options.AddPolicy("ApprovalDecide", policy => policy.RequireClaim("permission", "payment.approval.decide"));
                options.AddPolicy("WebhookView", policy => policy.RequireClaim("permission", "payment.webhook.view"));
                options.AddPolicy("WebhookAdmin", policy => policy.RequireClaim("permission", "payment.webhook.create_admin"));
                options.AddPolicy("DiagnosticsView", policy => policy.RequireClaim("permission", "payment.diagnostics.view"));
                options.AddPolicy("ReportsView", policy => policy.RequireClaim("permission", "payment.reports.view"));
                options.AddPolicy("SettingsRead", policy => policy.RequireClaim("permission", "SETTINGS_READ"));
                options.AddPolicy("SettingsManage", policy => policy.RequireClaim("permission", "SETTINGS_MANAGE"));
            });
        }
        else
        {
            // No-op auth: authenticate all requests as anonymous so [Authorize] attributes don't crash.
            // The actual policies (WalletAdmin, etc.) will pass because we add a fallback handler.
            services.AddAuthentication(NoOpAuthHandler.SchemeName)
                .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, NoOpAuthHandler>(NoOpAuthHandler.SchemeName, null);

            services.AddAuthorization(options =>
            {
                // Register all policies so [Authorize(Policy="...")] doesn't fail resolution
                var policies = new[]
                {
                    "PaymentRead", "PaymentWrite", "PaymentManage",
                    "WalletRead", "WalletAdmin", "WalletManage",
                    "PayoutRead", "PayoutManage",
                    "RiskView", "RiskAdmin",
                    "ComplianceView", "ComplianceManage",
                    "ApprovalView", "ApprovalDecide",
                    "WebhookView", "WebhookAdmin",
                    "DiagnosticsView", "ReportsView",
                    "SettingsRead", "SettingsManage",
                };
                foreach (var policy in policies)
                    options.AddPolicy(policy, p => p.RequireAssertion(_ => true));
            });
        }

        // Tenant context (works with or without auth)
        services.AddScoped<ITenantContext, ClaimsTenantContext>();
        services.AddScoped(sp => (ClaimsTenantContext)sp.GetRequiredService<ITenantContext>());

        // Data Protection for gateway secret encryption
        services.AddDataProtection();

        // Health checks
        services.AddHealthChecks()
            .AddSqlServer(configuration.GetConnectionString("DefaultConnection") ?? "", name: "sqlserver")
            .AddRedis(configuration.GetConnectionString("Redis") ?? "localhost:6379", name: "redis");

        // Payment gateway (Parbad)
        services.AddPaymentGateway(configuration, environment);

        // Payment SDK client
        services.AddPaymentClient(options =>
        {
            options.BaseUrl = configuration["Payment:PublicBaseUrl"] ?? "https://payment.example.com";
        });

        return services;
    }
}
