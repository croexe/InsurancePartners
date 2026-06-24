using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Partners.Api.Endpoints;
using Partners.Api.Hubs;
using Partners.Dal.Database;
using Serilog;

namespace Partners.Api.Extensions;

internal static class WebApplicationExtensions
{
    public static WebApplication ApplyWienWebApplicationConfiguration(this WebApplication app)
    {
        app.UseSerilogRequestLogging();

        app.UseExceptionHandler();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        app.UseCors("AllowFrontend");

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapAuthEndpoints();
        app.MapPartnerEndpoints();
        app.MapPolicyEndpoints();

        app.MapHub<PartnerHub>("/hubs/partners");

        return app;
    }

    public static async Task ApplyWienMigrationsAndSeedAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        await IdentitySeeder.SeedAsync(
            roleManager,
            userManager,
            config["Identity:DefaultUser:Email"]!,
            config["Identity:DefaultUser:Password"]!);
    }
}
