using MediatR;
using MultiTenant.Application.Common.Models;
using MultiTenant.Application.Features.Employees.DTOs;
using MultiTenant.Application.Common.Interfaces;
using MultiTenant.Domain.Entities;

namespace MultiTenant.Application.Features.Employees.Commands.CreateEmployee;

public record CreateEmployeeCommand(string FullName, string EmailAddress, string UserId) : IRequest<ServiceResult<EmployeeDto>>;

public class CreateEmployeeCommandHandler : IRequestHandler<CreateEmployeeCommand, ServiceResult<EmployeeDto>>
{
    private readonly ITenantDbContext _tenantDb;

    public CreateEmployeeCommandHandler(ITenantDbContext tenantDb)
    {
        _tenantDb = tenantDb;
    }

    public async Task<ServiceResult<EmployeeDto>> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var emp = new Employee
        {
            Id = Guid.NewGuid().ToString(),
            FullName = request.FullName,
            EmailAddress = request.EmailAddress,
            UserId = request.UserId,
            CreatedAt = DateTime.UtcNow
        };

        _tenantDb.Employees.Add(emp);
        await _tenantDb.SaveChangesAsync(cancellationToken);

        var dto = new EmployeeDto { Id = emp.Id, FullName = emp.FullName, EmailAddress = emp.EmailAddress, UserId = emp.UserId };
        return ServiceResult.Success(dto);
    }
}
