using TaskFlow.Application.Common;
using TaskFlow.Application.Common.Interfaces;

namespace TaskFlow.Application.Commands.Projects.DeleteProject;

public sealed record DeleteProjectCommand(Guid Id) : IUserCommand
{
    public UserCommandContext UserCommandContext { get; init; } = null!;
    public Guid CorrelationId { get; init; }
}
