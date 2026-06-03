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
using MultiTenant.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using MultiTenant.Infrastructure.Services;
using Microsoft.Extensions.Logging;

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

        services.AddScoped<IMasterDbContext>(sp => sp.GetRequiredService<MasterDbContext>());

        // Register application-facing tenant DB context interface
        services.AddScoped<ITenantDbContext>(sp => sp.GetRequiredService<TenantDbContext>());

        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
        })
            .AddEntityFrameworkStores<MasterDbContext>()
            .AddDefaultTokenProviders();

        var jwtSettings = configuration.GetSection("Jwt").Get<JwtSettings>();
        if (jwtSettings == null || string.IsNullOrWhiteSpace(jwtSettings.Key))
        {
            throw new Exception("JWT Settings or Key is not configured correctly in appsettings.json.");
        }
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.IncludeErrorDetails = true;
            options.MapInboundClaims = false; // Preserves literal 'role' claim strings
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,

                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
                ClockSkew = TimeSpan.FromSeconds(30),

                RoleClaimType = "role",
                NameClaimType = System.Security.Claims.ClaimTypes.NameIdentifier,
            };
        });

        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));

        services.AddDbContext<TenantDbContext>((sp, options) =>
        {
            var tenantProvider = sp.GetRequiredService<ITenantProvider>();
            var masterDb = sp.GetRequiredService<MasterDbContext>();
            var tenantId = tenantProvider.GetTenantId();
            // TenantProvider returns ApplicationUser.TenantId which is tenant.Id (PK). Lookup by Id.
            var tenant = masterDb.Tenants.FirstOrDefault(x => x.Id == tenantId);

            if (tenant == null)
                throw new Exception("Tenant not found");

            options.UseNpgsql(tenant.DbConnStr);
        });

        // service for resolving tenants outside of HTTP context if needed
        services.AddSingleton<TenantResolver>();

        // Tenant service used by application layer via interface
        services.AddScoped<ITenantService, TenantService>();

        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));

        return services;
    }

}
