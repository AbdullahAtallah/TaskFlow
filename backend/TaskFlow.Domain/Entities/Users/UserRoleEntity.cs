using TaskFlow.Domain.Entities.Authorization;

namespace TaskFlow.Domain.Entities.Users;

/// <summary>Junction row between a user and a role; hard-deleted when removed.</summary>
public class UserRoleEntity
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }

    public UserEntity? User { get; set; }
    public RoleEntity? Role { get; set; }
}
