using TaskFlow.Application.Common.Filtering;
using TaskFlow.Domain.Enums;
using TaskStatus = TaskFlow.Domain.Enums.TaskStatus;

namespace TaskFlow.Application.Queries.Tasks;

public sealed class GetTasksQuery : FilterParams
{
    public Guid? ProjectId { get; set; }
    public TaskStatus? Status { get; set; }
    public TaskPriority? Priority { get; set; }
    public Guid? AssigneeId { get; set; }

    /// <summary>Only tasks without an assignee; cannot be combined with <see cref="AssigneeId"/>.</summary>
    public bool Unassigned { get; set; }

    /// <summary>Overdue: not done and the due date lies before today (UTC).</summary>
    public bool? IsOverdue { get; set; }

    /// <summary>Inclusive lower bound of the due date; tasks without a due date are left out.</summary>
    public DateOnly? DueFrom { get; set; }

    /// <summary>Inclusive upper bound of the due date; tasks without a due date are left out.</summary>
    public DateOnly? DueTo { get; set; }
}

public static class TaskSortFields
{
    public const string Title = "title";
    public const string Status = "status";
    public const string Priority = "priority";
    public const string DueDate = "dueDate";
    public const string DateTimeCreated = "dateTimeCreated";

    public static IReadOnlyList<string> All { get; } = [Title, Status, Priority, DueDate, DateTimeCreated];
}
