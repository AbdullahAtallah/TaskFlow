using System.Text.Json.Serialization;
using TaskFlow.Application.Common;
using TaskFlow.Application.Common.Interfaces;

namespace TaskFlow.Application.Commands.Users.UpdateUser;

/// <summary>
/// Updates the display name and the active state; an inactive user gets 401 account_revoked. A request body without
/// isActive answers 400, so a missing value never deactivates anyone. RowVersion is the version the form showed.
/// </summary>
public sealed record UpdateUserCommand(
    Guid Id,
    string DisplayName,
    [property: JsonRequired] bool IsActive,
    [property: JsonRequired] byte[] RowVersion) : IUserCommand
{
    public UserCommandContext UserCommandContext { get; init; } = null!;
    public Guid CorrelationId { get; init; }
}
