using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace MultiTenant.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DebugController : ControllerBase
{
    [HttpGet("claims")]
    public async Task<IActionResult> GetClaims()
    {
        var authHeader = Request.Headers["Authorization"].ToString();

        // Explicitly attempt to authenticate using the JWT Bearer scheme so we can inspect any failures
        var authResult = await HttpContext.AuthenticateAsync(JwtBearerDefaults.AuthenticationScheme);

        var a= authResult.Ticket.Principal.Claims.ToList();
        var b = authResult.Ticket.Principal.Identity.IsAuthenticated;
        var c = authResult.Ticket.Principal.Identity.AuthenticationType;

        var isAuthenticated = User?.Identity?.IsAuthenticated ?? false;
        var name = User?.Identity?.Name;
        var claims = User?.Claims.Select(c => new KeyValuePair<string, string>(c.Type, c.Value)).ToList() ?? new List<KeyValuePair<string, string>>();

        var resultInfo = new
        {
            Scheme = authResult?.Ticket?.AuthenticationScheme,
            Succeeded = authResult?.Succeeded,
            Failure = authResult?.Failure?.Message,
            HasTicket = authResult?.Ticket != null
        };

        return Ok(new { AuthorizationHeader = authHeader, IsAuthenticated = isAuthenticated, Name = name, Claims = claims, AuthenticateResult = resultInfo });
    }
}
