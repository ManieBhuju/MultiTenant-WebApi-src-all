using MediatR;
using MultiTenant.Application.Common.Models;
using MultiTenant.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using MultiTenant.Application.Common.Interfaces;

namespace MultiTenant.Application.Features.Users.Commands.CreateEmployee;

public record CreateEmployeeCommand(string Email, string Password) : IRequest<ServiceResult<bool>>;

public class CreateEmployeeCommandHandler : IRequestHandler<CreateEmployeeCommand, ServiceResult<bool>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITenantDbContext _tenantDb;
    private readonly ITenantProvider _tenantProvider;

    public CreateEmployeeCommandHandler(UserManager<ApplicationUser> userManager, ITenantDbContext tenantDb, ITenantProvider tenantProvider)
    {
        _userManager = userManager;
        _tenantDb = tenantDb;
        _tenantProvider = tenantProvider;
    }

    public async Task<ServiceResult<bool>> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId();

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            EmailConfirmed = true,
            TenantId = tenantId
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return ServiceResult.Failed<bool>(new ServiceError("CreateUserFailed", string.Join(";", result.Errors.Select(e => e.Description))));

        await _userManager.AddToRoleAsync(user, "Employee");

        // Create corresponding employee record in tenant DB
        try
        {
            var emp = new Employee
            {
                Id = Guid.NewGuid().ToString(),
                FullName = request.Email,
                EmailAddress = request.Email,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow
            };
            _tenantDb.Employees.Add(emp);
            await _tenantDb.SaveChangesAsync(cancellationToken);
        }
        catch (Exception)
        {
            // rollback user creation if tenant DB insert fails
            await _userManager.DeleteAsync(user);
            return ServiceResult.Failed<bool>(new ServiceError("CreateEmployeeFailed", "Failed to create employee record in tenant database."));
        }

        return ServiceResult.Success(true);
    }
}
