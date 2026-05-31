using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using MultiTenant.Application.Common.Interfaces;


namespace MultiTenant.Application.Common.Services;

public class UserService : IUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Task<Guid?> GetCurrentUserIdAsync(CancellationToken cancellationToken = default)
    {
        var idStr = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(idStr, out var guid))
            return Task.FromResult<Guid?>(guid);

        return Task.FromResult<Guid?>(null);
    }

    public Task<IEnumerable<string>> GetCurrentUserRolesAsync(CancellationToken cancellationToken = default)
    {
        var roles = _httpContextAccessor.HttpContext?.User?.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value) ?? Enumerable.Empty<string>();

        return Task.FromResult(roles);
    }

    public Task<bool> IsInRoleAsync(string role, CancellationToken cancellationToken = default)
    {
        var isInRole = _httpContextAccessor.HttpContext?.User?.IsInRole(role) ?? false;
        return Task.FromResult(isInRole);
    }

    public Task<bool> IsInAnyRoleAsync(IEnumerable<string> roles, CancellationToken cancellationToken = default)
    {
        var userRoles = _httpContextAccessor.HttpContext?.User?.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value) ?? Enumerable.Empty<string>();

        var result = roles != null && userRoles.Intersect(roles).Any();
        return Task.FromResult(result);
    }
}
