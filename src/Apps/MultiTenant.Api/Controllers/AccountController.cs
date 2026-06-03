using Microsoft.AspNetCore.Mvc;
using MultiTenant.Application.DTOs;
using MultiTenant.Application.Features.Accounts.Queries.Login;

namespace MultiTenant.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountController : BaseApiController
{
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginQuery request, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(request, cancellationToken);
        if (result.Succeeded)
            return Ok(result.Data);

        return BadRequest(result.Errors);
    }
}
