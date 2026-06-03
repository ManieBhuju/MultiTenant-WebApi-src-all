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
    private readonly MasterDbContext _masterDb;

    public TenantService(IConfiguration configuration, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _configuration = configuration;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<ServiceResult<string>> CreateTenantAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        var masterConn = _configuration.GetConnectionString("MasterConnection");
        // Use the base connection string from configuration and only replace the Database name
        var builder = new NpgsqlConnectionStringBuilder(masterConn)
        {
            Database = tenant.TenantId
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

            var options = new DbContextOptionsBuilder<TenantDbContext>();
            options.UseNpgsql(tenantConn);
            using var tenantContext = new TenantDbContext(options.Options);
            await tenantContext.Database.EnsureCreatedAsync(cancellationToken);

            // create admin user in master DB
            var adminPassword = _configuration["DefaultTenantAdminPassword"] ?? "Tester@123";
            var adminUser = new ApplicationUser { UserName = tenant.EmailAddress, Email = tenant.EmailAddress, EmailConfirmed = true, TenantId = tenant.Id };
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

    public async Task<ServiceResult> DeleteTenantAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        var tenant = await _masterDb.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken);
        if (tenant == null)
            return ServiceResult.Failed<string>(new ServiceError("NotFound", "Tenant not found"));

        var masterConn = _configuration.GetConnectionString("MasterConnection");
        var dbName = tenant.TenantId;
        try
        {
            var maintenance = new NpgsqlConnectionStringBuilder(masterConn) { Database = "postgres" };
            await using var conn = new NpgsqlConnection(maintenance.ToString());
            await conn.OpenAsync(cancellationToken);
            await using var cmd = conn.CreateCommand();
            // terminate other connections to the tenant DB
            cmd.CommandText = $"SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname = '{dbName}' AND pid <> pg_backend_pid();";
            await cmd.ExecuteNonQueryAsync(cancellationToken);

            cmd.CommandText = $"DROP DATABASE IF EXISTS \"{dbName}\"";
            await cmd.ExecuteNonQueryAsync(cancellationToken);

            // remove master record
            _masterDb.Tenants.Remove(tenant);
            await _masterDb.SaveChangesAsync(cancellationToken);

            return ServiceResult.Success(tenant.Id);
        }
        catch (Exception ex)
        {
            return ServiceResult.Failed<string>(new ServiceError("DeleteTenantFailed", ex.Message));
        }
    }

    public async Task<ServiceResult> UpdateTenantAsync(Tenant tenant, string? oldTenantId = null, CancellationToken cancellationToken = default)
    {
        // tenant is expected to be tracked by _masterDb in same scope
        var existing = await _masterDb.Tenants.FirstOrDefaultAsync(t => t.Id == tenant.Id, cancellationToken);
        if (existing == null)
            return ServiceResult.Failed<string>(new ServiceError("NotFound", "Tenant not found"));

        var masterConn = _configuration.GetConnectionString("MasterConnection");

        // if TenantId (database name) changed and oldTenantId provided, attempt rename
        if (!string.IsNullOrWhiteSpace(oldTenantId) && oldTenantId != tenant.TenantId)
        {
            try
            {
                var maintenance = new NpgsqlConnectionStringBuilder(masterConn) { Database = "postgres" };
                await using var conn = new NpgsqlConnection(maintenance.ToString());
                await conn.OpenAsync(cancellationToken);
                await using var cmd = conn.CreateCommand();
                // terminate other connections to the old DB
                cmd.CommandText = $"SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname = '{oldTenantId}' AND pid <> pg_backend_pid();";
                await cmd.ExecuteNonQueryAsync(cancellationToken);

                // rename database
                cmd.CommandText = $"ALTER DATABASE \"{oldTenantId}\" RENAME TO \"{tenant.TenantId}\"";
                await cmd.ExecuteNonQueryAsync(cancellationToken);

                // update DbConnStr if present
                if (!string.IsNullOrWhiteSpace(existing.DbConnStr))
                {
                    try
                    {
                        var builder = new NpgsqlConnectionStringBuilder(existing.DbConnStr)
                        {
                            Database = tenant.TenantId
                        };
                        existing.DbConnStr = builder.ToString();
                    }
                    catch
                    {
                        // ignore connection string parse errors
                    }
                }
            }
            catch (Exception ex)
            {
                return ServiceResult.Failed<string>(new ServiceError("RenameDbFailed", ex.Message));
            }
        }

        // apply metadata updates
        existing.Name = tenant.Name;
        existing.EmailAddress = tenant.EmailAddress;
        existing.TenantId = tenant.TenantId;
        if (!string.IsNullOrWhiteSpace(tenant.DbConnStr))
            existing.DbConnStr = tenant.DbConnStr;
        existing.ModifiedAt = DateTime.UtcNow;

        await _masterDb.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success(tenant);
    }
}
