using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Reflection;

namespace MultiTenant.Infrastructure.Persistence;

public class MasterDbContextFactory : IDesignTimeDbContextFactory<MasterDbContext>
{
    public MasterDbContext CreateDbContext(string[] args)
    {
        // Build configuration from appsettings, environment variables and user secrets (if available)
        var env = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Development";

        var configBuilder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables();

        try
        {
            var entryAssembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
            configBuilder.AddUserSecrets(entryAssembly, optional: true);
        }
        catch
        {
            // ignore if user-secrets are not available
        }

        var configuration = configBuilder.Build();

        // Look up connection string in multiple places: ConnectionStrings:MasterConnection, MasterConnection env var, direct key
        var connectionString = configuration.GetConnectionString("MasterConnection")
                               ?? Environment.GetEnvironmentVariable("MasterConnection")
                               ?? configuration["ConnectionStrings:MasterConnection"]
                               ?? configuration["MasterConnection"];

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            // Fallback development connection string - change if needed
            //connectionString = "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=Admin@123";
            Console.WriteLine("[DesignTime] Using fallback MasterConnection. Consider setting MasterConnection via environment or user-secrets.");
        }

        var builder = new DbContextOptionsBuilder<MasterDbContext>();
        builder.UseNpgsql(connectionString);
        return new MasterDbContext(builder.Options);
    }
}
