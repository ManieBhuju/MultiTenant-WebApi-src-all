using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiTenant.Application.Features.Users.Commands.CreateEmployee;
using MultiTenant.Application.Features.Users.Commands.DeleteEmployee;
using MultiTenant.Application.Features.Users.Commands.UpdateEmployee;
using MultiTenant.Application.Features.Users.Queries.GetEmployee;


namespace MultiTenant.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EmployeeController : BaseApiController
    {
        [HttpPost("register")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RegisterEmployee(CreateEmployeeCommand request, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(request, cancellationToken).ConfigureAwait(false);
            return FromServiceResult(result);
        }

        [HttpPut]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateEmployee(UpdateEmployeeCommand request, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(request, cancellationToken).ConfigureAwait(false);
            return FromServiceResult(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteEmployee(string id, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new DeleteEmployeeCommand(id), cancellationToken).ConfigureAwait(false);
            return FromServiceResult(result);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> GetEmployees(CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new GetEmployeesQuery(), cancellationToken).ConfigureAwait(false);
            return FromServiceResult(result);
        }
    }
}
