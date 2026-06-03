using MediatR;
using MultiTenant.Application.Common.Models;
using MultiTenant.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using MultiTenant.Application.DTOs;

namespace MultiTenant.Application.Features.Users.Queries.GetEmployee;

public record GetEmployeesQuery() : IRequest<ServiceResult<List<EmployeeDto>>>;

public class GetEmployeeQueryHandler : IRequestHandler<GetEmployeesQuery, ServiceResult<List<EmployeeDto>>>
{
    private readonly ITenantDbContext _tenantDb;

    public GetEmployeeQueryHandler(ITenantDbContext tenantDb)
    {
        _tenantDb = tenantDb;
    }

    public async Task<ServiceResult<List<EmployeeDto>>> Handle(GetEmployeesQuery request, CancellationToken cancellationToken)
    {
        var emps = await _tenantDb.Employees.AsNoTracking()
            .Select(e => new EmployeeDto { Id = e.Id, FullName = e.FullName, EmailAddress = e.EmailAddress, UserId = e.UserId })
            .ToListAsync(cancellationToken);

        return ServiceResult.Success(emps);
    }
}
