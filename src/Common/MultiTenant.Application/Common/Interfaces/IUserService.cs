


namespace MultiTenant.Application.Common.Interfaces;

public interface IUserService
{
    /// <summary>
    /// Returns the current authenticated user's Id, or null when no user is authenticated.
    /// </summary>
    Task<Guid?> GetCurrentUserIdAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the roles for the current authenticated user.
    /// </summary>
    Task<IEnumerable<string>> GetCurrentUserRolesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns true if the current authenticated user is in the specified role.
    /// </summary>
    Task<bool> IsInRoleAsync(string role, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns true if the current authenticated user is in any of the specified roles.
    /// </summary>
    Task<bool> IsInAnyRoleAsync(IEnumerable<string> roles, CancellationToken cancellationToken = default);
}
