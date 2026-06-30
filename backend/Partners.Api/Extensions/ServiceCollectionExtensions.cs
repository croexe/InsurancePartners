using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;

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

        services.AddIdentity<IdentityUser, IdentityRole>(options =>
        {
            options.Password.RequiredLength = 8;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequireDigit = true;
            options.Password.RequireUppercase = true;

            // Zakljucavanje racuna nakon 5 neuspjelih pokusaja na 15 minuta (zastita od brute-force).
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.Lockout.AllowedForNewUsers = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = configuration["Jwt:Issuer"],
                ValidAudience = configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]!))
            };
        });

        services.AddAuthorization();

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

        // Rate limiting za login endpoint — zastita od password-spraying napada.
        // Limit je konfigurabilan i particioniran po IP-u. Config se cita lazy (na runtime)
        // unutar policyja kako bi override-ovi iz okoline/testova bili vidljivi.
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // Globalni limiter — vrijedi za SVE endpointe. Lanac dva limitera (zahtjev mora proci OBA):
            //   1) fixed window po IP-u  → "koliko zahtjeva po minuti" (volumetrijski)
            //   2) concurrency (globalni) → "koliko zahtjeva ISTOVREMENO" (zastita resursa)
            // Config se cita lazy (kao login policy) radi override-a u testovima.
            options.GlobalLimiter = PartitionedRateLimiter.CreateChained(
                PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                {
                    var config = httpContext.RequestServices.GetRequiredService<IConfiguration>();
                    var permitLimit = config.GetValue<int?>("RateLimiting:Global:PermitLimit") ?? 100;
                    var windowSeconds = config.GetValue<int?>("RateLimiting:Global:WindowSeconds") ?? 60;

                    return RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = permitLimit,
                            Window = TimeSpan.FromSeconds(windowSeconds)
                        });
                }),
                PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                {
                    var config = httpContext.RequestServices.GetRequiredService<IConfiguration>();
                    var permitLimit = config.GetValue<int?>("RateLimiting:Concurrency:PermitLimit") ?? 20;
                    var queueLimit = config.GetValue<int?>("RateLimiting:Concurrency:QueueLimit") ?? 50;

                    return RateLimitPartition.GetConcurrencyLimiter(
                        partitionKey: "global",
                        factory: _ => new ConcurrencyLimiterOptions
                        {
                            PermitLimit = permitLimit,
                            QueueLimit = queueLimit,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                        });
                }));

            options.AddPolicy("login", httpContext =>
            {
                var config = httpContext.RequestServices.GetRequiredService<IConfiguration>();
                var permitLimit = config.GetValue<int?>("RateLimiting:Login:PermitLimit") ?? 10;
                var windowSeconds = config.GetValue<int?>("RateLimiting:Login:WindowSeconds") ?? 60;

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = permitLimit,
                        Window = TimeSpan.FromSeconds(windowSeconds)
                    });
            });
        });

        return builder;
    }
}
