using Microsoft.AspNetCore.OutputCaching;

namespace Partners.Api.Caching;

public sealed class PartnersListCachePolicy : IOutputCachePolicy
{
    private static readonly TimeSpan Duration = TimeSpan.FromSeconds(10);

    public ValueTask CacheRequestAsync(OutputCacheContext context, CancellationToken cancellationToken)
    {
        // Kesiramo samo GET; namjerno NE iskljucujemo autentificirane zahtjeve jer je
        // lista ista za sve PolicyManagere (shared kes). Tag sluzi za invalidaciju na write.
        var isGet = HttpMethods.IsGet(context.HttpContext.Request.Method);

        context.EnableOutputCaching = isGet;
        context.AllowCacheLookup = isGet;
        context.AllowCacheStorage = isGet;
        context.AllowLocking = true;
        context.ResponseExpirationTimeSpan = Duration;
        context.Tags.Add("partners");

        return ValueTask.CompletedTask;
    }

    public ValueTask ServeFromCacheAsync(OutputCacheContext context, CancellationToken cancellationToken)
        => ValueTask.CompletedTask;

    public ValueTask ServeResponseAsync(OutputCacheContext context, CancellationToken cancellationToken)
    {
        // Kesiraj samo uspjesne (200) odgovore.
        if (context.HttpContext.Response.StatusCode != StatusCodes.Status200OK)
        {
            context.AllowCacheStorage = false;
        }

        return ValueTask.CompletedTask;
    }
}
