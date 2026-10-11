using TaskFlow.Application.Common;
using TaskFlow.Application.Common.Interfaces;

namespace TaskFlow.Application.Commands.Tasks.AssignTask;

/// <summary>Assigns the task to a member of the project's team; a null assignee unassigns it.</summary>
public sealed record AssignTaskCommand(Guid Id, Guid? AssigneeId) : IUserCommand
{
    public UserCommandContext UserCommandContext { get; init; } = null!;
    public Guid CorrelationId { get; init; }
}
