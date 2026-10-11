using TaskFlow.Domain.Entities.Users;

namespace TaskFlow.Domain.Entities.Teams;

/// <summary>Junction row between a team and a user; removing a member deletes the row.</summary>
public class TeamMemberEntity
{
    public Guid TeamId { get; set; }
    public Guid UserId { get; set; }

    public TeamEntity? Team { get; set; }
    public UserEntity? User { get; set; }
}
