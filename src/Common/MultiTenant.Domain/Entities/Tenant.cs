

namespace MultiTenant.Domain.Entities
{
    public class Tenant
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string EmailAddress { get; set; }
        public string TenantId { get; set; }
        public string DbConsStr { get; set; }
    }
}
