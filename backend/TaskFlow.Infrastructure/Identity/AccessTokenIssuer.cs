using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using TaskFlow.Application.Identity;
using TaskFlow.Domain.Entities.Users;

namespace TaskFlow.Infrastructure.Identity;

/// <summary>Signs access tokens with HMAC-SHA256; the API validates them with the same key.</summary>
internal sealed class AccessTokenIssuer(IOptions<JwtSettings> jwtSettings, TimeProvider timeProvider) : IAccessTokenIssuer
{
    public AccessTokenDto Issue(UserEntity user)
    {
        var settings = jwtSettings.Value;

        // Whole seconds, like the exp claim, so ExpiresAt is exactly when the token stops working.
        var now = DateTimeOffset.FromUnixTimeSeconds(timeProvider.GetUtcNow().ToUnixTimeSeconds()).UtcDateTime;
        var expiresAt = now + settings.AccessTokenLifetime;

        // No not-before claim: an instance whose clock runs a little behind must still accept a fresh token.
        var tokenHandler = new JsonWebTokenHandler { SetDefaultTimesOnTokenCreation = false };
        var accessToken = tokenHandler.CreateToken(new SecurityTokenDescriptor
        {
            Issuer = settings.Issuer,
            Audience = settings.Audience,
            IssuedAt = now,
            Expires = expiresAt,
            Claims = new Dictionary<string, object>
            {
                [AccessTokenClaims.UserId] = user.Id.ToString(),
                [AccessTokenClaims.Email] = user.UserName,
                [AccessTokenClaims.Name] = user.DisplayName,
                [AccessTokenClaims.SecurityStamp] = user.SecurityStamp.ToString(),
                [AccessTokenClaims.TokenId] = Guid.NewGuid().ToString()
            },
            SigningCredentials = new SigningCredentials(settings.CreateSigningKey(), SecurityAlgorithms.HmacSha256)
        });

        return new AccessTokenDto(accessToken, "Bearer", expiresAt);
    }
}
