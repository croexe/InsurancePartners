using Partners.Core.Results;

namespace Partners.Api.Authentication;

public interface IAuthService
{
    // Vraca Result s JWT-om na uspjeh; neuspjeh (kriva lozinka, nepostojeci/zakljucan racun)
    // je Fail bez detalja — endpoint ga mapira u 401.
    Task<Result<string>> AuthenticateAsync(string? email, string? password);
}
