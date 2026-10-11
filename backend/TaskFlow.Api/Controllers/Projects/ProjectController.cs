using Microsoft.AspNetCore.Mvc;
using TaskFlow.Api.Shared;
using TaskFlow.Application.Commands.Projects.CreateProject;
using TaskFlow.Application.Commands.Projects.DeleteProject;
using TaskFlow.Application.Commands.Projects.UpdateProject;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Constants;
using TaskFlow.Domain.Enums;
using TaskFlow.Infrastructure.Middleware.Attributes;
using TaskFlow.Infrastructure.Services;
using Wolverine;

namespace TaskFlow.Api.Controllers.Projects;

[Route("api/project")]
[ApiController]
[Authorization(PackageTypes.Projects)]
public class ProjectController(
    IMessageBus messageBus,
    ISendNotificationCommandHandler sendNotificationCommandHandler,
    ICurrentUserRetriever currentUserRetriever)
    : CommandControllerBase(messageBus, sendNotificationCommandHandler, currentUserRetriever)
{
    /// <summary>Creates a project in a team inside the user's scope.</summary>
    [HttpPost]
    [Claim(ClaimTypes.ProjectsCreate)]
    public Task<AcceptedResult> Post([FromBody] CreateProjectCommand command)
    {
        return PublishAsync(command with { UserCommandContext = UserContext, CorrelationId = Guid.NewGuid() }, EntityType.Project);
    }

    [HttpPut("{id:guid}")]
    [Claim(ClaimTypes.ProjectsManage)]
    public Task<AcceptedResult> Put(Guid id, [FromBody] UpdateProjectCommand command)
    {
        return PublishAsync(command with { Id = id, UserCommandContext = UserContext, CorrelationId = Guid.NewGuid() }, EntityType.Project);
    }

    /// <summary>Soft-deletes the project and all of its tasks.</summary>
    [HttpDelete("{id:guid}")]
    [Claim(ClaimTypes.ProjectsManage)]
    public Task<AcceptedResult> Delete(Guid id)
    {
        return PublishAsync(new DeleteProjectCommand(id) { UserCommandContext = UserContext, CorrelationId = Guid.NewGuid() }, EntityType.Project);
    }
}
