using Microsoft.EntityFrameworkCore;
using MultiTenant.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace MultiTenant.Application.Common.Interfaces;

public interface ITenantDbContext
{
    DbSet<Employee> Employees { get; set; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
