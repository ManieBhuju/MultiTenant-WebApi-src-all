using MediatR;
using MultiTenant.Application.Common.Security;
using System.Reflection;


namespace MultiTenant.Application.Common.Behaviours;

public class AuthorizationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest,TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var authorizeAttributes = request.GetType().GetCustomAttributes<AuthorizeAttribute>();

        if (!authorizeAttributes.Any())
            return await next();

        // Resolve current user roles from HttpContext
        var httpContext = (IServiceProvider)AppDomain.CurrentDomain.GetData("ServiceProvider");
        return await next();
    }
}
