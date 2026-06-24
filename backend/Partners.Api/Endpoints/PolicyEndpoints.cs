using Partners.Core.Contracts;
using Partners.Core.DTOs.Requests;
using Partners.Core.DTOs.Responses;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;

namespace Partners.Api.Endpoints;

public static class PolicyEndpoints
{
    public static IEndpointRouteBuilder MapPolicyEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/policies")
            .AddFluentValidationAutoValidation()
            .RequireAuthorization(policy => policy.RequireRole("PolicyManager"));

        group.MapPost("/", async (CreatePolicyRequest request, IPolicyService service) =>
        {
            var result = await service.CreatePolicyAsync(request);
            return result.Success
                ? Results.Created(string.Empty, result.Policy)
                : Results.BadRequest(new { errors = result.Errors });
        })
        .Produces<PolicyResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        return app;
    }
}
