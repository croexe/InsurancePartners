using FluentValidation;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.EntityFrameworkCore;
using Partners.Api.Caching;
using Partners.Api.ErrorHandling;
using Partners.Api.Extensions.Configurations;
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

        // t2: ogranicenje velicine request body-ja (Kestrel) — zastita od velikih payloada.
        var maxRequestBodyBytes = configuration.GetValue<long?>("RequestLimits:MaxBodyBytes") ?? 262144;
        builder.WebHost.ConfigureKestrel(kestrel =>
        {
            kestrel.Limits.MaxRequestBodySize = maxRequestBodyBytes;
        });

        // t3: request timeout — spori zahtjevi ne drze resurse (npr. Slowloris).
        var requestTimeoutSeconds = configuration.GetValue<int?>("RequestTimeout:Seconds") ?? 30;
        services.AddRequestTimeouts(options =>
        {
            options.DefaultPolicy = new RequestTimeoutPolicy
            {
                Timeout = TimeSpan.FromSeconds(requestTimeoutSeconds)
            };
        });

        services.AddSignalR();

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("InsurancePartnersDb")));

        services.AddJwtAuthentication(configuration);

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

        // t4: in-memory output caching — ponovljeni GET-ovi se posluze iz kesa umjesto iz baze.
        // Custom policy "PartnersList" kesira i autentificirane GET-ove (lista je ista za sve
        // PolicyManagere → shared kes), s tagom za invalidaciju na write.
        services.AddOutputCache(options =>
        {
            options.AddPolicy("PartnersList", new PartnersListCachePolicy());
        });

        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy
                    .WithOrigins(builder.Configuration["Cors:AllowedOrigin"]!)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        services.AddRateLimitingPolicies();

        return builder;
    }
}
