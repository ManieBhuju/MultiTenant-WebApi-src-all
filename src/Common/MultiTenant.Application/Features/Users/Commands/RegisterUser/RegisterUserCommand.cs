using MediatR;
using MultiTenant.Application.Common.Models;
using MultiTenant.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace MultiTenant.Application.Features.Users.Commands.RegisterUser;

public record RegisterUserCommand(string Email, string Password, string Role) : IRequest<ServiceResult<bool>>;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, ServiceResult<bool>>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public RegisterUserCommandHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<ServiceResult<bool>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return ServiceResult.Failed<bool>(new ServiceError("CreateUserFailed", string.Join(";", result.Errors.Select(e => e.Description))));

        await _userManager.AddToRoleAsync(user, request.Role);
        return ServiceResult.Success(true);
    }
}
