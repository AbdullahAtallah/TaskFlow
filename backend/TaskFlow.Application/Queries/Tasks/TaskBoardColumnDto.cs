using TaskStatus = TaskFlow.Domain.Enums.TaskStatus;

namespace TaskFlow.Application.Queries.Tasks;

public sealed record TaskBoardColumnDto
{
    public TaskStatus Status { get; init; }
    public int Count { get; init; }
    public IReadOnlyList<TaskCardDto> Tasks { get; init; } = [];
}
