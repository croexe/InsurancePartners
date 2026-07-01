using Microsoft.AspNetCore.RateLimiting;
using Partners.Api.Constants;
using System.Threading.RateLimiting;

namespace Partners.Api.Extensions.Configurations;

internal static class RateLimitingExtensions
{
    // Rate limiting — zastita od password-spraying i volumetrijskih napada.
    // Limiti su konfigurabilni i particionirani po IP-u. Config se cita lazy (na runtime)
    // unutar policyja kako bi override-ovi iz okoline/testova bili vidljivi.
    public static IServiceCollection AddRateLimitingPolicies(this IServiceCollection services)
    {
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

            options.AddPolicy(PolicyNames.LoginRateLimit, httpContext =>
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

        return services;
    }
}
