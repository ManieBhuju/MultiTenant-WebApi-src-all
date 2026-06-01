using MultiTenant.Application.Common.Interfaces;
using MultiTenant.Application.Common.Models;
using MultiTenant.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Npgsql;
using MultiTenant.Infrastructure.Persistence;

namespace MultiTenant.Infrastructure.Services;

public class TenantService : ITenantService
{
    private readonly IConfiguration _configuration;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public TenantService(IConfiguration configuration, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _configuration = configuration;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<ServiceResult<string>> CreateTenantAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        var masterConn = _configuration.GetConnectionString("MasterConnection");
        var builder = new NpgsqlConnectionStringBuilder(masterConn)
        {
            Database = $"tenant_{tenant.TenantId}"
        };

        var tenantConn = builder.ToString();

        try
        {
            // Create DB if not exists
            var maintenance = new NpgsqlConnectionStringBuilder(masterConn) { Database = "postgres" };
            using var conn = new NpgsqlConnection(maintenance.ToString());
            await conn.OpenAsync(cancellationToken);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $"SELECT 1 FROM pg_database WHERE datname = '{builder.Database}'";
            var exists = (await cmd.ExecuteScalarAsync(cancellationToken)) != null;
            if (!exists)
            {
                cmd.CommandText = $"CREATE DATABASE \"{builder.Database}\"";
                await cmd.ExecuteNonQueryAsync(cancellationToken);
            }

            // Migrate tenant DB
            var options = new DbContextOptionsBuilder<TenantDbContext>();
            options.UseNpgsql(tenantConn);
            using var tenantContext = new TenantDbContext(options.Options);
            await tenantContext.Database.MigrateAsync(cancellationToken);

            // create admin user in master DB
            var adminPassword = _configuration["DefaultTenantAdminPassword"] ?? "Tester@123";
            var adminUser = new ApplicationUser { UserName = tenant.EmailAddress, Email = tenant.EmailAddress, EmailConfirmed = true, TenantId = tenant.TenantId };
            var createResult = await _userManager.CreateAsync(adminUser, adminPassword);
            if (!createResult.Succeeded)
                return ServiceResult.Failed<string>(new ServiceError("CreateAdminFailed", string.Join(";", createResult.Errors.Select(e => e.Description))));

            if (!await _roleManager.RoleExistsAsync("Admin"))
                await _roleManager.CreateAsync(new IdentityRole("Admin"));

            await _userManager.AddToRoleAsync(adminUser, "Admin");

            return ServiceResult.Success(tenantConn);
        }
        catch (Exception ex)
        {
            return ServiceResult.Failed<string>(new ServiceError("TenantCreationFailed", ex.Message));
        }
    }
}
