using System.Text;

namespace TaskFlow.Application.Commands.Users.SetUserPassword;

/// <summary>
/// An administrator sets a new password for a user, for example when they forgot theirs. It also ends all sessions of
/// that user. PUT /api/user/{id}/password calls its handler directly: the command carries a password, so it is never a
/// message.
/// </summary>
public sealed record SetUserPasswordCommand(Guid UserId, string Password)
{
    /// <summary>Leaves the password out of ToString(), and so out of every log message.</summary>
    private bool PrintMembers(StringBuilder builder)
    {
        builder.Append($"UserId = {UserId}");
        return true;
    }
}
