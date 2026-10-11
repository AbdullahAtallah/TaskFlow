using TaskFlow.Domain.Enums;
using TaskStatus = TaskFlow.Domain.Enums.TaskStatus;

namespace TaskFlow.Application.Queries.Tasks;

public sealed record TaskDetailDto
{
    public Guid Id { get; init; }
    public Guid ProjectId { get; init; }
    public string ProjectName { get; init; } = string.Empty;
    public Guid TeamId { get; init; }
    public string TeamName { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public TaskStatus Status { get; init; }
    public TaskPriority Priority { get; init; }
    public DateOnly? DueDate { get; init; }
    public bool IsOverdue { get; init; }
    public Guid? AssigneeId { get; init; }
    public string? AssigneeName { get; init; }
    public Guid CreatedById { get; init; }
    public DateTime DateTimeCreated { get; init; }
    public DateTime? DateTimeModified { get; init; }

    /// <summary>Send this back with an edit: a save over a newer change is then refused.</summary>
    public byte[] RowVersion { get; init; } = [];
}
