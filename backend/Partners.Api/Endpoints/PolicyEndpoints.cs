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
            .AddFluentValidationAutoValidation();

        group.MapPost("/", async (CreatePolicyRequest request, IPolicyService service) =>
        {
            var result = await service.CreateAsync(request);
            return result.Success
                ? Results.Created(string.Empty, result.Policy)
                : Results.BadRequest(new { errors = result.Errors });
        })
        .Produces<PolicyResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        return app;
    }
}
