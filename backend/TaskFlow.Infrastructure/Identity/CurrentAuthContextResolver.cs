using Microsoft.AspNetCore.Http;
using TaskFlow.Application.Identity;

namespace TaskFlow.Infrastructure.Identity;

/// <summary>
/// Reads the user of the token from the database once per request, so a deactivation, role change or logout applies
/// to the next request at once, on every instance of the API.
/// </summary>
internal sealed class CurrentAuthContextResolver(
    IHttpContextAccessor httpContextAccessor,
    CurrentUserDataReader currentUserDataReader) : ICurrentAuthContextResolver
{
    /// <summary>Where the [Authorization] filter keeps the resolved user for the rest of the request.</summary>
    public const string HttpContextItemKey = "TaskFlow.CurrentUser";

    public async Task<CurrentUser?> GetUserAsync(CancellationToken cancellationToken = default)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext?.User.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        if (httpContext.Items.TryGetValue(HttpContextItemKey, out var resolved) && resolved is CurrentUser resolvedUser)
        {
            return resolvedUser;
        }

        var userId = AccessTokenClaims.ReadUserId(httpContext.User);
        var securityStamp = AccessTokenClaims.ReadSecurityStamp(httpContext.User);
        if (userId is null || securityStamp is null)
        {
            return null;
        }

        var currentUser = await currentUserDataReader.ReadAsync(userId.Value, securityStamp.Value, cancellationToken);
        if (currentUser is not null)
        {
            httpContext.Items[HttpContextItemKey] = currentUser;
        }

        return currentUser;
    }
}
