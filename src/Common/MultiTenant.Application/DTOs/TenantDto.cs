namespace MultiTenant.Application.DTOs;

public class TenantDto
{
    public string Id { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string EmailAddress { get; set; } = default!;
    public string TenantId { get; set; } = default!;
    public string DbConnStr { get; set; } = default!;
}
