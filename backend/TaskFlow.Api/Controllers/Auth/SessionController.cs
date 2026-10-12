using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Commands.Auth.Logout;
using TaskFlow.Infrastructure.Middleware.Attributes;
using TaskFlow.Infrastructure.Services;

namespace TaskFlow.Api.Controllers.Auth;

[Route("api/auth")]
[ApiController]
[Authorization]
public class SessionController(LogoutCommandHandler logoutCommandHandler, ICurrentUserRetriever currentUserRetriever) : ControllerBase
{
    /// <summary>Ends every session of the signed-in user: all their tokens are refused from now on.</summary>
    [HttpPost("logout")]
    public async Task<ActionResult> Logout()
    {
        await logoutCommandHandler.Execute(new LogoutCommand(currentUserRetriever.GetAuthenticatedCurrentUser().Id));

        return NoContent();
    }
}
