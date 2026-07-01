using Partners.Core.Results;

namespace Partners.Api.Endpoints;

internal static class ResultExtensions
{
    // Jedinstveni oblik odgovora za neuspjeh servisa (konzistentan error contract).
    public static IResult ToBadRequest<T>(this Result<T> result) =>
        Results.BadRequest(new { errors = result.Errors });
}
