using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using TaskFlow.Domain.Entities.Users;
using TaskFlow.Infrastructure.Identity;

namespace TaskFlow.IntegrationTests.Infrastructure;

/// <summary>Hand-made access tokens for the token validation tests: valid ones and every kind of broken one.</summary>
public static class TestTokens
{
    public static string Create(
        TaskFlowApiFactory factory,
        UserEntity? user,
        DateTime? expires = null,
        string? issuer = null,
        string? audience = null,
        string? signingKey = null,
        bool signed = true,
        Guid? securityStamp = null)
    {
        var settings = factory.Services.GetRequiredService<IOptions<JwtSettings>>().Value;
        var claims = new Dictionary<string, object>();
        if (user is not null)
        {
            claims[AccessTokenClaims.UserId] = user.Id.ToString();
            claims[AccessTokenClaims.SecurityStamp] = (securityStamp ?? user.SecurityStamp).ToString();
        }

        var expiresAt = expires ?? DateTime.UtcNow.AddMinutes(10);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey ?? factory.SigningKey));

        return new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = issuer ?? settings.Issuer,
            Audience = audience ?? settings.Audience,
            IssuedAt = expiresAt.AddMinutes(-20),
            NotBefore = expiresAt.AddMinutes(-20),
            Expires = expiresAt,
            Claims = claims,
            SigningCredentials = signed ? new SigningCredentials(key, SecurityAlgorithms.HmacSha256) : null
        });
    }
}
