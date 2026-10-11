using Microsoft.AspNetCore.Mvc;
using TaskFlow.Api.Shared;
using TaskFlow.Application.Commands.Users.CreateUser;
using TaskFlow.Application.Commands.Users.DeleteUser;
using TaskFlow.Application.Commands.Users.SetUserPassword;
using TaskFlow.Application.Commands.Users.UpdateUser;
using TaskFlow.Application.Commands.Users.UpdateUserRoles;
using TaskFlow.Application.Interfaces;
using TaskFlow.Application.Interfaces.DataReaders;
using TaskFlow.Application.Queries.Users;
using TaskFlow.Domain.Constants;
using TaskFlow.Domain.Enums;
using TaskFlow.Infrastructure.Middleware.Attributes;
using TaskFlow.Infrastructure.Services;
using Wolverine;

namespace TaskFlow.Api.Controllers.Users;

[Route("api/user")]
[ApiController]
[Authorization(PackageTypes.Management)]
public class UserController(
    IMessageBus messageBus,
    ISendNotificationCommandHandler sendNotificationCommandHandler,
    ICurrentUserRetriever currentUserRetriever,
    CreateUserCommandHandler createUserCommandHandler,
    SetUserPasswordCommandHandler setUserPasswordCommandHandler,
    IUserDataReader userDataReader)
    : CommandControllerBase(messageBus, sendNotificationCommandHandler, currentUserRetriever)
{
    /// <summary>
    /// Creates a user with an initial password and roles. Answered right away (201 with the new user) instead of with a
    /// notification, because a password is never put on the message bus.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(UserDetailDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<UserDetailDto>> Post([FromBody] CreateUserCommand command)
    {
        var userId = await createUserCommandHandler.Execute(command);

        return Created($"/api/user/{userId}", await userDataReader.GetByIdAsync(userId));
    }

    /// <summary>Updates the display name or the active state; nobody can deactivate their own account.</summary>
    [HttpPut("{id:guid}")]
    public Task<AcceptedResult> Put(Guid id, [FromBody] UpdateUserCommand command)
    {
        return PublishAsync(command with { Id = id, UserCommandContext = UserContext, CorrelationId = Guid.NewGuid() }, EntityType.User);
    }

    /// <summary>
    /// Sets a new password (body: password), for example when the user forgot theirs, and ends all their sessions.
    /// Answered right away (204), because a password is never put on the message bus.
    /// </summary>
    [HttpPut("{id:guid}/password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<NoContentResult> PutPassword(Guid id, [FromBody] SetUserPasswordCommand command)
    {
        await setUserPasswordCommandHandler.Execute(command with { UserId = id });

        return NoContent();
    }

    /// <summary>Replaces the user's roles (body: roleIds; the user id comes from the route).</summary>
    [HttpPut("{id:guid}/permissions")]
    public Task<AcceptedResult> PutPermissions(Guid id, [FromBody] UpdateUserRolesCommand command)
    {
        return PublishAsync(command with { UserId = id, UserCommandContext = UserContext, CorrelationId = Guid.NewGuid() }, EntityType.User);
    }

    /// <summary>Soft-deletes the user and removes their roles, memberships and task assignments.</summary>
    [HttpDelete("{id:guid}")]
    public Task<AcceptedResult> Delete(Guid id)
    {
        return PublishAsync(new DeleteUserCommand(id) { UserCommandContext = UserContext, CorrelationId = Guid.NewGuid() }, EntityType.User);
    }
}
