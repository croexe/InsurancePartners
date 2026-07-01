using Microsoft.AspNetCore.OutputCaching;
using Partners.Api.Constants;
using Partners.Api.Endpoints.Extensions;
using Partners.Core.Constants;
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
            .RequireAuthorization(policy => policy.RequireRole(Roles.PolicyManager));

        group.MapGet("/", async (int? page, int? pageSize, IPartnerService service, CancellationToken cancellationToken) =>
        {
            var currentPage = Math.Max(page ?? 1, 1);
            var size = Math.Clamp(pageSize ?? 10, 1, 100);
            var partners = await service.GetPartnersPageAsync(currentPage, size, cancellationToken);
            return Results.Ok(partners);
        })
        .CacheOutput(CacheConstants.PartnersListPolicy)
        .Produces<PagedResponse<PartnerListItemResponse>>();

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
                return result.ToBadRequest();
            }

            await outputCacheStore.EvictByTagAsync(CacheConstants.PartnersTag, cancellationToken);

            return Results.Created($"/api/partners/{result.Value}", new { id = result.Value });
        })
        .Produces(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        return app;
    }
}
