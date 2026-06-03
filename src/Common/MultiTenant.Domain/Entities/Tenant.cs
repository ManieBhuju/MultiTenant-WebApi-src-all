

using MultiTenant.Domain.Common;

namespace MultiTenant.Domain.Entities
{
    public class Tenant : BaseEntity
    {
        public string Name { get; set; } = default!;
        public string EmailAddress { get; set; } = default!;
        public string TenantId { get; set; } = default!;
        public string DbConnStr { get; set; } = default!;
    }
}
