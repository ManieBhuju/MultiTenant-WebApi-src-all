using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace MultiTenant.Application.Common.Security;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]

public class AuthorizeAttribute : Attribute
{
    //  <summary>
    //  Initializes new Instance of AuthorizeAttribute class
    public AuthorizeAttribute() { }

    //  <summary>
    //  Get or Sets a comma delimited list of roles that are allowed to access the resource.
    public string Roles { get; set; }
}
