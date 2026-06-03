
using Microsoft.AspNetCore.Identity;

namespace MultiTenant.Domain.Entities
{
    public class ApplicationUser : IdentityUser 
    {
        public string? TenantId { get; set; }
        public Tenant? Tenant { get; set; } 
    }
}
