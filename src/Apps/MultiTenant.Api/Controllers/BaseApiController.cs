using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MultiTenant.Application.Common.Models;

namespace MultiTenant.Api.Controllers;


[Route("api/[controller]")]
[ApiController]
public abstract class BaseApiController : ControllerBase
{
    private ISender _mediator;

    protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetService<ISender>()!;

    protected IActionResult FromServiceResult<T>(ServiceResult<T> result)
    {
        if (result == null)
            return NoContent();

        if (result.Succeeded)
        {
            return Ok(new { success = true, data = result.Data });
        }

        var errors = result.Errors?.ToArray() ?? Array.Empty<ServiceError>();
        // Map common error codes to HTTP status codes
        if (errors.Any(e => e.Code == "NotFound"))
            return NotFound(new { success = false, errors });

        return BadRequest(new { success = false, errors });
    }
}
