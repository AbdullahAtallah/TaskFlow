using Microsoft.AspNetCore.Mvc;
using TaskFlow.Api.Shared;
// using TaskFlow.Application.Commands.Roles.CreateRole;
// using TaskFlow.Application.Commands.Roles.DeleteRole;
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
    // The roles are seeded (Seeds.cs), as in FAMAS: they are not created or deleted through the API.
    // [HttpPost]
    // public Task<ActionResult> Post([FromBody] CreateRoleCommand command)
    // {
    //     return PublishAsync(command with { UserCommandContext = UserContext, CorrelationId = Guid.NewGuid() }, EntityType.Role);
    // }

    /// <summary>Replaces the role's packages and claims; its users get the new permissions on their next request.</summary>
    [HttpPut("{id:guid}")]
    public Task<ActionResult> Put(Guid id, [FromBody] UpdateRoleCommand command)
    {
        return PublishAsync(command with { Id = id, UserCommandContext = UserContext, CorrelationId = Guid.NewGuid() }, EntityType.Role);
    }

    // [HttpDelete("{id:guid}")]
    // public Task<ActionResult> Delete(Guid id)
    // {
    //     return PublishAsync(new DeleteRoleCommand(id) { UserCommandContext = UserContext, CorrelationId = Guid.NewGuid() }, EntityType.Role);
    // }
}
