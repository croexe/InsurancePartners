using FluentValidation;
using Partners.Api.ErrorHandling;
using Partners.Api.Notifications;
using Partners.Core.Contracts;
using Partners.Core.Services;
using Partners.Core.Validators;
using Partners.Dal.Database;
using Partners.Dal.Repositories;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;
using System.Text.Json.Serialization;

namespace Partners.Api.Extensions;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection ApplyWienConfiguration(
        this IServiceCollection services,
        IWebHostEnvironment environment,
        IConfiguration configuration)
    {
        services.AddOpenApi();

        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        services.AddSignalR();

        services.AddSingleton<IDbConnectionFactory>(_ =>
            new SqlConnectionFactory(
                configuration.GetConnectionString("InsurancePartnersDb")!));

        services.AddScoped<IPartnerRepository, PartnerRepository>();
        services.AddScoped<IPolicyRepository, PolicyRepository>();
        services.AddScoped<IPartnerService, PartnerService>();
        services.AddScoped<IPolicyService, PolicyService>();
        services.AddScoped<IPartnerNotifier, SignalRPartnerNotifier>();

        services.AddValidatorsFromAssemblyContaining<CreatePartnerRequestValidator>();
        services.AddFluentValidationAutoValidation();

        services.AddProblemDetails();
        services.AddExceptionHandler<ProblemExceptionHandler>();

        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .SetIsOriginAllowed(_ => true) // development
                    .AllowCredentials();
            });
        });

        return services;
    }
}
