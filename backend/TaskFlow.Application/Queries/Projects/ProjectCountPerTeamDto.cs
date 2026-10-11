namespace TaskFlow.Application.Queries.Projects;

public sealed record ProjectCountPerTeamDto
{
    public Guid TeamId { get; init; }
    public string TeamName { get; init; } = string.Empty;
    public int ProjectCount { get; init; }
}
