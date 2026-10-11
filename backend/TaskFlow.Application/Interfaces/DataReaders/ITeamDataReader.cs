using TaskFlow.Application.Common.Filtering;
using TaskFlow.Application.Queries.Teams;

namespace TaskFlow.Application.Interfaces.DataReaders;

public interface ITeamDataReader
{
    Task<FilteringResult<TeamRowDto>> GetGridAsync(GetTeamsQuery query);

    Task<TeamDetailDto> GetByIdAsync(Guid id);

    Task<IReadOnlyList<TeamMemberDto>> GetMembersAsync(Guid teamId);

    Task<bool> IsNameInUseAsync(string name, Guid? excludeTeamId);

    Task<bool> HasProjectsAsync(Guid teamId);
}
