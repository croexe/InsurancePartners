using Partners.Api.Authentication;
using Partners.Api.Constants;
using Partners.Core.DTOs.Requests;

namespace Partners.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/login", async (LoginRequest request, IAuthService authService) =>
        {
            var result = await authService.AuthenticateAsync(request.Email, request.Password);

            return result.Success
                ? Results.Ok(new { token = result.Value })
                : Results.Unauthorized();
        })
        .AllowAnonymous()
        .RequireRateLimiting(PolicyNames.LoginRateLimit)
        .Produces<object>()
        .ProducesProblem(StatusCodes.Status401Unauthorized);

        return app;
    }
}
