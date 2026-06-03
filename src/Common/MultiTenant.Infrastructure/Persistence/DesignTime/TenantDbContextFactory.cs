using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Reflection;

namespace MultiTenant.Infrastructure.Persistence;

public class TenantDbContextFactory : IDesignTimeDbContextFactory<TenantDbContext>
{
    public TenantDbContext CreateDbContext(string[] args)
    {
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
        }

        var configuration = configBuilder.Build();

        var connectionString = configuration["TenantConnection"]
                               ?? Environment.GetEnvironmentVariable("TenantConnection")
                               ?? "Host=localhost;Port=5432;Database=tenant_db;Username=postgres;Password=Admin@123";

        var builder = new DbContextOptionsBuilder<TenantDbContext>();
        builder.UseNpgsql(connectionString);
        return new TenantDbContext(builder.Options);
    }
}
