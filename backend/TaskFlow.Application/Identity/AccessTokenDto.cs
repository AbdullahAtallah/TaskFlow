namespace TaskFlow.Application.Identity;

/// <summary>The answer to register and login. Send the token with every request: "Authorization: Bearer {AccessToken}".</summary>
public sealed record AccessTokenDto(string AccessToken, string TokenType, DateTime ExpiresAt);
