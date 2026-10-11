using TaskFlow.Domain.Entities.Projects;
using TaskFlow.Domain.Entities.Users;
using TaskFlow.Domain.Enums;
using TaskStatus = TaskFlow.Domain.Enums.TaskStatus;

namespace TaskFlow.Domain.Entities.Tasks;

public class TaskEntity : BaseEntity
{
    public Guid ProjectId { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public TaskStatus Status { get; set; }
    public TaskPriority Priority { get; set; }
    public DateOnly? DueDate { get; set; }

    /// <summary>Must be a member of the project's team.</summary>
    public Guid? AssigneeId { get; set; }
    public Guid CreatedById { get; set; }

    public ProjectEntity? Project { get; set; }
    public UserEntity? Assignee { get; set; }
    public UserEntity? CreatedBy { get; set; }
}
