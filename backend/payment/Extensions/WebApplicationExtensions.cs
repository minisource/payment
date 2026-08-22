using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// Extension methods on <see cref="WebApplication"/> for startup concerns
/// (auto-migration, seed data, production safety guard).
/// Keeps Program.cs clean and focused on pipeline ordering.
/// </summary>
public static class WebApplicationExtensions
{
    /// <summary>
    /// Applies pending EF Core migrations at startup (Code-First).
    /// Controlled by <c>Migrations:AutoMigrate</c> (default: <c>true</c>).
    /// </summary>
    public static async Task ApplyMigrationsAsync(this WebApplication app)
    {
        var autoMigrate = app.Configuration.GetValue<bool?>("Migrations:AutoMigrate") ?? true;
        if (!autoMigrate) return;

        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();

        try
        {
            await db.Database.MigrateAsync();
            app.Logger.LogInformation("Database migrations applied successfully");
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex,
                "Failed to apply database migrations. " +
                "Ensure the SQL Server container is running (docker-compose.dev.yml started).");
            throw;
        }
    }

    /// <summary>
    /// Seeds gateway providers, payment settings, and risk rules.
    /// Controlled by <c>Migrations:AutoSeed</c> (default: <c>true</c>).
    /// Must be called AFTER <see cref="ApplyMigrationsAsync"/>.
    /// </summary>
    public static async Task ApplySeedDataAsync(this WebApplication app)
    {
        var autoSeed = app.Configuration.GetValue<bool?>("Migrations:AutoSeed") ?? true;
        if (!autoSeed) return;

        using var scope = app.Services.CreateScope();

        try
        {
            var seeder = scope.ServiceProvider.GetRequiredService<PaymentDataSeeder>();
            await seeder.SeedAsync();
            app.Logger.LogInformation("Seed data applied successfully");
        }
        catch (Exception ex)
        {
            app.Logger.LogWarning(ex, "Seed data skipped or partially applied (non-critical)");
        }
    }

    /// <summary>
    /// Guards against running with auth disabled in production.
    /// Must be called AFTER the DI container is built but BEFORE the middleware pipeline runs.
    /// </summary>
    public static void ValidateAuthSafety(this WebApplication app)
    {
        var authEnabled = app.Configuration.GetValue<bool>("Auth:Enabled");
        var isProduction = app.Environment.IsProduction();
        var allowAuthDisabledInProduction =
            app.Configuration.GetValue<bool>("Payment:Security:AllowAuthDisabledInProduction");

        if (authEnabled) return;

        if (isProduction && !allowAuthDisabledInProduction)
        {
            var msg = "FATAL: Auth is disabled in production. "
                      + "Set Payment:Security:AllowAuthDisabledInProduction=true to override (unsafe).";
            app.Logger.LogCritical("{Msg}", msg);
            throw new InvalidOperationException(msg);
        }

        app.Logger.LogWarning(
            "WARNING: Payment auth is DISABLED. " +
            "This is only safe for local/dev/internal/private deployments.");
    }
}
