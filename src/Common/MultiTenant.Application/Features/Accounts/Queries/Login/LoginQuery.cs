using MediatR;
using MultiTenant.Application.Common.Models;
using MultiTenant.Application.DTOs;
using MultiTenant.Application.Common.Interfaces;
using MultiTenant.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace MultiTenant.Application.Features.Accounts.Queries.Login;

public record LoginQuery(string Email, string Password) : IRequest<ServiceResult<LoginResponse>>;

public class LoginQueryHandler : IRequestHandler<LoginQuery, ServiceResult<LoginResponse>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtService _jwtService;

    public LoginQueryHandler(UserManager<ApplicationUser> userManager, IJwtService jwtService)
    {
        _userManager = userManager;
        _jwtService = jwtService;
    }

    public async Task<ServiceResult<LoginResponse>> Handle(LoginQuery request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return ServiceResult.Failed<LoginResponse>(ServiceError.NotFound);

        var valid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!valid)
            return ServiceResult.Failed<LoginResponse>(ServiceError.CustomMessage("Invalid credentials"));

        var roles = await _userManager.GetRolesAsync(user);
        var token = await _jwtService.GenerateToken(user, roles);

        var response = new LoginResponse
        {
            Token = token,
            Email = user.Email ?? string.Empty,
            Role = roles.FirstOrDefault() ?? string.Empty,
            TenantId = user.TenantId ?? string.Empty
        };

        return ServiceResult.Success(response);
    }
}
