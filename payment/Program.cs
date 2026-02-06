using Application.Options;
using Application.Services;
using Domain.Repositories;
using Infrastructure.Data;
using Infrastructure.Locking;
using Infrastructure.Payment;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Minisource.Common.Auth;
using Minisource.Common.Domain;
using Minisource.Common.Locking;
using Minisource.Sdk.Auth;
using Presentaion.Middlewares;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Configure forwarded headers for running behind Traefik/reverse proxy
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "opaque",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "OAuth token"
    });
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Configure payment options
builder.Services.Configure<PaymentOptions>(builder.Configuration.GetSection("Payment"));

// Configure Redis lock options
builder.Services.Configure<RedisLockOptions>(builder.Configuration.GetSection("RedisLock"));

builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();

// Add OAuth authentication using csharp-common
builder.Services.AddOAuthIntrospection(options =>
{
    options.AuthServiceUrl = builder.Configuration["Auth:ServerUrl"]
        ?? "http://localhost:8080";
    options.IntrospectionPath = builder.Configuration["Auth:IntrospectionPath"]
        ?? "/api/v1/oauth/introspect";
    options.ClientId = builder.Configuration["Auth:ClientId"] ?? "payment-service";
    options.ClientSecret = builder.Configuration["Auth:ClientSecret"] ?? "";
    options.CacheSeconds = 300;
});

// Add Auth client from csharp-sdk for service-to-service calls
builder.Services.AddAuthClient(options =>
{
    options.BaseUrl = builder.Configuration["Auth:ServerUrl"] ?? "http://localhost:8080";
    options.ClientId = builder.Configuration["Auth:ClientId"] ?? "payment-service";
    options.ClientSecret = builder.Configuration["Auth:ClientSecret"] ?? "";
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("WalletAdmin", policy => policy.RequireClaim("permission", "WALLET_MANAGE"));
    options.AddPolicy("WalletRead", policy => policy.RequireClaim("permission", "WALLET_READ"));
    options.AddPolicy("PaymentRead", policy => policy.RequireClaim("permission", "PAYMENT_READ"));
    options.AddPolicy("PaymentWrite", policy => policy.RequireClaim("permission", "PAYMENT_WRITE"));
});

// Ensure Kestrel listens on configured port when running in container
var kestrelPort = builder.Configuration["PORT"];
if (!string.IsNullOrEmpty(kestrelPort))
{
    builder.WebHost.UseUrls($"http://*:{kestrelPort}");
}

// Add PostgreSQL DbContext
builder.Services.AddDbContext<PaymentDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Redis connection for distributed locking
var redisConnectionString = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
    ConnectionMultiplexer.Connect(redisConnectionString));

// Add distributed lock service
builder.Services.AddSingleton(sp =>
{
    var config = builder.Configuration.GetSection("RedisLock").Get<RedisLockOptions>() ?? new RedisLockOptions();
    return config;
});
builder.Services.AddSingleton<IDistributedLockService, RedisDistributedLockService>();

// Add Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Add repositories
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IWalletRepository, WalletRepository>();
builder.Services.AddScoped<IWalletTransactionRepository, WalletTransactionRepository>();

// Add application services
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IWalletService, WalletService>();

// Add health checks
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection") ?? "", name: "postgres")
    .AddRedis(redisConnectionString, name: "redis");

// Add payment gateway
builder.Services.AddPaymentGateway(builder.Configuration, builder.Environment);

var app = builder.Build();

// Use forwarded headers for Traefik/reverse proxy
app.UseForwardedHeaders();

// Enable Swagger in all environments
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("./v1/swagger.json", "Payment API v1");
    c.RoutePrefix = "swagger";
});

// Only use HTTPS redirection when not behind a reverse proxy
if (!app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

// Exception handling middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.UsePaymentGateway(builder.Environment);

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
