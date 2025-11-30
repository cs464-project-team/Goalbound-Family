using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using DotNetEnv;

namespace GoalboundFamily.Api.Data;

/// <summary>
/// Design-time factory for creating ApplicationDbContext during migrations
/// This allows EF Core tools to create the DbContext without running the full application
/// </summary>
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        // Load environment variables from .env file
        var envFile = Path.Combine(Directory.GetCurrentDirectory(), ".env");
        if (File.Exists(envFile))
        {
            Env.Load(envFile);
        }

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        // Build connection string from environment variables (same as Program.cs)
        var dbHost = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
        var dbPort = Environment.GetEnvironmentVariable("DB_PORT") ?? "5432";
        var dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? "postgres";
        var dbUser = Environment.GetEnvironmentVariable("DB_USER") ?? "postgres";
        var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "postgres";

        var connectionString = $"Host={dbHost};Port={dbPort};Database={dbName};Username={dbUser};Password={dbPassword};SSL Mode=Require;Trust Server Certificate=true;Timeout=30;CommandTimeout=30;Pooling=true;MinPoolSize=0;MaxPoolSize=10";

        optionsBuilder.UseNpgsql(
            connectionString,
            npgsqlOptions => npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorCodesToAdd: null
            )
        );

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
