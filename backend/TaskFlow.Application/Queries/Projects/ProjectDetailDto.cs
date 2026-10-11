namespace TaskFlow.Application.Queries.Projects;

public sealed record ProjectDetailDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public Guid TeamId { get; init; }
    public string TeamName { get; init; } = string.Empty;
    public int TaskCount { get; init; }
    public int OpenTaskCount { get; init; }
    public int DoneTaskCount { get; init; }
    public DateTime DateTimeCreated { get; init; }
    public DateTime? DateTimeModified { get; init; }

    /// <summary>Send this back with an edit: a save over a newer change is then refused.</summary>
    public byte[] RowVersion { get; init; } = [];
}
