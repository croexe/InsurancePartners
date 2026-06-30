using Microsoft.AspNetCore.Http.Timeouts;

namespace Partners.Api.Extensions.Configurations;

internal static class RequestHardeningExtensions
{
    // Zastita od resursno-iscrpljujucih zahtjeva (DDoS): ogranicenje velicine body-ja + request timeout.
    public static WebApplicationBuilder AddRequestHardening(this WebApplicationBuilder builder)
    {
        var configuration = builder.Configuration;

        // t2: ogranicenje velicine request body-ja (Kestrel) — zastita od velikih payloada.
        var maxRequestBodyBytes = configuration.GetValue<long?>("RequestLimits:MaxBodyBytes") ?? 262144;
        builder.WebHost.ConfigureKestrel(kestrel =>
        {
            kestrel.Limits.MaxRequestBodySize = maxRequestBodyBytes;
        });

        // t3: request timeout — spori zahtjevi ne drze resurse (npr. Slowloris).
        var requestTimeoutSeconds = configuration.GetValue<int?>("RequestTimeout:Seconds") ?? 30;
        builder.Services.AddRequestTimeouts(options =>
        {
            options.DefaultPolicy = new RequestTimeoutPolicy
            {
                Timeout = TimeSpan.FromSeconds(requestTimeoutSeconds)
            };
        });

        return builder;
    }
}
