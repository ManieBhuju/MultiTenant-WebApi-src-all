using MultiTenant.Application.Common.Models;
using MultiTenant.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace MultiTenant.Application.Common.Interfaces;

public interface ITenantService
{
    /// <summary>
    /// Creates tenant database, applies migrations and creates default admin user.
    /// Returns ServiceResult with the tenant DB connection string on success.
    /// </summary>
    Task<ServiceResult<string>> CreateTenantAsync(Tenant tenant, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes tenant database and removes tenant record from master DB.
    /// </summary>
    Task<ServiceResult> DeleteTenantAsync(string tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates tenant metadata and optionally renames the tenant database when TenantId (DB name) changes.
    /// </summary>
    Task<ServiceResult> UpdateTenantAsync(Tenant tenant, string? oldTenantId = null, CancellationToken cancellationToken = default);
}
