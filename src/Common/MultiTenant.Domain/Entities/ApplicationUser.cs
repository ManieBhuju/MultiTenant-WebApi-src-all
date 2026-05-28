using Microsoft.AspNet.Identity.EntityFramework;

namespace MultiTenant.Domain.Entities
{
    public class ApplicationUser : IdentityUser 
    {
        public Guid? TenantId { get; set; }
        public Tenant? Tenant { get; set; } 
    }
}
