using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MultiTenant.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        // Admins within a tenant can create employees
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Create() => Ok("Create employee - Admin only");

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Update(string id) => Ok($"Update employee {id} - Admin only");

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(string id) => Ok($"Delete employee {id} - Admin only");

        // Employee can see their own record
        [HttpGet("me")]
        [Authorize(Roles = "Employee,Admin,SuperAdmin")]
        public IActionResult GetMe()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Ok(new { Id = userId, Message = "Employee self data (mock)" });
        }

        // Admin and SuperAdmin can list employees for a tenant
        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public IActionResult GetAll() => Ok("List employees for tenant - Admin or SuperAdmin");
    }
}
