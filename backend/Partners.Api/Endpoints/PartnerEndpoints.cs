using Partners.Core.Contracts;
using Partners.Core.DTOs.Requests;
using Partners.Core.DTOs.Responses;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;

namespace Partners.Api.Endpoints;

public static class PartnerEndpoints
{
    public static IEndpointRouteBuilder MapPartnerEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/partners")
            .AddFluentValidationAutoValidation()
            .RequireAuthorization(policy => policy.RequireRole("PolicyManager"));

        group.MapGet("/", async (IPartnerService service) =>
        {
            var partners = await service.GetAllPartnersAsync();
            return Results.Ok(partners);
        })
        .Produces<IEnumerable<PartnerListItemResponse>>();

        group.MapGet("/{id:int}", async (int id, IPartnerService service) =>
        {
            var partner = await service.GetByIdAsync(id);
            return partner is null
                ? Results.NotFound(new { message = $"Partner with Id '{id}' was not found." })
                : Results.Ok(partner);
        })
        .Produces<PartnerDetailResponse>()
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreatePartnerRequest request, IPartnerService service) =>
        {
            var result = await service.CreateAsync(request);
            return result.Success
                ? Results.Created($"api/partners/{result.PartnerId}", new { id = result.PartnerId })
                : Results.BadRequest(new { errors = result.Errors });
        })
        .Produces(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        return app;
    }
}
