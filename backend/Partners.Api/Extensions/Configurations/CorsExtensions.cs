namespace Partners.Api.Extensions.Configurations;

internal static class CorsExtensions
{
    // Dopusta frontendu (konfigurabilni origin) pristup API-ju s kredencijalima.
    public static IServiceCollection AddCorsPolicy(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy
                    .WithOrigins(configuration["Cors:AllowedOrigin"]!)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        return services;
    }
}
