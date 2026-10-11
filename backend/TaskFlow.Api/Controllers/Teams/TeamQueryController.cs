using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Common.Filtering;
using TaskFlow.Application.Interfaces.DataReaders;
using TaskFlow.Application.Queries.Teams;
using TaskFlow.Domain.Constants;
using TaskFlow.Infrastructure.Middleware.Attributes;

namespace TaskFlow.Api.Controllers.Teams;

[Route("api/team")]
[ApiController]
[Authorization(PackageTypes.Teams)]
public class TeamQueryController(ITeamDataReader teamDataReader) : ControllerBase
{
    /// <summary>Paged list of the visible teams: the user's teams, or every team with Teams.View.All.</summary>
    [HttpGet]
    public async Task<ActionResult<FilteringResult<TeamRowDto>>> Get([FromQuery] GetTeamsQuery filter)
    {
        return Ok(await teamDataReader.GetGridAsync(filter));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TeamDetailDto>> Get(Guid id)
    {
        return Ok(await teamDataReader.GetByIdAsync(id));
    }

    [HttpGet("{id:guid}/member")]
    public async Task<ActionResult<IReadOnlyList<TeamMemberDto>>> GetMembers(Guid id)
    {
        return Ok(await teamDataReader.GetMembersAsync(id));
    }
}
