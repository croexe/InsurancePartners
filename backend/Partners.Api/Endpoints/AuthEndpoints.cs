using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Partners.Core.DTOs.Requests;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Partners.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/login", async (
            LoginRequest request,
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IConfiguration config) =>
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
                return Results.Unauthorized();

            var user = await userManager.FindByEmailAsync(request.Email);
            if (user is null)
            {
                return Results.Unauthorized();
            }

            // Broji neuspjele pokusaje i zakljucava racun (lockoutOnFailure: true).
            var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
            if (!result.Succeeded)
            {
                return Results.Unauthorized();
            }

            var roles = await userManager.GetRolesAsync(user);
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id),
                new(JwtRegisteredClaimNames.Email, user.Email!),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Secret"]!));
            var token = new JwtSecurityToken(
                issuer: config["Jwt:Issuer"],
                audience: config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

            return Results.Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
        })
        .AllowAnonymous()
        .RequireRateLimiting("login")
        .Produces<object>()
        .ProducesProblem(StatusCodes.Status401Unauthorized);

        return app;
    }
}
