using System;
using System.Collections.Generic;
using System.Text;

namespace MultiTenant.Application.DTOs;

public class LoginResponse
{
    public string Token { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
    public string TenantId { get; set; }
}
