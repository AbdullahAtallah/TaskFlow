using Microsoft.AspNetCore.Mvc;
using TaskFlow.Api.Shared;
using TaskFlow.Application.Commands.Tasks.AssignTask;
using TaskFlow.Application.Commands.Tasks.ChangeTaskStatus;
using TaskFlow.Application.Commands.Tasks.CreateTask;
using TaskFlow.Application.Commands.Tasks.DeleteTask;
using TaskFlow.Application.Commands.Tasks.UpdateTask;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Constants;
using TaskFlow.Domain.Enums;
using TaskFlow.Infrastructure.Middleware.Attributes;
using TaskFlow.Infrastructure.Services;
using Wolverine;

namespace TaskFlow.Api.Controllers.Tasks;

[Route("api/task")]
[ApiController]
[Authorization(PackageTypes.Tasks)]
public class TaskController(
    IMessageBus messageBus,
    ISendNotificationCommandHandler sendNotificationCommandHandler,
    ICurrentUserRetriever currentUserRetriever)
    : CommandControllerBase(messageBus, sendNotificationCommandHandler, currentUserRetriever)
{
    /// <summary>Creates a task inside a project; setting an assignee also requires Tasks.Assign.</summary>
    [HttpPost]
    [Claim(ClaimTypes.TasksCreate)]
    public Task<AcceptedResult> Post([FromBody] CreateTaskCommand command)
    {
        return PublishAsync(command with { UserCommandContext = UserContext, CorrelationId = Guid.NewGuid() }, EntityType.Task);
    }

    /// <summary>Updates every field; changing the assignee also requires Tasks.Assign.</summary>
    [HttpPut("{id:guid}")]
    [Claim(ClaimTypes.TasksManage)]
    public Task<AcceptedResult> Put(Guid id, [FromBody] UpdateTaskCommand command)
    {
        return PublishAsync(command with { Id = id, UserCommandContext = UserContext, CorrelationId = Guid.NewGuid() }, EntityType.Task);
    }

    [HttpPut("{id:guid}/status")]
    [Claim(ClaimTypes.TasksManage)]
    public Task<AcceptedResult> PutStatus(Guid id, [FromBody] ChangeTaskStatusCommand command)
    {
        return PublishAsync(command with { Id = id, UserCommandContext = UserContext, CorrelationId = Guid.NewGuid() }, EntityType.Task);
    }

    /// <summary>Assigns the task to a member of the project's team; a null assignee unassigns it.</summary>
    [HttpPut("{id:guid}/assignee")]
    [Claim(ClaimTypes.TasksAssign)]
    public Task<AcceptedResult> PutAssignee(Guid id, [FromBody] AssignTaskCommand command)
    {
        return PublishAsync(command with { Id = id, UserCommandContext = UserContext, CorrelationId = Guid.NewGuid() }, EntityType.Task);
    }

    [HttpDelete("{id:guid}")]
    [Claim(ClaimTypes.TasksManage)]
    public Task<AcceptedResult> Delete(Guid id)
    {
        return PublishAsync(new DeleteTaskCommand(id) { UserCommandContext = UserContext, CorrelationId = Guid.NewGuid() }, EntityType.Task);
    }
}
