

using Microsoft.AspNetCore.Http;
using MultiTenant.Application.Common.Interfaces;

namespace MultiTenant.Infrastructure.MultiTenancy;

public class TenantProvider : ITenantProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    public string GetTenantId()
    {
        return _httpContextAccessor.HttpContext?.User?.FindFirst("tenantId")?.Value
               ?? throw new InvalidOperationException("Tenant ID claim not found.");
    }
}
