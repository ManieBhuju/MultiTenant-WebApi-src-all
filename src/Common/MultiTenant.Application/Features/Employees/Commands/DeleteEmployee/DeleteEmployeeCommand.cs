using MediatR;
using MultiTenant.Application.Common.Models;
using MultiTenant.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using MultiTenant.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MultiTenant.Application.Features.Users.Commands.DeleteEmployee;

public record DeleteEmployeeCommand(string Id) : IRequest<ServiceResult<bool>>;

public class DeleteEmployeeCommandHandler : IRequestHandler<DeleteEmployeeCommand, ServiceResult<bool>>
{
    private readonly UserManager<ApplicationUser> _userManager;

    private readonly ITenantDbContext _tenantDb;
    private readonly ITenantProvider _tenantProvider;

    public DeleteEmployeeCommandHandler(UserManager<ApplicationUser> userManager, ITenantDbContext tenantDb, ITenantProvider tenantProvider)
    {
        _userManager = userManager;
        _tenantDb = tenantDb;
        _tenantProvider = tenantProvider;
    }

    public async Task<ServiceResult<bool>> Handle(DeleteEmployeeCommand request, CancellationToken cancellationToken)
    {
        var emp = await _tenantDb.Employees.FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);
        if (emp == null)
            return ServiceResult.Failed<bool>(ServiceError.NotFound);

        try
        {
            _tenantDb.Employees.Remove(emp);
            await _tenantDb.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            return ServiceResult.Failed<bool>(new ServiceError("DeleteEmployeeFailed", ex.Message));
        }

        if (!string.IsNullOrWhiteSpace(emp.UserId))
        {
            var user = await _userManager.FindByIdAsync(emp.UserId);
            if (user != null)
            {
                var res = await _userManager.DeleteAsync(user);
                if (!res.Succeeded)
                    return ServiceResult.Failed<bool>(new ServiceError("DeleteUserFailed", string.Join(";", res.Errors.Select(e => e.Description))));
            }
        }

        return ServiceResult.Success(true);
    }
}
