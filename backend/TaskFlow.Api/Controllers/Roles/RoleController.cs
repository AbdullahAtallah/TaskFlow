using Microsoft.AspNetCore.Mvc;
using TaskFlow.Api.Shared;
using TaskFlow.Application.Commands.Roles.CreateRole;
using TaskFlow.Application.Commands.Roles.DeleteRole;
using TaskFlow.Application.Commands.Roles.UpdateRole;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Constants;
using TaskFlow.Domain.Enums;
using TaskFlow.Infrastructure.Middleware.Attributes;
using TaskFlow.Infrastructure.Services;
using Wolverine;

namespace TaskFlow.Api.Controllers.Roles;

[Route("api/role")]
[ApiController]
[Authorization(PackageTypes.Management)]
public class RoleController(
    IMessageBus messageBus,
    ISendNotificationCommandHandler sendNotificationCommandHandler,
    ICurrentUserRetriever currentUserRetriever)
    : CommandControllerBase(messageBus, sendNotificationCommandHandler, currentUserRetriever)
{
    /// <summary>Creates a role (name, description, isDefault, packages, claims); General is always granted.</summary>
    [HttpPost]
    public Task<AcceptedResult> Post([FromBody] CreateRoleCommand command)
    {
        return PublishAsync(command with { UserCommandContext = UserContext, CorrelationId = Guid.NewGuid() }, EntityType.Role);
    }

    /// <summary>Updates a role; its users get the new permissions on their next request.</summary>
    [HttpPut("{id:guid}")]
    public Task<AcceptedResult> Put(Guid id, [FromBody] UpdateRoleCommand command)
    {
        return PublishAsync(command with { Id = id, UserCommandContext = UserContext, CorrelationId = Guid.NewGuid() }, EntityType.Role);
    }

    /// <summary>Deletes a role that is not a default role, not the role for new users and not assigned to anyone.</summary>
    [HttpDelete("{id:guid}")]
    public Task<AcceptedResult> Delete(Guid id)
    {
        return PublishAsync(new DeleteRoleCommand(id) { UserCommandContext = UserContext, CorrelationId = Guid.NewGuid() }, EntityType.Role);
    }
}
