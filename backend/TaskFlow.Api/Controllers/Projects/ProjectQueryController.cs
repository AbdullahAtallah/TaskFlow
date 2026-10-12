using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Interfaces.DataReaders;
using TaskFlow.Application.Queries.Projects;
using TaskFlow.Domain.Constants;
using TaskFlow.Infrastructure.Middleware.Attributes;

namespace TaskFlow.Api.Controllers.Projects;

[Route("api/project")]
[ApiController]
[Authorization(PackageTypes.Projects)]
public class ProjectQueryController(IProjectDataReader projectDataReader) : ControllerBase
{
    /// <summary>Paged list of the visible projects: those of the user's teams, or all with Projects.View.All.</summary>
    [HttpGet]
    public async Task<ActionResult> Get([FromQuery] GetProjectsQuery filter)
    {
        return Ok(await projectDataReader.GetGridAsync(filter));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult> Get(Guid id)
    {
        return Ok(await projectDataReader.GetByIdAsync(id));
    }

    /// <summary>Dashboard: the number of projects per visible team.</summary>
    [HttpGet("counts-per-team")]
    public async Task<ActionResult> GetCountsPerTeam()
    {
        return Ok(await projectDataReader.GetCountsPerTeamAsync());
    }
}
