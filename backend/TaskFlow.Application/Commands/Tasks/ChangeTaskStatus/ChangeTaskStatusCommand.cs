using System.Text.Json.Serialization;
using TaskFlow.Application.Common;
using TaskFlow.Application.Common.Interfaces;
using TaskStatus = TaskFlow.Domain.Enums.TaskStatus;

namespace TaskFlow.Application.Commands.Tasks.ChangeTaskStatus;

/// <summary>A request body without status answers 400 instead of moving the task back to ToDo.</summary>
public sealed record ChangeTaskStatusCommand(Guid Id, [property: JsonRequired] TaskStatus Status) : IUserCommand
{
    public UserCommandContext UserCommandContext { get; init; } = null!;
    public Guid CorrelationId { get; init; }
}
