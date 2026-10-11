using System.Text.Json.Serialization;
using TaskFlow.Application.Commands.Roles.Common;
using TaskFlow.Application.Common;
using TaskFlow.Application.Common.Interfaces;

namespace TaskFlow.Application.Commands.Roles.CreateRole;

public sealed record CreateRoleCommand(
    string Name,
    string? Description,
    [property: JsonRequired] bool IsDefault,
    List<string> Packages,
    List<string> Claims) : IUserCommand, IRoleFieldsCommand
{
    public UserCommandContext UserCommandContext { get; init; } = null!;
    public Guid CorrelationId { get; init; }
}
