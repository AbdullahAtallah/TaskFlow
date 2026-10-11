using System.Text.Json.Serialization;
using TaskFlow.Application.Commands.Tasks.Common;
using TaskFlow.Application.Common;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Enums;
using TaskStatus = TaskFlow.Domain.Enums.TaskStatus;

namespace TaskFlow.Application.Commands.Tasks.CreateTask;

public sealed record CreateTaskCommand(
    Guid ProjectId,
    string Title,
    string? Description,
    [property: JsonRequired] TaskStatus Status,
    [property: JsonRequired] TaskPriority Priority,
    DateOnly? DueDate,
    Guid? AssigneeId) : IUserCommand, ITaskFieldsCommand
{
    public UserCommandContext UserCommandContext { get; init; } = null!;
    public Guid CorrelationId { get; init; }
}
