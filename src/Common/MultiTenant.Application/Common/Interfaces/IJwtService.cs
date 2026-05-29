
using MultiTenant.Domain.Entities;

namespace MultiTenant.Application.Common.Interfaces;

public interface IJwtService
{
    string GenerateToken(ApplicationUser user, IList<string> roles);
}
