
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MultiTenant.Application.Common.Interfaces;
using MultiTenant.Domain.Entities;

namespace MultiTenant.Infrastructure.Persistence;

public class MasterDbContext : IdentityDbContext<ApplicationUser>, IMasterDbContext
{
    public MasterDbContext(DbContextOptions<MasterDbContext> options) : base(options)
    {
    }

    public DbSet<Tenant> Tenants { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Apply all configurations in this assembly except tenant-specific ones (e.g. Employee)
        builder.ApplyConfigurationsFromAssembly(
            typeof(MasterDbContext).Assembly,
            type =>
            {
                // Exclude IEntityTypeConfiguration<Employee> so Employees are not part of the master model
                var interfaces = type.GetInterfaces();
                foreach (var i in interfaces)
                {
                    if (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>))
                    {
                        var arg = i.GetGenericArguments()[0];
                        if (arg == typeof(Employee))
                            return false;
                    }
                }

                return true;
            });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is Tenant && (e.State == EntityState.Added || e.State == EntityState.Modified));
        foreach (var entry in entries)
        {
            var tenant = (Tenant)entry.Entity;

            if (entry.State == EntityState.Modified)
                tenant.ModifiedAt = DateTime.UtcNow;

            if (entry.State == EntityState.Added)
                tenant.CreatedAt = DateTime.UtcNow;
            
        }
        return await base.SaveChangesAsync(cancellationToken);
    }

}
