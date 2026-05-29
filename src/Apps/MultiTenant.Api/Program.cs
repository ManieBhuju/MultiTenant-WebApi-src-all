using MultiTenant.Application;
using MultiTenant.Infrastructure;
using MultiTenant.Infrastructure.Persistence;
using FluentValidation.AspNetCore;
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

builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHealthChecks().AddDbContextCheck<MasterDbContext>();
builder.Services.AddHealthChecks().AddDbContextCheck<TenantDbContext>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
builder.Services.AddFluentValidationAutoValidation().AddFluentValidationClientsideAdapters();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(a =>
{
    a.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MultiTenant API",
        Version = "v1",
    });

    //Enable authorization using JWT in Swagger
    a.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer eyUdksa7ak3b4kfnkJ9dsHDsldLs3nsDjla\""
    });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
