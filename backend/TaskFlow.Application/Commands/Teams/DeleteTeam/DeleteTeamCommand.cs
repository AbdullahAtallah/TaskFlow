using TaskFlow.Application.Common;
using TaskFlow.Application.Common.Interfaces;

namespace TaskFlow.Application.Commands.Teams.DeleteTeam;

public sealed record DeleteTeamCommand(Guid Id) : IUserCommand
{
    public UserCommandContext UserCommandContext { get; init; } = null!;
    public Guid CorrelationId { get; init; }
}
