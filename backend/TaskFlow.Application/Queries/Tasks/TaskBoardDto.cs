namespace TaskFlow.Application.Queries.Tasks;

public sealed record TaskBoardDto
{
    public Guid ProjectId { get; init; }
    public string ProjectName { get; init; } = string.Empty;

    /// <summary>One column per status, in the order To do, In progress, Done.</summary>
    public IReadOnlyList<TaskBoardColumnDto> Columns { get; init; } = [];
}
