using Domain.Gateways;
using Domain.Repositories;
using Infrastructure.Gateways;
using Infrastructure.Gateways.Adapters;
using Infrastructure.Gateways.CustomGateways.AzkiVam;
using Infrastructure.Gateways.CustomGateways.Tara;
using Infrastructure.Locking;
using Infrastructure.Payment;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Minisource.Common.Domain;
using Minisource.Common.Locking;
using StackExchange.Redis;

namespace Infrastructure;

/// <summary>
/// Registers all Infrastructure layer services with DI container.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddPaymentInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        var redisConnectionString = configuration.GetConnectionString("Redis") ?? "localhost:6379";

        // SQL Server DbContext
        services.AddDbContext<PaymentDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Redis connection for distributed locking
        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(redisConnectionString));

        // Distributed lock service
        services.AddSingleton(sp =>
        {
            var config = configuration.GetSection("RedisLock").Get<RedisLockOptions>() ?? new RedisLockOptions();
            return config;
        });
        services.AddSingleton<IDistributedLockService, RedisDistributedLockService>();

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Payment repositories (old system)
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IWalletRepository, WalletRepository>();
        services.AddScoped<IWalletTransactionRepository, WalletTransactionRepository>();

        // Wallet foundation repositories
        services.AddScoped<IWalletAccountRepository, WalletAccountRepository>();
        services.AddScoped<IWalletLedgerRepository, WalletLedgerRepository>();
        services.AddScoped<IWalletTransactionRecordRepository, WalletTransactionRecordRepository>();

        // Infrastructure repositories
        services.AddScoped<ISettingsRepository, SettingsRepository>();
        services.AddScoped<IIdempotencyRecordRepository, IdempotencyRecordRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IOutboxEventRepository, OutboxEventRepository>();

        // Webhook repositories
        services.AddScoped<IWebhookSubscriptionRepository, WebhookSubscriptionRepository>();
        services.AddScoped<IWebhookDeliveryRepository, WebhookDeliveryRepository>();

        // Gateway adapter & secret protection
        services.AddScoped<IPaymentGatewayAdapterFactory, GatewayAdapterFactory>();
        services.AddScoped<IGatewaySecretProtector, GatewaySecretProtector>();

        // Custom gateway adapters
        services.AddScoped<AzkiVamGatewayAdapter>();
        services.AddScoped<TaraGatewayAdapter>();

        // HTTP clients for custom gateways
        services.AddHttpClient<AzkiVamGatewayAdapter>();
        services.AddHttpClient<TaraGatewayAdapter>();

        // Gateway repositories (composite class implementing all gateway interfaces)
        services.AddScoped<GatewayRepositories>();
        services.AddScoped<IPaymentIntentRepository>(sp => sp.GetRequiredService<GatewayRepositories>());
        services.AddScoped<IPaymentTransactionRepository>(sp => sp.GetRequiredService<GatewayRepositories>());
        services.AddScoped<IGatewayProviderRepository>(sp => sp.GetRequiredService<GatewayRepositories>());
        services.AddScoped<IGatewayConfigRepository>(sp => sp.GetRequiredService<GatewayRepositories>());
        services.AddScoped<IGatewayRoutingPolicyRepository>(sp => sp.GetRequiredService<GatewayRepositories>());
        services.AddScoped<IGatewayRoutingRuleRepository>(sp => sp.GetRequiredService<GatewayRepositories>());

        // Payment link repository
        services.AddScoped<IPaymentLinkRepository, PaymentLinkRepository>();

        // Payout repositories (composite class)
        services.AddScoped<PayoutRepositories>();
        services.AddScoped<IPayoutAccountRepository>(sp => sp.GetRequiredService<PayoutRepositories>());
        services.AddScoped<IWithdrawalRequestRepository>(sp => sp.GetRequiredService<PayoutRepositories>());
        services.AddScoped<IPayoutRecordRepository>(sp => sp.GetRequiredService<PayoutRepositories>());

        // Refund repositories
        services.AddScoped<IRefundRequestRepository, RefundRequestRepository>();
        services.AddScoped<IWalletHoldRepository, WalletHoldRepository>();

        // Data seeder (runs at startup when Migrations:AutoSeed=true)
        services.AddScoped<PaymentDataSeeder>();

        // HTTP clients for webhook and notifier publishing
        services.AddHttpClient("WebhookClient");
        services.AddHttpClient("NotifierClient");

        return services;
    }
}
