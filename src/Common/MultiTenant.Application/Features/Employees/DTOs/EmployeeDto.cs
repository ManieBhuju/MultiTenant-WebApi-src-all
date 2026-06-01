namespace MultiTenant.Application.Features.Employees.DTOs;

public class EmployeeDto
{
    public string Id { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string EmailAddress { get; set; } = default!;
    public string UserId { get; set; } = default!;
}
