using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Partners.Api.ErrorHandling;
using Partners.Api.Notifications;
using Partners.Core.Contracts;
using Partners.Core.Services;
using Partners.Core.Validators;
using Partners.Dal.Database;
using Partners.Dal.Repositories;
using Serilog;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;

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

            // Globalni limiter — vrijedi za SVE endpointe (zastita od opceg flooda / L7 zlouporabe).
            // Particioniran po IP-u; config se cita lazy (kao i login policy) radi override-a u testovima.
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
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
            });

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
