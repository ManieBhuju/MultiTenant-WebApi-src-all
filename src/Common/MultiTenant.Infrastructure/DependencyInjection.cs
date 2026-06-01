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

        services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<MasterDbContext>()
            .AddDefaultTokenProviders();

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

        // service for resolving tenants outside of HTTP context if needed
        services.AddSingleton<TenantResolver>();

        // Tenant service used by application layer via interface
        services.AddScoped<ITenantService, TenantService>();

        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));

        var jwtSection = configuration.GetSection("Jwt");
        var key = jwtSection.GetValue<string>("Key");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options => { 
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
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

            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = ctx =>
                {
                    var logger = ctx.HttpContext.RequestServices.GetService<ILoggerFactory>()?.CreateLogger("JwtAuth");
                    logger?.LogError(ctx.Exception, "Authentication failed: {Message}", ctx.Exception.Message);
                    return Task.CompletedTask;
                },
                OnTokenValidated = ctx =>
                {
                    var logger = ctx.HttpContext.RequestServices.GetService<Microsoft.Extensions.Logging.ILoggerFactory>()?.CreateLogger("JwtAuth");
                    logger?.LogInformation("JWT token validated for {sub}", ctx.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                    return Task.CompletedTask;
                }
            };
        });

        return services;
    }

}
