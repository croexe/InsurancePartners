using Microsoft.AspNetCore.Identity;

namespace Partners.Api.Authentication;

public interface IJwtTokenGenerator
{
    string GenerateToken(IdentityUser user, IEnumerable<string> roles);
}
