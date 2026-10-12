using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Interfaces.DataReaders;
using TaskFlow.Application.Queries.Users;
using TaskFlow.Domain.Constants;
using TaskFlow.Infrastructure.Middleware.Attributes;

namespace TaskFlow.Api.Controllers.Users;

[Route("api/user")]
[ApiController]
[Authorization(PackageTypes.Management)]
public class UserQueryController(IUserDataReader userDataReader) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult> Get([FromQuery] GetUsersQuery filter)
    {
        return Ok(await userDataReader.GetGridAsync(filter));
    }

    // Not used by the design: there is no user details page.
    // /// <summary>A single user with roles and teams.</summary>
    // [HttpGet("{id:guid}")]
    // public async Task<ActionResult> Get(Guid id)
    // {
    //     return Ok(await userDataReader.GetByIdAsync(id));
    // }
}
