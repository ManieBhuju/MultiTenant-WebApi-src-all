using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Linq;

// TenantResolver is registered in DI; middleware should receive it via constructor injection

namespace MultiTenant.Infrastructure.MultiTenancy;

public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantResolutionMiddleware> _logger;
    private readonly TenantResolver _resolver;

    public TenantResolutionMiddleware(RequestDelegate next, ILogger<TenantResolutionMiddleware> logger, TenantResolver resolver)
    {
        _next = next;
        _logger = logger;
        _resolver = resolver;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Attempt to resolve tenant from header, host subdomain or authenticated claims
        var tenantId = _resolver.Resolve(context);
        if (!string.IsNullOrEmpty(tenantId))
        {
            // Store resolved tenant in items for downstream access
            context.Items["TenantId"] = tenantId;
            _logger.LogDebug("Resolved tenant {TenantId} for request {Path}", tenantId, context.Request.Path);
        }
        else
        {
            // Log additional diagnostics when user is present but tenant not found
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                var roles = context.User.Claims.Where(c => c.Type == System.Security.Claims.ClaimTypes.Role).Select(c => c.Value);
                _logger.LogDebug("Authenticated user present but no tenant resolved. User: {User}, Roles: {Roles}", context.User.Identity?.Name, string.Join(',', roles));
            }
            else
            {
                _logger.LogDebug("No tenant resolved and no authenticated user for request {Path}", context.Request.Path);
            }
        }

        await _next(context);
    }
}
