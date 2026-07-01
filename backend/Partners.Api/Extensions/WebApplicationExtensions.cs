using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Partners.Api.Constants;
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

        // UseRouting mora doci prije UseRateLimiter da endpoint-specifican limiter
        // (RequireRateLimiting) vidi endpoint metadata.
        app.UseRouting();

        app.UseCors(PolicyNames.Cors);

        app.UseRateLimiter();

        app.UseRequestTimeouts();

        app.UseAuthentication();
        app.UseAuthorization();

        // Poslije autorizacije — neautoriziran zahtjev dobije 401 prije nego dode do kesa.
        app.UseOutputCache();

        app.MapAuthEndpoints();
        app.MapPartnerEndpoints();
        app.MapPolicyEndpoints();

        // Hub je dugotrajna veza — iskljucujemo ga iz request timeouta da ne prekida SignalR.
        app.MapHub<PartnerHub>("/hubs/partners").DisableRequestTimeout();

        return app;
    }

    public static async Task ApplyWienMigrationsAndSeedAsync(this WebApplication app)
    {
        // Auto-migracija i seed se izvrsavaju samo u developmentu. U produkciji
        // se migracije primjenjuju kao zaseban deploy korak (izbjegava race condition
        // kod vise instanci), a korisnici se provizioniraju zasebno.
        if (!app.Environment.IsDevelopment())
        {
            return;
        }

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
