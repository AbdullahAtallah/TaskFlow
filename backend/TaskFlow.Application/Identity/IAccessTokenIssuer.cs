using TaskFlow.Domain.Entities.Users;

namespace TaskFlow.Application.Identity;

public interface IAccessTokenIssuer
{
    /// <summary>A signed access token with the user's id, e-mail address, display name and security stamp.</summary>
    AccessTokenDto Issue(UserEntity user);
}
