using Microsoft.AspNetCore.Mvc;
using TaskFlow.Api.Shared;
// using TaskFlow.Application.Commands.Tasks.AssignTask;
// using TaskFlow.Application.Commands.Tasks.ChangeTaskStatus;
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
    public Task<ActionResult> Post([FromBody] CreateTaskCommand command)
    {
        return PublishAsync(command with { UserCommandContext = UserContext, CorrelationId = Guid.NewGuid() }, EntityType.Task);
    }

    /// <summary>Updates every field; changing the assignee also requires Tasks.Assign.</summary>
    [HttpPut("{id:guid}")]
    [Claim(ClaimTypes.TasksManage)]
    public Task<ActionResult> Put(Guid id, [FromBody] UpdateTaskCommand command)
    {
        return PublishAsync(command with { Id = id, UserCommandContext = UserContext, CorrelationId = Guid.NewGuid() }, EntityType.Task);
    }

    // Not used by the design: the task dialog saves the status and the assignee with PUT /api/task/{id}.
    // [HttpPut("{id:guid}/status")]
    // [Claim(ClaimTypes.TasksManage)]
    // public Task<ActionResult> PutStatus(Guid id, [FromBody] ChangeTaskStatusCommand command)
    // {
    //     return PublishAsync(command with { Id = id, UserCommandContext = UserContext, CorrelationId = Guid.NewGuid() }, EntityType.Task);
    // }
    //
    // [HttpPut("{id:guid}/assignee")]
    // [Claim(ClaimTypes.TasksAssign)]
    // public Task<ActionResult> PutAssignee(Guid id, [FromBody] AssignTaskCommand command)
    // {
    //     return PublishAsync(command with { Id = id, UserCommandContext = UserContext, CorrelationId = Guid.NewGuid() }, EntityType.Task);
    // }

    [HttpDelete("{id:guid}")]
    [Claim(ClaimTypes.TasksManage)]
    public Task<ActionResult> Delete(Guid id)
    {
        return PublishAsync(new DeleteTaskCommand(id) { UserCommandContext = UserContext, CorrelationId = Guid.NewGuid() }, EntityType.Task);
    }
}
