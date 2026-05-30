using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MultiTenant.Application.Common.Interfaces;
using MultiTenant.Infrastructure.MultiTenancy;
using MultiTenant.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MultiTenant.Application.Common.Models;
using MultiTenant.Infrastructure.Identity;

namespace MultiTenant.Infrastructure;

public static class DependencyInjection
{

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ITenantProvider, TenantProvider>();
        services.AddScoped<IJwtService, JwtService>();
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

        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));

        var jwtSection = configuration.GetSection("Jwt");
        var key = jwtSection.GetValue<string>("Key");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options => { 
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSection.GetValue<string>("Issuer"),
                ValidAudience = jwtSection.GetValue<string>("Audience"),
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
            };
        });

        return services;
    }

}
