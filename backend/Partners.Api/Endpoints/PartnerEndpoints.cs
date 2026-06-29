using Microsoft.AspNetCore.OutputCaching;
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

        group.MapGet("/", async (IPartnerService service, CancellationToken cancellationToken) =>
        {
            var partners = await service.GetAllPartnersAsync(cancellationToken);
            return Results.Ok(partners);
        })
        .CacheOutput("PartnersList")
        .Produces<IEnumerable<PartnerListItemResponse>>();

        group.MapGet("/{id:int}", async (int id, IPartnerService service, CancellationToken cancellationToken) =>
        {
            var partner = await service.GetPartnerDetailsByIdAsync(id, cancellationToken);
            return partner is null
                ? Results.NotFound(new { message = $"Partner with Id '{id}' was not found." })
                : Results.Ok(partner);
        })
        .Produces<PartnerDetailResponse>()
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", async (
            CreatePartnerRequest request,
            IPartnerService service,
            IOutputCacheStore outputCacheStore,
            CancellationToken cancellationToken) =>
        {
            var result = await service.CreatePartnerAsync(request, cancellationToken);

            if (!result.Success)
            {
                return Results.BadRequest(new { errors = result.Errors });
            }

            await outputCacheStore.EvictByTagAsync("partners", cancellationToken);

            return Results.Created($"/api/partners/{result.PartnerId}", new { id = result.PartnerId });
        })
        .Produces(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        return app;
    }
}
