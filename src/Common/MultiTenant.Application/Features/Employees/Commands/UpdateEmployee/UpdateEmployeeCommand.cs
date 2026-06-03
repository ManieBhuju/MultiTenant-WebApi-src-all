using MediatR;
using Microsoft.AspNetCore.Identity;
using MultiTenant.Application.Common.Models;
using MultiTenant.Domain.Entities;
using MultiTenant.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MultiTenant.Application.Features.Users.Commands.UpdateEmployee;

public record UpdateEmployeeCommand(string Id, string? Email, string? Password) : IRequest<ServiceResult<bool>>;

public class UpdateEmployeeCommandHandler : IRequestHandler<UpdateEmployeeCommand, ServiceResult<bool>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITenantDbContext _tenantDb;

    public UpdateEmployeeCommandHandler(UserManager<ApplicationUser> userManager, ITenantDbContext tenantDb)
    {
        _userManager = userManager;
        _tenantDb = tenantDb;
    }

    public async Task<ServiceResult<bool>> Handle(UpdateEmployeeCommand request, CancellationToken cancellationToken)
    {
        // Here request.Id refers to the Employee.Id in the tenant DB
        var emp = await _tenantDb.Employees.FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);
        if (emp == null)
            return ServiceResult.Failed<bool>(ServiceError.NotFound);

        var changed = false;
        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            emp.EmailAddress = request.Email;
            emp.FullName = request.Email;
            changed = true;
        }

        if (changed)
        {
            emp.ModifiedAt = DateTime.UtcNow;
            try
            {
                await _tenantDb.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                return ServiceResult.Failed<bool>(new ServiceError("UpdateEmployeeFailed", ex.Message));
            }
        }

        if (!string.IsNullOrWhiteSpace(emp.UserId))
        {
            var user = await _userManager.FindByIdAsync(emp.UserId);
            if (user != null)
            {
                if (!string.IsNullOrWhiteSpace(request.Email))
                {
                    user.Email = request.Email;
                    user.UserName = request.Email;
                    user.EmailConfirmed = true;
                    var updateRes = await _userManager.UpdateAsync(user);
                    if (!updateRes.Succeeded)
                        return ServiceResult.Failed<bool>(new ServiceError("UpdateUserFailed", string.Join(";", updateRes.Errors.Select(e => e.Description))));
                }

                if (!string.IsNullOrWhiteSpace(request.Password))
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var res = await _userManager.ResetPasswordAsync(user, token, request.Password);
                    if (!res.Succeeded)
                        return ServiceResult.Failed<bool>(new ServiceError("UpdatePasswordFailed", string.Join(";", res.Errors.Select(e => e.Description))));
                }
            }
        }

        return ServiceResult.Success(true);
    }
}
