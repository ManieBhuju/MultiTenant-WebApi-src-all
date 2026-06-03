using MediatR;
using MultiTenant.Application.Common.Security;
using System.Reflection;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;


namespace MultiTenant.Application.Common.Behaviours;

public class AuthorizationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest,TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var authorizeAttributes = request.GetType().GetCustomAttributes<AuthorizeAttribute>();

        if (!authorizeAttributes.Any())
            return await next();

        // Resolve current user roles from HttpContext (if available)
        var serviceProvider = AppDomain.CurrentDomain.GetData("ServiceProvider") as IServiceProvider;
        var httpContext = serviceProvider?.GetService<IHttpContextAccessor>()?.HttpContext;

        if (httpContext == null)
            return await next();

        // Example: extract role claims (adjust claim type to your setup)
        var userRoles = httpContext.User?.Claims
            .Where(c => string.Equals(c.Type, "role", StringComparison.OrdinalIgnoreCase) || c.Type.EndsWith("role", StringComparison.OrdinalIgnoreCase))
            .Select(c => c.Value)
            .ToList() ?? new List<string>();

        // TODO: evaluate authorizeAttributes against userRoles and throw if unauthorized
        return await next();
    }
}
