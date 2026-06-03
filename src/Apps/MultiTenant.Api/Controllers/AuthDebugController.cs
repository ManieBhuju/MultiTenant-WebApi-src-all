using Microsoft.AspNetCore.Mvc;
using MultiTenant.Infrastructure.Services;
using System.IdentityModel.Tokens.Jwt;

namespace MultiTenant.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthDebugController : ControllerBase
{
    private readonly IJwtLogStore _logStore;

    public AuthDebugController(IJwtLogStore logStore)
    {
        _logStore = logStore;
    }

    [HttpGet("authinfo")]
    public IActionResult GetAuthInfo()
    {
        var authHeader = Request.Headers["Authorization"].ToString();
        string? token = null;
        object? payload = null;
        if (!string.IsNullOrWhiteSpace(authHeader))
        {
            token = authHeader.StartsWith("Bearer ") ? authHeader.Substring(7) : authHeader;
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);
                payload = jwt.Payload;
            }
            catch (Exception ex)
            {
                payload = new { error = ex.Message };
            }
        }

        var logs = _logStore.GetEntries();
        return Ok(new { AuthorizationHeader = authHeader, Token = token, Payload = payload, JwtLogs = logs });
    }
}
