using System.Text;

namespace TaskFlow.Application.Commands.Auth.Login;

/// <summary>Signs in with e-mail address and password; POST /api/auth/login calls its handler directly, it is never a message.</summary>
public sealed record LoginCommand(string Email, string Password)
{
    /// <summary>Leaves the password out of ToString(), and so out of every log message.</summary>
    private bool PrintMembers(StringBuilder builder)
    {
        builder.Append($"Email = {Email}");
        return true;
    }
}
