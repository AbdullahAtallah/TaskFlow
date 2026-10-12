using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Interfaces.DataReaders;
using TaskFlow.Application.Queries.Roles;
using TaskFlow.Domain.Constants;
using TaskFlow.Infrastructure.Middleware.Attributes;

namespace TaskFlow.Api.Controllers.Roles;

[Route("api/role")]
[ApiController]
[Authorization(PackageTypes.Management)]
public class RoleQueryController(IRoleDataReader roleDataReader) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult> Get([FromQuery] GetRolesQuery filter)
    {
        return Ok(await roleDataReader.GetGridAsync(filter));
    }

    /// <summary>A single role with its packages and claims.</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult> Get(Guid id)
    {
        return Ok(await roleDataReader.GetByIdAsync(id));
    }

    /// <summary>The users assigned to the role.</summary>
    [HttpGet("{id:guid}/user")]
    public async Task<ActionResult> GetUsers(Guid id, [FromQuery] GetRoleUsersQuery filter)
    {
        return Ok(await roleDataReader.GetUsersAsync(id, filter));
    }
}
