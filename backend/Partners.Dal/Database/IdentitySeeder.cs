using Microsoft.AspNetCore.Identity;

namespace Partners.Dal.Database;

public static class IdentitySeeder
{
    public const string PolicyManagerRole = "PolicyManager";

    public static async Task SeedAsync(RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager, string defaultUserEmail, string defaultUserPassword)
    {
        if (!await roleManager.RoleExistsAsync(PolicyManagerRole))
        {
            await roleManager.CreateAsync(new IdentityRole(PolicyManagerRole));
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
            await userManager.AddToRoleAsync(user, PolicyManagerRole);
        }
    }
}
