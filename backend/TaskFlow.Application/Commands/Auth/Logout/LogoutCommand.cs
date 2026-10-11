namespace TaskFlow.Application.Commands.Auth.Logout;

/// <summary>
/// Ends every session of the user by replacing their security stamp: all their tokens are refused from now on.
/// POST /api/auth/logout calls its handler directly.
/// </summary>
public sealed record LogoutCommand(Guid UserId);
