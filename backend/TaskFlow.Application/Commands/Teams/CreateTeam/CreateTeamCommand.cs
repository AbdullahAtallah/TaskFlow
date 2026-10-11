using TaskFlow.Application.Common;
using TaskFlow.Application.Common.Interfaces;

namespace TaskFlow.Application.Commands.Teams.CreateTeam;

public sealed record CreateTeamCommand(
    string Name,
    string? Description,
    List<Guid>? MemberIds) : IUserCommand
{
    public UserCommandContext UserCommandContext { get; init; } = null!;
    public Guid CorrelationId { get; init; }
}
