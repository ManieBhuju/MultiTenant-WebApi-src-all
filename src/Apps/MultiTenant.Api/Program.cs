using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using MultiTenant.Application;
using MultiTenant.Application.Common.Models;
using MultiTenant.Domain.Entities;
using MultiTenant.Infrastructure;
using MultiTenant.Infrastructure.Identity;
using MultiTenant.Infrastructure.MultiTenancy;
using MultiTenant.Infrastructure.Persistence;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var env = builder.Environment.EnvironmentName;
var appName = builder.Environment.ApplicationName;

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddApplication(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddRouting(options => options.LowercaseUrls = true);

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MultiTenant API",
        Version = "v1"
    });
    // Enable authorization using JWT in Swagger (HTTP Bearer)
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer {token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    options.AddSecurityRequirement(document =>
                    new OpenApiSecurityRequirement
                    {
                        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
                    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHealthChecks().AddDbContextCheck<MasterDbContext>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
builder.Services.AddFluentValidationAutoValidation().AddFluentValidationClientsideAdapters();
builder.Services.AddLogging();


var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.IncludeErrorDetails = true; // Include error details in the response for easier debugging
        options.MapInboundClaims = false; // Prevent automatic claim type mapping to preserve original claim types
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtOptions!.Issuer,
            ValidAudience = jwtOptions.Audience,

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtOptions.Key)),
            ClockSkew = TimeSpan.FromSeconds(30), // Optional: reduce default clock skew for token expiration

            RoleClaimType = "role", 
            NameClaimType = System.Security.Claims.ClaimTypes.NameIdentifier,
        };
    });

builder.Services.AddAuthorization();


var app = builder.Build();


var isTestEnv = env == "Test" || env == "Testing";
if (!isTestEnv)
{
    if (builder.Environment.IsDevelopment())
    {
        //builder.Configuration.AddUserSecrets<Program>();
        app.MapOpenApi();
    }
    else if (env.ToUpper() == "DEVSERVER" || env.ToUpper() == "UATSERVER")
    {

    }
}
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var masterDbContext = services.GetRequiredService<MasterDbContext>();
        logger.LogInformation("Migrating master database...");
        await masterDbContext.Database.MigrateAsync();

        // Migrate each tenant database individually using their connection strings from the master DB.
        var tenants = await masterDbContext.Tenants.AsNoTracking().ToListAsync();
        foreach (var tenant in tenants)
        {
            // Retry/backoff strategy for transient failures
            var maxAttempts = 3;
            var attempt = 0;
            var delay = TimeSpan.FromSeconds(2);
            while (attempt < maxAttempts)
            {
                attempt++;
                try
                {
                    logger.LogInformation("Migrating tenant {TenantId} (attempt {Attempt})", tenant.TenantId, attempt);
                    var optionsBuilder = new DbContextOptionsBuilder<TenantDbContext>();
                    optionsBuilder.UseNpgsql(tenant.DbConnStr);
                    using var tenantContext = new TenantDbContext(optionsBuilder.Options);
                    await tenantContext.Database.MigrateAsync();
                    logger.LogInformation("Successfully migrated tenant {TenantId}", tenant.TenantId);
                    break;
                }
                catch (Exception innerEx)
                {
                    logger.LogWarning(innerEx, "Failed to migrate tenant {TenantId} on attempt {Attempt}", tenant.TenantId, attempt);
                    if (attempt >= maxAttempts)
                    {
                        logger.LogError(innerEx, "Giving up migrating tenant {TenantId} after {Attempts} attempts", tenant.TenantId, attempt);
                    }
                    else
                    {
                        await Task.Delay(delay);
                        delay = delay * 2; // exponential backoff
                    }
                }
            }
        }

        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        await IdentitySeeder.SeedAsync(userManager, roleManager);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while migrating and seeding the database");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();

app.UseRouting();


app.UseAuthentication();
//app.UseMiddleware<TenantResolutionMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();
