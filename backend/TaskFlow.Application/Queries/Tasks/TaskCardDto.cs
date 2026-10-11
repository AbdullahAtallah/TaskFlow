using TaskFlow.Domain.Enums;
using TaskStatus = TaskFlow.Domain.Enums.TaskStatus;

namespace TaskFlow.Application.Queries.Tasks;

public sealed record TaskCardDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public TaskStatus Status { get; init; }
    public TaskPriority Priority { get; init; }
    public DateOnly? DueDate { get; init; }
    public bool IsOverdue { get; init; }
    public Guid? AssigneeId { get; init; }
    public string? AssigneeName { get; init; }
}
