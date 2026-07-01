using Microsoft.AspNetCore.OutputCaching;
using Partners.Api.Constants;
using Partners.Core.Constants;
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
            .RequireAuthorization(policy => policy.RequireRole(Roles.PolicyManager));

        group.MapPost("/", async (
            CreatePolicyRequest request,
            IPolicyService service,
            IPartnerNotifier notifier,
            IOutputCacheStore outputCacheStore,
            ILoggerFactory loggerFactory,
            CancellationToken cancellationToken) =>
        {
            var result = await service.CreatePolicyAsync(request, cancellationToken);

            if (!result.Success)
            {
                return Results.BadRequest(new { errors = result.Errors });
            }

            // Nova polica mijenja flag/summary partnera u listi — invalidiraj kesiranu listu.
            await outputCacheStore.EvictByTagAsync(CacheConstants.PartnersTag, cancellationToken);

            // Notifikacija je sporedni efekt — njen neuspjeh ne smije srušiti uspješno kreiranu policu.
            try
            {
                await notifier.NotifyPartnerFlagChangedAsync(result.Value!.Policy.PartnerId, result.Value.IsFlagged);
            }
            catch (Exception ex)
            {
                loggerFactory
                    .CreateLogger("Partners.Api.Endpoints.PolicyEndpoints")
                    .LogWarning(ex,
                        "Polica {PolicyId} je spremljena, ali real-time notifikacija o flagu partnera {PartnerId} nije uspjela.",
                        result.Value!.Policy.Id, result.Value.Policy.PartnerId);
            }

            return Results.Created($"/api/partners/{result.Value!.Policy.PartnerId}", result.Value.Policy);
        })
        .Produces<PolicyResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        return app;
    }
}
