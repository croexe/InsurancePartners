using FluentValidation;
using Partners.Api.ErrorHandling;
using Partners.Api.Extensions.Configurations;
using Partners.Api.Notifications;
using Partners.Core.Contracts;
using Partners.Core.Services;
using Partners.Core.Validators;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;
using System.Text.Json.Serialization;

namespace Partners.Api.Extensions;

internal static class ServiceCollectionExtensions
{
    public static WebApplicationBuilder ApplyWienConfiguration(this WebApplicationBuilder builder)
    {
        builder.AddSerilogLogging();

        var services = builder.Services;
        var configuration = builder.Configuration;

        services.AddOpenApi();

        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        builder.AddRequestHardening();

        services.AddSignalR();

        services.AddPersistence(configuration);

        services.AddJwtAuthentication(configuration);

        services.AddScoped<IPartnerService, PartnerService>();
        services.AddScoped<IPolicyService, PolicyService>();
        services.AddScoped<IPartnerNotifier, SignalRPartnerNotifier>();

        services.AddValidatorsFromAssemblyContaining<CreatePartnerRequestValidator>();
        services.AddFluentValidationAutoValidation();

        services.AddProblemDetails();
        services.AddExceptionHandler<ProblemExceptionHandler>();

        services.AddOutputCachePolicies();

        services.AddCorsPolicy(configuration);

        services.AddRateLimitingPolicies();

        return builder;
    }
}
