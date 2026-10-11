namespace TaskFlow.Application.Queries.Users;

public sealed record UserSelectItemDto
{
    public Guid Id { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
}
