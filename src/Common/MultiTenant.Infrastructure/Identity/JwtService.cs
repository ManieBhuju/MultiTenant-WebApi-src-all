

using MultiTenant.Application.Common.Interfaces;
using MultiTenant.Domain.Entities;

namespace MultiTenant.Infrastructure.Identity;

public class JwtService : IJwtService
{
    public string GenerateToken(ApplicationUser user, IList<string> roles)
    {
        throw new NotImplementedException();
    }
}
