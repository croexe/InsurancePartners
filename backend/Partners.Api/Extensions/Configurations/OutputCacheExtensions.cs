using Partners.Api.Caching;

namespace Partners.Api.Extensions.Configurations;

internal static class OutputCacheExtensions
{
    // t4: in-memory output caching — ponovljeni GET-ovi se posluze iz kesa umjesto iz baze.
    // Custom policy "PartnersList" kesira i autentificirane GET-ove (lista je ista za sve
    // PolicyManagere → shared kes), s tagom za invalidaciju na write.
    public static IServiceCollection AddOutputCachePolicies(this IServiceCollection services)
    {
        services.AddOutputCache(options =>
        {
            options.AddPolicy("PartnersList", new PartnersListCachePolicy());
        });

        return services;
    }
}
