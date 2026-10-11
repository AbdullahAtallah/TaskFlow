using TaskFlow.Application.Common.Filtering;
using TaskFlow.Application.Queries.Projects;

namespace TaskFlow.Application.Interfaces.DataReaders;

public interface IProjectDataReader
{
    Task<FilteringResult<ProjectRowDto>> GetGridAsync(GetProjectsQuery query);

    Task<ProjectDetailDto> GetByIdAsync(Guid id);

    Task<IReadOnlyList<ProjectCountPerTeamDto>> GetCountsPerTeamAsync();
}
