namespace TaskFlow.Application.Queries.Roles;

public sealed record RoleUserDto
{
    public Guid Id { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public bool IsActive { get; init; }
}
