

using Microsoft.AspNetCore.Http;
using MultiTenant.Application.Common.Interfaces;
using System;
using System.Linq;

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
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
            throw new InvalidOperationException("No HttpContext available.");

        // First check if middleware resolved a tenant and stored it in HttpContext.Items
        if (context.Items.TryGetValue("TenantId", out var tenantObj) && tenantObj is string tenantFromItem && !string.IsNullOrEmpty(tenantFromItem))
            return tenantFromItem;

        var user = context.User;
        var claim = user?.Claims?.FirstOrDefault(c => string.Equals(c.Type, "TenantId", StringComparison.OrdinalIgnoreCase));
        if (claim != null && !string.IsNullOrEmpty(claim.Value))
            return claim.Value;

        throw new InvalidOperationException("Tenant ID not found in context or claims.");
    }
}
