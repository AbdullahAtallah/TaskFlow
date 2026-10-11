using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace TaskFlow.Infrastructure.Identity;

/// <summary>
/// The access tokens TaskFlow issues itself. Issuer, audience and lifetime live in appsettings.json; the signing key
/// is a secret from User Secrets or the environment variable Jwt__SigningKey, never from a committed file.
/// </summary>
public sealed class JwtSettings
{
    public const string SectionName = "Jwt";

    /// <summary>HMAC-SHA256 needs a key of at least 256 bits, so at least 32 characters.</summary>
    public const int MinimumSigningKeyLength = 32;

    [Required]
    public string Issuer { get; set; } = string.Empty;

    [Required]
    public string Audience { get; set; } = string.Empty;

    [Required]
    [MinLength(MinimumSigningKeyLength)]
    public string SigningKey { get; set; } = string.Empty;

    /// <summary>How long a token is valid. There are no refresh tokens: after this the user signs in again.</summary>
    [Range(typeof(TimeSpan), "00:01:00", "7.00:00:00")]
    public TimeSpan AccessTokenLifetime { get; set; } = TimeSpan.FromHours(8);

    public SymmetricSecurityKey CreateSigningKey()
    {
        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKey));
    }
}
