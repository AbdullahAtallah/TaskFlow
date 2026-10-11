using Microsoft.AspNetCore.SignalR;
using TaskFlow.Infrastructure.Identity;

namespace TaskFlow.Infrastructure.SignalR;

/// <summary>Identifies a SignalR connection by the user id (the sub claim) in its access token.</summary>
internal sealed class TokenUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
    {
        return connection.User is null ? null : AccessTokenClaims.ReadUserId(connection.User)?.ToString();
    }
}
