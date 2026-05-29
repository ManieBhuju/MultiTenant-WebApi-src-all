
using FluentValidation;
using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace MultiTenant.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {

        services.AddSingleton(GetConfigureMappingConfig);
        services.AddScoped<IMapper, Mapper>();

        var assembly = Assembly.GetExecutingAssembly();

        // MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

        // FluentValidation
        services.AddValidatorsFromAssembly(assembly);

        // Pipeline Behaviors
        //services.AddTransient(
        //    typeof(IPipelineBehavior<,>),
        //    typeof(ValidationBehavior<,>));

        return services;
    }

    private static TypeAdapterConfig GetConfigureMappingConfig()
    {
        var config = TypeAdapterConfig.GlobalSettings;
        IList<IRegister> registers = config.Scan(Assembly.GetExecutingAssembly());
        config.Apply(registers);
        return config;
    }
}
