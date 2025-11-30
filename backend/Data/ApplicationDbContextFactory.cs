using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GoalboundFamily.Api.Data;

/// <summary>
/// Design-time factory for creating ApplicationDbContext during migrations
/// This allows EF Core tools to create the DbContext without running the full application
/// </summary>
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        // Use a default connection string for migrations
        // This will be replaced with actual connection string at runtime
        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=goalbound_family;Username=postgres;Password=postgres",
            npgsqlOptions => npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorCodesToAdd: null
            )
        );

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
