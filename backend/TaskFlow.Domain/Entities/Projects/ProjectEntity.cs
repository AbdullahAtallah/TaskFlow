using TaskFlow.Domain.Entities.Tasks;
using TaskFlow.Domain.Entities.Teams;

namespace TaskFlow.Domain.Entities.Projects;

/// <summary>A project always belongs to exactly one team.</summary>
public class ProjectEntity : BaseEntity
{
    public Guid TeamId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }

    public TeamEntity? Team { get; set; }
    public ICollection<TaskEntity> Tasks { get; set; } = [];
}
