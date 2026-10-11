using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.Queries.Tasks;

/// <summary>The task board of one project; the priority and assignee filters also apply to the board.</summary>
public sealed class GetTaskBoardQuery
{
    public Guid ProjectId { get; set; }
    public TaskPriority? Priority { get; set; }
    public Guid? AssigneeId { get; set; }
    public bool Unassigned { get; set; }
}
