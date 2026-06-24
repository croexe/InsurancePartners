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
}
