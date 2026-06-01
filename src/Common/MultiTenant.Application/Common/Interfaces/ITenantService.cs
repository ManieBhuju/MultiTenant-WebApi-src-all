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
}
