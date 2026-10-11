using TaskFlow.Domain.Enums;
using TaskStatus = TaskFlow.Domain.Enums.TaskStatus;

namespace TaskFlow.Application.Commands.Tasks.Common;

/// <summary>The fields a task form edits; shared by the create and update commands.</summary>
public interface ITaskFieldsCommand
{
    string Title { get; }
    string? Description { get; }
    TaskStatus Status { get; }
    TaskPriority Priority { get; }
    Guid? AssigneeId { get; }
}
