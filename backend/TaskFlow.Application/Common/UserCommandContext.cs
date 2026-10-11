using TaskFlow.Application.Common.Interfaces;

namespace TaskFlow.Application.Common;

/// <summary>
/// The user who sent a command, captured when the request was authorized. Handlers run in the background,
/// so they apply the scope rules with these claims instead of the HTTP context.
/// </summary>
public sealed record UserCommandContext(Guid Id, string UserName, IReadOnlyList<string> Claims) : IUserScope
{
    public bool HasClaim(string claimId)
    {
        return Claims.Contains(claimId);
    }
}
