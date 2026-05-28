

namespace MultiTenant.Domain.Entities
{
    public class Employee
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public string EmailAddress { get; set; }
        public string UserId { get; set; }
    }
}
