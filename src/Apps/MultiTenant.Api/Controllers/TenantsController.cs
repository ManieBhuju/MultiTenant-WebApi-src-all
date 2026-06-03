using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiTenant.Application.Features.Tenants.Commands.CreateTenant;
using MultiTenant.Application.Features.Tenants.Commands.UpdateTenant;
using MultiTenant.Application.Features.Tenants.Commands.DeleteTenant;
using MultiTenant.Application.Features.Tenants.Queries.GetTenants;

namespace MultiTenant.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin")]
    public class TenantsController : BaseApiController
    {
        // SuperAdmin only endpoints

        [HttpGet("TenantList")]
        public async Task<IActionResult> GetTenantList(CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new GetTenantsListQuery(), cancellationToken).ConfigureAwait(false);
            if (result.Succeeded)
                return Ok(result.Data);

            return BadRequest(result.Errors);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTenant(CreateTenantCommand request, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(request, cancellationToken).ConfigureAwait(false);
            if (result.Succeeded)
                return Ok(result.Data);

            return BadRequest(result.Errors);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateTenant(UpdateTenantCommand request, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(request, cancellationToken).ConfigureAwait(false);
            if (result.Succeeded)
                return Ok(result.Data);

            return BadRequest(result.Errors);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTenant(string id, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new DeleteTenantCommand(id), cancellationToken).ConfigureAwait(false);
            if (result.Succeeded)
                return Ok(result.Data);

            return BadRequest(result.Errors);
        }
    }
}
