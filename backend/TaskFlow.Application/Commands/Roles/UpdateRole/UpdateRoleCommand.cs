using TaskFlow.Application.Commands.Roles.Common;
using TaskFlow.Application.Common;
using TaskFlow.Application.Common.Interfaces;

namespace TaskFlow.Application.Commands.Roles.UpdateRole;

/// <summary>
/// Replaces the packages and claims of a seeded role. The roles themselves are not renamed, created or deleted, and the
/// latest change wins: the role details page saves every switch on its own.
/// </summary>
public sealed record UpdateRoleCommand(
    Guid Id,
    List<string> Packages,
    List<string> Claims) : IUserCommand, IRolePermissionsCommand
{
    public UserCommandContext UserCommandContext { get; init; } = null!;
    public Guid CorrelationId { get; init; }
}
