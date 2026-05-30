
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MultiTenant.Domain.Entities;

namespace MultiTenant.Infrastructure.Persistence;

public class MasterDbContext : IdentityDbContext<ApplicationUser>
{
    public MasterDbContext(DbContextOptions<MasterDbContext> options) : base(options)
    {
    }

    public DbSet<Tenant> Tenants { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder.ApplyConfigurationsFromAssembly(typeof(MasterDbContext).Assembly);
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
