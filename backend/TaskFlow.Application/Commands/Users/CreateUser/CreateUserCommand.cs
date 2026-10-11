using System.Text;

namespace TaskFlow.Application.Commands.Users.CreateUser;

/// <summary>
/// Creates a user with an initial password and roles. POST /api/user calls its handler directly: the command carries a
/// password, so it is never a message and never reaches the message inbox in the database.
/// </summary>
public sealed record CreateUserCommand(
    string UserName,
    string DisplayName,
    string Password,
    List<Guid> RoleIds)
{
    /// <summary>Leaves the password out of ToString(), and so out of every log message.</summary>
    private bool PrintMembers(StringBuilder builder)
    {
        builder.Append($"UserName = {UserName}, DisplayName = {DisplayName}, RoleIds = [{string.Join(", ", RoleIds ?? [])}]");
        return true;
    }
}
