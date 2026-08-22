using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Persistence;

/// <summary>
/// Design-time factory for PaymentDbContext.
/// Enables `dotnet ef migrations add` to work when the DbContext
/// is in a different project than the startup project.
/// Reads connection string from the payment project's appsettings.Development.json.
/// </summary>
public class PaymentDbContextFactory : IDesignTimeDbContextFactory<PaymentDbContext>
{
    public PaymentDbContext CreateDbContext(string[] args)
    {
        // Navigate from Infrastructure project to the payment (Presentation) project
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "payment");
        
        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Server=localhost,1433;Database=PaymentDb;User Id=sa;Password=YourPassword123!;TrustServerCertificate=True;Encrypt=False";

        var optionsBuilder = new DbContextOptionsBuilder<PaymentDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new PaymentDbContext(optionsBuilder.Options);
    }
}
