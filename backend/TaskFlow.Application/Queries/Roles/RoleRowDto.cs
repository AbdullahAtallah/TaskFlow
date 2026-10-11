namespace TaskFlow.Application.Queries.Roles;

public sealed record RoleRowDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }

    /// <summary>The role newly registered users receive.</summary>
    public bool IsDefault { get; init; }

    /// <summary>One of the seeded roles Admin, User and ReadOnlyUser; cannot be deleted or renamed.</summary>
    public bool IsBuiltIn { get; init; }

    public int PackageCount { get; init; }
    public int ClaimCount { get; init; }
    public int UserCount { get; init; }

    /// <summary>Send this back with an edit: a save over a newer change is then refused.</summary>
    public byte[] RowVersion { get; init; } = [];
}
