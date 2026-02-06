using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Parbad.Builder;
using Parbad.Gateway.IdPay;
using Parbad.Gateway.PayIr;
using Parbad.Gateway.ZarinPal;
using Parbad.Gateway.Zibal;
using Parbad.Gateway.ParbadVirtual;
using Infrastructure.Data;

namespace Infrastructure.Payment
{
    public static class PaymentExtension
    {
        public static void AddPaymentGateway(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment _env)
        {
            services.AddParbad()
                .ConfigureGateways(gateways =>
                {
                    #region ZarinPal
                    gateways
                    .AddZarinPal()
                    .WithAccounts(accounts =>
                    {
                        accounts.AddInMemory(account =>
                        {
                            account.MerchantId = configuration["Payment:Gateways:ZarinPal:MerchantId"] ?? "test-merchant-id";
                            account.IsSandbox = bool.Parse(configuration["Payment:Gateways:ZarinPal:IsSandbox"] ?? "true");
                        });
                    });
                    #endregion

                    #region IdPay
                    gateways
                    .AddIdPay()
                    .WithAccounts(accounts =>
                    {
                        accounts.AddInMemory(account =>
                        {
                            account.Api = configuration["Payment:Gateways:IdPay:Api"] ?? "test-api";
                            account.IsTestAccount = bool.Parse(configuration["Payment:Gateways:IdPay:IsTestAccount"] ?? "true");
                        });
                    });
                    #endregion

                    #region Pay.ir
                    gateways
                    .AddPayIr()
                    .WithAccounts(accounts =>
                    {
                        accounts.AddInMemory(account =>
                        {
                            account.Api = configuration["Payment:Gateways:PayIr:Api"] ?? "test-api";
                            account.IsTestAccount = bool.Parse(configuration["Payment:Gateways:PayIr:IsTestAccount"] ?? "true");
                        });
                    });
                    #endregion

                    #region Zibal
                    gateways
                    .AddZibal()
                    .WithAccounts(accounts =>
                    {
                        accounts.AddInMemory(account =>
                        {
                            account.Merchant = configuration["Payment:Gateways:Zibal:Merchant"] ?? "test-merchant";
                        });
                    });
                    #endregion

                    #region ParbadVirtual (for development)
                    if (!_env.IsProduction())
                    {
                        gateways.AddParbadVirtual()
                            .WithOptions(options => options.GatewayPath = "/MyVirtualGateway");
                    }
                    #endregion
                })
                .ConfigureHttpContext(httpContextBuilder => httpContextBuilder.UseDefaultAspNetCore())
                .ConfigureStorage(builder => builder.AddStorage<PaymentStorageRepository>(ServiceLifetime.Transient))
                .ConfigureOptions(options =>
                {
                    options.EnableLogging = true;

                    options.Messages.PaymentSucceed = "Payment completed successfully";
                    options.Messages.PaymentFailed = "Payment failed";
                    options.Messages.DuplicateTrackingNumber = "Tracking number already exists";
                    options.Messages.UnexpectedErrorText = "An unexpected error occurred";
                    options.Messages.InvalidDataReceivedFromGateway = "Invalid data received from gateway";
                    options.Messages.PaymentIsAlreadyProcessedBefore = "Payment already processed";
                    options.Messages.PaymentCanceledProgrammatically = "Payment canceled";
                    options.Messages.OnlyCompletedPaymentCanBeRefunded = "Only completed payments can be refunded";
                })
                .ConfigureAutoIncrementTrackingNumber(options =>
                {
                    options.MinimumValue = 10000;
                    options.Increment = 1;
                });
        }

        public static void UsePaymentGateway(this IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (!env.IsProduction())
                app.UseParbadVirtualGateway();
        }
    }
}
