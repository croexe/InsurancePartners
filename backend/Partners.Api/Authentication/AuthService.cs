using Microsoft.AspNetCore.Identity;
using Partners.Core.Results;

namespace Partners.Api.Authentication;

public sealed class AuthService : IAuthService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly IJwtTokenGenerator _tokenGenerator;

    public AuthService(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        IJwtTokenGenerator tokenGenerator)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<Result<string>> AuthenticateAsync(string? email, string? password)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            return Result<string>.Fail();
        }

        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return Result<string>.Fail();
        }

        // Broji neuspjele pokusaje i zakljucava racun (lockoutOnFailure: true).
        var signIn = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);
        if (!signIn.Succeeded)
        {
            return Result<string>.Fail();
        }

        var roles = await _userManager.GetRolesAsync(user);
        var token = _tokenGenerator.GenerateToken(user, roles);

        return Result<string>.Ok(token);
    }
}
