using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Partners.Api.Authentication;
using System.Security.Claims;

namespace Partners.Api.Tests.Authentication;

public class JwtTokenGeneratorTests
{
    private const string Secret = "unit-test-secret-key-with-min-32-characters!";

    private static JwtTokenGenerator CreateGenerator(DateTimeOffset now, int expiryHours = 8)
    {
        var options = Options.Create(new JwtOptions
        {
            Secret = Secret,
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ExpiryHours = expiryHours
        });

        return new JwtTokenGenerator(options, new FixedTimeProvider(now));
    }

    [Fact]
    public void GenerateToken_SetsExpiryFromTimeProvider()
    {
        var now = new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);
        var generator = CreateGenerator(now, expiryHours: 8);
        var user = new IdentityUser { Id = "user-123", Email = "test@wiener.hr" };

        var token = generator.GenerateToken(user, ["PolicyManager"]);

        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(token);
        jwt.ValidTo.Should().Be(now.UtcDateTime.AddHours(8));
        jwt.ValidFrom.Should().Be(now.UtcDateTime);
    }

    [Fact]
    public void GenerateToken_IncludesUserAndRoleClaims()
    {
        var now = new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);
        var generator = CreateGenerator(now);
        var user = new IdentityUser { Id = "user-123", Email = "test@wiener.hr" };

        var token = generator.GenerateToken(user, ["PolicyManager", "Admin"]);

        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(token);
        jwt.Subject.Should().Be("user-123");
        jwt.Issuer.Should().Be("TestIssuer");
        jwt.Audiences.Should().Contain("TestAudience");
        jwt.GetClaim(JwtRegisteredClaimNames.Email).Value.Should().Be("test@wiener.hr");
        jwt.Claims
            .Where(claim => claim.Type == ClaimTypes.Role)
            .Select(claim => claim.Value)
            .Should().BeEquivalentTo("PolicyManager", "Admin");
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
