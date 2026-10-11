using TaskFlow.Application.Common;
using TaskFlow.Application.Common.Interfaces;

namespace TaskFlow.Application.Commands.Teams.AddTeamMembers;

public sealed record AddTeamMembersCommand(
    Guid TeamId,
    List<Guid> UserIds) : IUserCommand
{
    public UserCommandContext UserCommandContext { get; init; } = null!;
    public Guid CorrelationId { get; init; }
}
