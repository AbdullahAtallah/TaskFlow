namespace TaskFlow.Application.Queries.Roles;

public sealed record RoleDetailDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsDefault { get; init; }
    public bool IsBuiltIn { get; init; }
    public IReadOnlyList<string> Packages { get; init; } = [];
    public IReadOnlyList<string> Claims { get; init; } = [];
    public int UserCount { get; init; }
    public DateTime DateTimeCreated { get; init; }
    public DateTime? DateTimeModified { get; init; }

    /// <summary>Send this back with an edit: a save over a newer change is then refused.</summary>
    public byte[] RowVersion { get; init; } = [];
}
