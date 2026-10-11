using TaskFlow.Application.Common;
using TaskFlow.Application.Common.Interfaces;

namespace TaskFlow.Application.Commands.Users.DeleteUser;

/// <summary>
/// Soft-deletes the user and removes their roles, team memberships and task assignments. Their tokens stop working at
/// once, and the e-mail address can be registered again as a new account; deactivate a user to block access instead.
/// </summary>
public sealed record DeleteUserCommand(Guid Id) : IUserCommand
{
    public UserCommandContext UserCommandContext { get; init; } = null!;
    public Guid CorrelationId { get; init; }
}
