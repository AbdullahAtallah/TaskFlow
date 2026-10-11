using TaskFlow.Domain.Entities.Projects;

namespace TaskFlow.Domain.Entities.Teams;

public class TeamEntity : BaseEntity
{
    public required string Name { get; set; }
    public string? Description { get; set; }

    public ICollection<TeamMemberEntity> Members { get; set; } = [];
    public ICollection<ProjectEntity> Projects { get; set; } = [];
}
