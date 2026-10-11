using TaskFlow.Domain.Entities.Users;

namespace TaskFlow.Domain.Entities.Authorization;

public class RoleEntity : BaseEntity
{
    public required string Name { get; set; }
    public string? Description { get; set; }

    /// <summary>The role newly registered users receive. Exactly one role has this flag.</summary>
    public bool IsDefault { get; set; }

    public ICollection<RolePackageEntity> RolePackages { get; set; } = [];
    public ICollection<RoleClaimEntity> RoleClaims { get; set; } = [];
    public ICollection<UserRoleEntity> UserRoles { get; set; } = [];
}
