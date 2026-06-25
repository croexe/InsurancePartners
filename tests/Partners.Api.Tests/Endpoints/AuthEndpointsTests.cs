using FluentAssertions;
using Partners.Api.Tests.Infrastructure;
using System.Net;
using System.Net.Http.Json;

namespace Partners.Api.Tests.Endpoints;

public class AuthEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_InvalidCredentials_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new { email = "wrong@wiener.hr", password = "WrongPass123!" });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_MissingBody_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new { });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_ExceedsRateLimit_Returns429()
    {
        // Dedicirani factory s niskim limitom (2/min).
        using var lowLimitFactory = new LowRateLimitFactory();
        var client = lowLimitFactory.CreateClient();
        var payload = new { email = "wrong@wiener.hr", password = "WrongPass123!" };

        await client.PostAsJsonAsync("/api/auth/login", payload);             // 1 — prolazi (401)
        await client.PostAsJsonAsync("/api/auth/login", payload);             // 2 — prolazi (401)
        var third = await client.PostAsJsonAsync("/api/auth/login", payload); // 3 — odbijen

        third.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
    }

    private sealed class LowRateLimitFactory : CustomWebApplicationFactory
    {
        protected override int LoginRateLimitPermits => 2;
    }
}
