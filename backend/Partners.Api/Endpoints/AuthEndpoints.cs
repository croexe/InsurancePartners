using Microsoft.AspNetCore.Identity;
using Partners.Api.Authentication;
using Partners.Core.DTOs.Requests;

namespace Partners.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/login", async (
            LoginRequest request,
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IJwtTokenGenerator tokenGenerator) =>
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
            var token = tokenGenerator.GenerateToken(user, roles);

            return Results.Ok(new { token });
        })
        .AllowAnonymous()
        .RequireRateLimiting("login")
        .Produces<object>()
        .ProducesProblem(StatusCodes.Status401Unauthorized);

        return app;
    }
}
