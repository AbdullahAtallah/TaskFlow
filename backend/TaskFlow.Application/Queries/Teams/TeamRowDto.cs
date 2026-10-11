namespace TaskFlow.Application.Queries.Teams;

public sealed record TeamRowDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int MemberCount { get; init; }
    public int ProjectCount { get; init; }
    public DateTime DateTimeCreated { get; init; }

    /// <summary>Send this back with an edit: a save over a newer change is then refused.</summary>
    public byte[] RowVersion { get; init; } = [];
}
