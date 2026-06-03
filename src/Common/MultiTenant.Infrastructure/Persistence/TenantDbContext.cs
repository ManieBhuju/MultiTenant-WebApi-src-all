
using Microsoft.EntityFrameworkCore;
using MultiTenant.Domain.Entities;
using MultiTenant.Application.Common.Interfaces;
using MultiTenant.Infrastructure.Persistence.Configurations;

namespace MultiTenant.Infrastructure.Persistence;

public class TenantDbContext : DbContext, ITenantDbContext
{
    public TenantDbContext(DbContextOptions<TenantDbContext> options) : base(options)
    {
    }

    public DbSet<Employee> Employees { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Explicitly apply only tenant-specific configurations (Employees).
        // Avoid scanning the assembly which would pull in master DB configurations
        // (Identity, Tenants, etc.) and cause those tables to be created in tenant DBs.
        modelBuilder.ApplyConfiguration(new EmployeeConfiguration());
    }
}
