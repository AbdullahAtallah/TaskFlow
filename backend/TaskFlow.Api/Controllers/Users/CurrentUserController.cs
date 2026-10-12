using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Interfaces.DataReaders;
using TaskFlow.Application.Queries.Users;
using TaskFlow.Domain.Constants;
using TaskFlow.Infrastructure.Middleware.Attributes;
using TaskFlow.Infrastructure.Services;

namespace TaskFlow.Api.Controllers.Users;

[Route("api/user")]
[ApiController]
[Authorization]
public class CurrentUserController(
    ICurrentUserRetriever currentUserRetriever,
    IUserDataReader userDataReader) : ControllerBase
{
    /// <summary>The signed-in user with their roles, packages, features and claims: what the frontend shows and hides.</summary>
    [HttpGet("current")]
    public ActionResult GetCurrent()
    {
        return Ok(currentUserRetriever.GetAuthenticatedCurrentUser());
    }

    /// <summary>Active users for the member picker and the assignee picker (?teamId= for the members of a team).</summary>
    [HttpGet("select-list")]
    [Claim(ClaimTypes.TeamsManage, ClaimTypes.TeamsMembersManage, ClaimTypes.TasksAssign)]
    public async Task<ActionResult> GetSelectList([FromQuery] GetUserSelectListQuery filter)
    {
        return Ok(await userDataReader.GetSelectListAsync(filter));
    }
}
