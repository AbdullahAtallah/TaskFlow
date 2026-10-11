using System.Text;

namespace TaskFlow.Application.Commands.Auth.Register;

/// <summary>
/// Creates an account with the role for new users and signs it in. POST /api/auth/register calls its handler directly:
/// it is never a message, so the password never reaches the message inbox in the database.
/// </summary>
public sealed record RegisterCommand(string Email, string DisplayName, string Password)
{
    /// <summary>Leaves the password out of ToString(), and so out of every log message.</summary>
    private bool PrintMembers(StringBuilder builder)
    {
        builder.Append($"Email = {Email}, DisplayName = {DisplayName}");
        return true;
    }
}
