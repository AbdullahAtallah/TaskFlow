using System.Security.Claims;

namespace TaskFlow.Infrastructure.Identity;

/// <summary>
/// The claims of a TaskFlow access token. A token only says who the user is: permissions are read from the database on
/// every request, so a role change applies at once.
/// </summary>
public static class AccessTokenClaims
{
    public const string UserId = "sub";
    public const string Email = "email";
    public const string Name = "name";
    public const string SecurityStamp = "security_stamp";
    public const string TokenId = "jti";

    public static Guid? ReadUserId(ClaimsPrincipal principal)
    {
        return ReadGuid(principal, UserId);
    }

    public static Guid? ReadSecurityStamp(ClaimsPrincipal principal)
    {
        return ReadGuid(principal, SecurityStamp);
    }

    private static Guid? ReadGuid(ClaimsPrincipal principal, string claimType)
    {
        return Guid.TryParse(principal.FindFirst(claimType)?.Value, out var value) ? value : null;
    }
}
