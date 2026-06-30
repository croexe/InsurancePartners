using System.ComponentModel.DataAnnotations;

namespace Partners.Api.Authentication;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required]
    [MinLength(32, ErrorMessage = "Jwt:Secret mora imati barem 32 znaka (256-bitni HMAC kljuc).")]
    public string Secret { get; set; } = string.Empty;

    [Required]
    public string Issuer { get; set; } = string.Empty;

    [Required]
    public string Audience { get; set; } = string.Empty;

    public int ExpiryHours { get; set; } = 8;
}
