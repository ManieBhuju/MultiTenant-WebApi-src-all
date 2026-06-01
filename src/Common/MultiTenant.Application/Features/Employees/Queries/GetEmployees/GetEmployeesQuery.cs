using MediatR;
using MultiTenant.Application.Common.Models;
using MultiTenant.Application.Features.Employees.DTOs;
using Microsoft.EntityFrameworkCore;
using MultiTenant.Application.Common.Interfaces;

namespace MultiTenant.Application.Features.Employees.Queries.GetEmployees;

public record GetEmployeesQuery() : IRequest<ServiceResult<IEnumerable<EmployeeDto>>>;

public class GetEmployeesQueryHandler : IRequestHandler<GetEmployeesQuery, ServiceResult<IEnumerable<EmployeeDto>>>
{
    private readonly ITenantDbContext _tenantDb;

    public GetEmployeesQueryHandler(ITenantDbContext tenantDb)
    {
        _tenantDb = tenantDb;
    }

    public async Task<ServiceResult<IEnumerable<EmployeeDto>>> Handle(GetEmployeesQuery request, CancellationToken cancellationToken)
    {
        var emps = await _tenantDb.Employees.AsNoTracking()
            .Select(e => new EmployeeDto { Id = e.Id, FullName = e.FullName, EmailAddress = e.EmailAddress, UserId = e.UserId })
            .ToListAsync(cancellationToken);

        return ServiceResult.Success<IEnumerable<EmployeeDto>>(emps);
    }
}
