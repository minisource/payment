using Application.Options;
using Application.Features.Approvals;
using Application.Features.Gateways;
using Application.Features.GatewayRouting;
using Application.Features.Ledger;
using Application.Features.Limits;
using Application.Features.Outbox;
using Application.Features.PaymentIntents;
using Application.Features.PaymentLinks;
using Application.Features.PaymentWalletPosting;
using Application.Features.Payments;
using Application.Features.PayoutAccounts;
using Application.Features.Reconciliation;
using Application.Features.Reports;
using Application.Features.Risk;
using Application.Features.Settings;
using Application.Features.Wallets;
using Application.Features.Webhooks;
using Application.Features.Withdrawals;
using Application.Features.Refunds;
using Application.Features.Notifications;
using Domain.Abstractions;
using Domain.Gateways;
using Domain.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

/// <summary>
/// Registers all Application layer services with DI container.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddPaymentApplication(
        this IServiceCollection services, IConfiguration configuration)
    {
        // Payment services (old system)
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IPaymentQueryService, PaymentQueryService>();
        services.AddScoped<IWalletService, WalletService>();

        // Wallet application services
        services.AddScoped<IAdminWalletService, AdminWalletService>();
        services.AddScoped<ISettingsService, SettingsService>();
        services.AddScoped<IWalletConsistencyService, WalletConsistencyService>();

        // Payment intent & gateway services
        services.AddScoped<IPaymentIntentService, PaymentIntentService>();
        services.AddScoped<IGatewayPaymentService, GatewayPaymentService>();
        services.AddScoped<IGatewayCallbackService, GatewayCallbackService>();
        services.AddScoped<IGatewayConfigService, GatewayConfigService>();
        services.AddScoped<IGatewayRoutingService, GatewayRoutingService>();
        services.AddScoped<IPaymentWalletPostingService, PaymentWalletPostingService>();

        // Payment link services
        services.AddScoped<IPaymentLinkService, PaymentLinkService>();
        services.AddScoped<IPublicPaymentService, PublicPaymentService>();

        // Withdrawal & payout services
        services.AddScoped<IPayoutAccountService, PayoutAccountService>();
        services.AddScoped<IWithdrawalService, WithdrawalService>();

        // Refund services
        services.AddScoped<IWalletHoldService, WalletHoldService>();
        services.AddScoped<IRefundService, RefundService>();
        services.AddScoped<IRefundQueryService, RefundQueryService>();

        // Financial security services
        services.AddScoped<ILedgerHashService, LedgerHashService>();
        services.AddScoped<ILedgerHashProvider>(sp => sp.GetRequiredService<ILedgerHashService>());
        services.AddScoped<IRiskEvaluationService, RiskEvaluationService>();
        services.AddScoped<IUserComplianceService, StubUserComplianceService>();
        services.AddScoped<IAdminApprovalService, AdminApprovalService>();
        services.AddScoped<ILimitUsageService, LimitUsageService>();
        services.AddScoped<IReconciliationService, ReconciliationService>();
        services.AddScoped<IBankStatementImportService, BankStatementImportService>();

        // Webhooks & event publishing services
        services.Configure<OutboxDispatcherOptions>(configuration.GetSection("Outbox"));
        services.Configure<NotifierOptions>(configuration.GetSection("Notifier"));
        services.AddSingleton<IWebhookSigningService, WebhookSigningService>();
        services.AddScoped<IIntegrationEventPublisher, WebhookEventPublisher>();
        services.AddScoped<IIntegrationEventPublisher, NotifierEventPublisher>();
        services.AddScoped<IIntegrationEventPublisher, NoopMessageBusPublisher>();

        // Notification service + in-process domain event dispatcher
        services.AddScoped<IPaymentNotificationService, PaymentNotificationService>();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        // Outbox dispatcher background worker
        services.AddHostedService<OutboxDispatcherHostedService>();

        return services;
    }
}
