namespace TaskFlow.Application.Queries.Teams;

public sealed record TeamMemberDto
{
    public Guid UserId { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public IReadOnlyList<string> Roles { get; init; } = [];
}
