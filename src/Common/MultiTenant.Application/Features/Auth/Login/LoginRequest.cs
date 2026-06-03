using System;
using System.Collections.Generic;
using System.Text;

namespace MultiTenant.Application.Features.Auth.Login;

public record LoginRequest
(
    string Email,
        string Password
);

