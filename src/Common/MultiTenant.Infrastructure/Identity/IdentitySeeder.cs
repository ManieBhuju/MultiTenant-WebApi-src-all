

using Microsoft.AspNetCore.Identity;
using MultiTenant.Domain.Constant;
using MultiTenant.Domain.Entities;

namespace MultiTenant.Infrastructure.Identity;

public static class IdentitySeeder
{
    public static async Task SeedAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        // Seed roles
        await SeedRoles(roleManager);
        await SeedSuperAdmin(userManager);
    }

    private static async Task SeedRoles(RoleManager<IdentityRole> roleManager)
    {
        var roles = new[]
        {
            Roles.SuperAdmin, 
            Roles.Admin, 
            Roles.Employee
        };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    private static async Task SeedSuperAdmin(UserManager<ApplicationUser> userManager)
    {
        var email = "assessment@yopmail.com";

        var user = await userManager.FindByEmailAsync(email);

        if (user != null)
            return;

        user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            Tenant = null
        };

        var result = await userManager.CreateAsync(user, "Tester@123");

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, Roles.SuperAdmin);
        }
        else
        {
            //Handling Error
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            Console.WriteLine($"Failed to create SuperAdmin user: {errors}");
            throw new Exception(errors);
        }
    }
}
