using TaskFlow.Application.Common;
using TaskFlow.Application.Common.Interfaces;

namespace TaskFlow.Application.Commands.Users.UpdateUserRoles;

/// <summary>Replaces the roles of a user; permissions come only from roles.</summary>
public sealed record UpdateUserRolesCommand(
    Guid UserId,
    List<Guid> RoleIds) : IUserCommand
{
    public UserCommandContext UserCommandContext { get; init; } = null!;
    public Guid CorrelationId { get; init; }
}
