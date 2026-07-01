using Microsoft.AspNetCore.Identity;
using Partners.Core.Constants;

namespace Partners.Dal.Database;

public static class IdentitySeeder
{
    public static async Task SeedAsync(RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager, string defaultUserEmail, string defaultUserPassword)
    {
        if (!await roleManager.RoleExistsAsync(Roles.PolicyManager))
        {
            await roleManager.CreateAsync(new IdentityRole(Roles.PolicyManager));
        }

        var existing = await userManager.FindByEmailAsync(defaultUserEmail);
        if (existing is null)
        {
            var user = new IdentityUser
            {
                UserName = defaultUserEmail,
                Email = defaultUserEmail,
                EmailConfirmed = true
            };

            await userManager.CreateAsync(user, defaultUserPassword);
            await userManager.AddToRoleAsync(user, Roles.PolicyManager);
        }
    }
}
