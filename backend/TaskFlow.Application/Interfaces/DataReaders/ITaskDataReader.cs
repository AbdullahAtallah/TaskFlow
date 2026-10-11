using TaskFlow.Application.Common.Filtering;
using TaskFlow.Application.Queries.Tasks;

namespace TaskFlow.Application.Interfaces.DataReaders;

public interface ITaskDataReader
{
    Task<FilteringResult<TaskRowDto>> GetGridAsync(GetTasksQuery query);

    Task<TaskDetailDto> GetByIdAsync(Guid id);

    Task<FilteringResult<TaskRowDto>> GetMyTasksAsync(GetMyTasksQuery query);

    Task<TaskStatusCountsDto> GetStatusCountsAsync();

    Task<TaskBoardDto> GetBoardAsync(GetTaskBoardQuery query);
}
