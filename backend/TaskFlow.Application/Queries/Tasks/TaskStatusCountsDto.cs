namespace TaskFlow.Application.Queries.Tasks;

/// <summary>Dashboard "Tasks per status"; overdue tasks are also counted in their own status.</summary>
public sealed record TaskStatusCountsDto
{
    public int ToDo { get; init; }
    public int InProgress { get; init; }
    public int Done { get; init; }
    public int Overdue { get; init; }
}
