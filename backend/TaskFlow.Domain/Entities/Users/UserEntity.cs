using TaskFlow.Domain.Entities.Teams;

namespace TaskFlow.Domain.Entities.Users;

/// <summary>A TaskFlow user, who signs in with their e-mail address and password.</summary>
public class UserEntity : BaseEntity
{
    /// <summary>The e-mail address, stored in lower case.</summary>
    public required string UserName { get; set; }
    public required string DisplayName { get; set; }
    public bool IsActive { get; set; } = true;

    /// <summary>The salted hash of the password; null for an account that cannot sign in with a password.</summary>
    public string? PasswordHash { get; set; }

    /// <summary>
    /// Copied into every access token. Logging out or setting a new password replaces it, which ends every session:
    /// a token with an old stamp is refused.
    /// </summary>
    public Guid SecurityStamp { get; set; } = Guid.NewGuid();

    public ICollection<UserRoleEntity> UserRoles { get; set; } = [];
    public ICollection<TeamMemberEntity> TeamMemberships { get; set; } = [];
}
