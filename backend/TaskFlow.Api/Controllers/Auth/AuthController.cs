using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TaskFlow.Api.Configurations;
using TaskFlow.Application.Commands.Auth.Login;
using TaskFlow.Application.Commands.Auth.Register;

namespace TaskFlow.Api.Controllers.Auth;

/// <summary>
/// The only endpoints without a token. Both answer with an access token right away instead of a notification, and both
/// are rate limited per client IP address.
/// </summary>
[Route("api/auth")]
[ApiController]
[AllowAnonymous]
[EnableRateLimiting(RateLimitingConfiguration.SignInPolicy)]
public class AuthController(RegisterCommandHandler registerCommandHandler, LoginCommandHandler loginCommandHandler) : ControllerBase
{
    /// <summary>Creates an account with the role for new users and signs it in.</summary>
    [HttpPost("register")]
    public async Task<ActionResult> Register([FromBody] RegisterCommand command)
    {
        return Created("/api/user/current", await registerCommandHandler.Execute(command));
    }

    /// <summary>
    /// Signs in; a wrong e-mail address or password answers 401 invalid_credentials, and too many failed sign-ins for one
    /// address 429 too_many_attempts.
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult> Login([FromBody] LoginCommand command)
    {
        return Ok(await loginCommandHandler.Execute(command));
    }
}
