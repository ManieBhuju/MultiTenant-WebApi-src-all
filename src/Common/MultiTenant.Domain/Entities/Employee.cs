

using MultiTenant.Domain.Common;

namespace MultiTenant.Domain.Entities
{
    public class Employee : BaseEntity
    {
        public string FullName { get; set; } = default!;
        public string EmailAddress { get; set; } = default!;
        public string UserId { get; set; } = default!;
    }
}
