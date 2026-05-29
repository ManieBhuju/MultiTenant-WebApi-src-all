


using Microsoft.EntityFrameworkCore;
using MultiTenant.Domain.Entities;

namespace MultiTenant.Infrastructure.Persistence;

public class TenantDbContext : DbContext
{
    public TenantDbContext(DbContextOptions<TenantDbContext> options) : base(options)
    {
    }

    public DbSet<Employee> Employees { get; set; }
}
