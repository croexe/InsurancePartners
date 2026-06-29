using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Partners.Core.Contracts;
using System.Net.Http.Json;

namespace Partners.Api.Tests.Infrastructure;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public Mock<IPartnerService> PartnerServiceMock { get; } = new();
    public Mock<IPolicyService> PolicyServiceMock { get; } = new();
    public Mock<IPartnerNotifier> PartnerNotifierMock { get; } = new();

    // Visok limit u testovima da visestruki pozivi ne udare u 429.
    // Testovi za 429 nadjacavaju ova svojstva (jedini izvor vrijednosti — bez sukoba config izvora).
    protected virtual int LoginRateLimitPermits => 1000;
    protected virtual int GlobalRateLimitPermits => 10000;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Koristimo LocalDB s testnom bazom umjesto produkcijske — MigrateAsync() radi normalno,
        // produkcijski kod ne zna da postoje testovi.
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:InsurancePartnersDb"] =
                    "Server=(localdb)\\MSSQLLocalDB;Database=WienerPartnersTest;Trusted_Connection=True;TrustServerCertificate=True;",
                ["RateLimiting:Login:PermitLimit"] = LoginRateLimitPermits.ToString(),
                ["RateLimiting:Login:WindowSeconds"] = "60",
                ["RateLimiting:Global:PermitLimit"] = GlobalRateLimitPermits.ToString(),
                ["RateLimiting:Global:WindowSeconds"] = "60"
            });
        });

        builder.ConfigureServices(services =>
        {
            var partnerService = services.SingleOrDefault(d => d.ServiceType == typeof(IPartnerService));
            if (partnerService != null) services.Remove(partnerService);
            services.AddScoped<IPartnerService>(_ => PartnerServiceMock.Object);

            var policyService = services.SingleOrDefault(d => d.ServiceType == typeof(IPolicyService));
            if (policyService != null) services.Remove(policyService);
            services.AddScoped<IPolicyService>(_ => PolicyServiceMock.Object);

            var partnerNotifier = services.SingleOrDefault(d => d.ServiceType == typeof(IPartnerNotifier));
            if (partnerNotifier != null) services.Remove(partnerNotifier);
            services.AddScoped<IPartnerNotifier>(_ => PartnerNotifierMock.Object);
        });

        builder.UseEnvironment("Development");
    }

    public async Task<string> GetValidTokenAsync()
    {
        using var scope = Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        if (!await roleManager.RoleExistsAsync("PolicyManager"))
            await roleManager.CreateAsync(new IdentityRole("PolicyManager"));

        var existing = await userManager.FindByEmailAsync("test@wiener.hr");
        if (existing is null)
        {
            var user = new IdentityUser { UserName = "test@wiener.hr", Email = "test@wiener.hr", EmailConfirmed = true };
            await userManager.CreateAsync(user, "Test123!");
            await userManager.AddToRoleAsync(user, "PolicyManager");
        }

        var client = CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new { email = "test@wiener.hr", password = "Test123!" });
        var result = await response.Content.ReadFromJsonAsync<TokenResponse>();
        return result!.Token;
    }
}

public record TokenResponse(string Token);
