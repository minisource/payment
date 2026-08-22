using Application;
using Infrastructure;
using Infrastructure.Payment;
using Presentaion;
using Presentaion.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Layer-by-layer service registration
builder.Services
    .AddPaymentApi(builder.Configuration, builder.Environment)
    .AddPaymentInfrastructure(builder.Configuration)
    .AddPaymentApplication(builder.Configuration);

// Ensure Kestrel listens on configured port when running in container
var kestrelPort = builder.Configuration["PORT"];
if (!string.IsNullOrEmpty(kestrelPort))
{
    builder.WebHost.UseUrls($"http://*:{kestrelPort}");
}

var app = builder.Build();

// ── CORS — allow all origins for local/dev ─────────────────────────────
app.UseCors(policy => policy
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

// ── Startup tasks (single-responsibility: delegated to extensions) ─────
await app.ApplyMigrationsAsync();
await app.ApplySeedDataAsync();
app.ValidateAuthSafety();

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

// Language detection middleware (must run before exception handling)
app.UseMiddleware<RequestLanguageMiddleware>();

// Exception handling middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseRequestLocalization();

// ── Auth Middleware (always enabled — uses no-op pass-through when Auth:Enabled=false) ──
app.UseAuthentication();
app.UseAuthorization();

// Tenant context extraction (after auth so claims are available; works standalone too)
app.UseMiddleware<TenantContextMiddleware>();

app.UsePaymentGateway(builder.Environment);

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
