using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MultiTenant.Application;
using MultiTenant.Domain.Entities;
using MultiTenant.Infrastructure;
using MultiTenant.Infrastructure.Identity;
using MultiTenant.Infrastructure.MultiTenancy;
using MultiTenant.Infrastructure.Persistence;
using System.Text;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

var env = builder.Environment.EnvironmentName;
var appName = builder.Environment.ApplicationName;

var isTestEnv = env == "Test" || env == "Testing";
if (!isTestEnv)
{
    if (builder.Environment.IsDevelopment())
    {
        //builder.Configuration.AddUserSecrets<Program>();
    }
    else if (env.ToUpper() == "DEVSERVER" || env.ToUpper() == "UATSERVER")
    {

    }
}

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

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter JWT token. Example: Bearer eyJhbGciOi...",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        BearerFormat = "JWT"
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHealthChecks().AddDbContextCheck<MasterDbContext>();
builder.Services.AddHealthChecks().AddDbContextCheck<TenantDbContext>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
builder.Services.AddFluentValidationAutoValidation().AddFluentValidationClientsideAdapters();
builder.Services.AddLogging();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization();


var app = builder.Build();

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

app.UseRouting();

app.UseMiddleware<TenantResolutionMiddleware>();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
