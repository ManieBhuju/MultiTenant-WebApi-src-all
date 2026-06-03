

using Microsoft.AspNetCore.Http;
using System;
using System.Linq;

namespace MultiTenant.Infrastructure.MultiTenancy;

public class TenantResolver
{
    /// <summary>
    /// Resolve tenant identifier from the incoming HTTP context.
    /// Resolution order: X-Tenant-Id header, subdomain (tenant.example.com), TenantId claim.
    /// Returns null when tenant cannot be resolved.
    /// </summary>
    public string? Resolve(HttpContext? context)
    {
        if (context == null)
            return null;

        // Header
        if (context.Request.Headers.TryGetValue("X-Tenant-Id", out var headerValue))
        {
            var header = headerValue.ToString();
            if (!string.IsNullOrWhiteSpace(header))
                return header;
        }

        // Subdomain 
        var host = context.Request.Host.Host;
        if (!string.IsNullOrWhiteSpace(host))
        {
            var parts = host.Split('.');
            if (parts.Length >= 3)
            {
                var subdomain = parts[0];
                if (!string.IsNullOrWhiteSpace(subdomain))
                    return subdomain;
            }
        }

        // 3. Claim
        var claim = context.User?.Claims?.FirstOrDefault(c => string.Equals(c.Type, "tenantid", StringComparison.OrdinalIgnoreCase));
        if (claim != null && !string.IsNullOrWhiteSpace(claim.Value))
            return claim.Value;

        return null;
    }
}
