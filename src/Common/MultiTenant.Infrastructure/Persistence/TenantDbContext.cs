
using Microsoft.EntityFrameworkCore;
using MultiTenant.Domain.Entities;
using MultiTenant.Application.Common.Interfaces;

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
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TenantDbContext).Assembly);
    }
}
