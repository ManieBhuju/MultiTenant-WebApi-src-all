using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace MultiTenant.Infrastructure.MultiTenancy;

public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantResolutionMiddleware> _logger;

    public TenantResolutionMiddleware(RequestDelegate next, ILogger<TenantResolutionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var resolver = new TenantResolver();
        var tenantId = resolver.Resolve(context);
        if (!string.IsNullOrEmpty(tenantId))
        {
            // Store resolved tenant in items for downstream access
            context.Items["TenantId"] = tenantId;
            _logger.LogDebug("Resolved tenant {TenantId} for request {Path}", tenantId, context.Request.Path);
        }
        else
        {
            _logger.LogDebug("No tenant resolved for request {Path}", context.Request.Path);
        }

        await _next(context);
    }
}
