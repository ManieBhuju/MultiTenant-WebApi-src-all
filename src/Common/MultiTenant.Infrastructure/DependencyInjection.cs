using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MultiTenant.Application.Common.Interfaces;
using MultiTenant.Infrastructure.MultiTenancy;
using MultiTenant.Infrastructure.Persistence;

namespace MultiTenant.Infrastructure;

public static class DependencyInjection
{

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ITenantProvider, TenantProvider>();

        services.AddDbContext<MasterDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("MasterConnection"));
        });

        services.AddDbContext<TenantDbContext>((sp, options) =>
        {
            var tenantProvider = sp.GetRequiredService<ITenantProvider>();
            var masterDb = sp.GetRequiredService<MasterDbContext>();
            var tenantId = tenantProvider.GetTenantId();
            var tenant = masterDb.Tenants.FirstOrDefault(x => x.TenantId == tenantId);

            if (tenant == null)
                throw new Exception("Tenant not found");

            options.UseNpgsql(tenant.DbConnStr);
        });

        return services;
    }

}
