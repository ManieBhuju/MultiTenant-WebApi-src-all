using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MultiTenant.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin")]
    public class TenantsController : ControllerBase
    {
        // SuperAdmin only endpoints
        [HttpGet]
        public IActionResult GetAll() => Ok("List tenants - SuperAdmin only");

        [HttpPost]
        public IActionResult Create() => Ok("Create tenant - SuperAdmin only");

        [HttpPut("{id}")]
        public IActionResult Update(string id) => Ok($"Update tenant {id} - SuperAdmin only");

        [HttpDelete("{id}")]
        public IActionResult Delete(string id) => Ok($"Delete tenant {id} - SuperAdmin only");
    }
}
