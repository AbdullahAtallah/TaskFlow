using TaskFlow.Application.Identity;

namespace TaskFlow.Infrastructure.Identity;

public interface ICurrentAuthContextResolver
{
    /// <summary>
    /// The TaskFlow user behind the access token of this request with their permissions, or null when the request is
    /// not authenticated, the user no longer exists or the token's session has ended. Throws for deactivated users.
    /// </summary>
    Task<CurrentUser?> GetUserAsync(CancellationToken cancellationToken = default);
}
