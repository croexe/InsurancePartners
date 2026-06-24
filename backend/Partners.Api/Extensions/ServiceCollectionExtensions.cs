using FluentValidation;
using Partners.Api.ErrorHandling;
using Partners.Api.Notifications;
using Partners.Core.Contracts;
using Partners.Core.Services;
using Partners.Core.Validators;
using Partners.Dal.Database;
using Partners.Dal.Repositories;
using Serilog;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;
using System.Text.Json.Serialization;

namespace Partners.Api.Extensions;

internal static class ServiceCollectionExtensions
{
    public static WebApplicationBuilder ApplyWienConfiguration(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((ctx, config) =>
        {
            var logPath = ctx.Configuration["Serilog:LogPath"]!;

            config
                .ReadFrom.Configuration(ctx.Configuration)
                .WriteTo.Console()
                .WriteTo.Async(a => a.File(
                    logPath,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"));
        });

        var services = builder.Services;
        var configuration = builder.Configuration;

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

        return builder;
    }
}
