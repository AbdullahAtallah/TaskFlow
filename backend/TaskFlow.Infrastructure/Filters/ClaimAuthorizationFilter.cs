using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TaskFlow.Infrastructure.Identity;

namespace TaskFlow.Infrastructure.Filters;

/// <summary>The user must hold at least one of the listed claims (else 403).</summary>
internal sealed class ClaimAuthorizationFilter(
    ICurrentAuthContextResolver authContextResolver,
    string[] claimIds) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var currentUser = await authContextResolver.GetUserAsync(context.HttpContext.RequestAborted);

        if (currentUser is null)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        if (!claimIds.Any(currentUser.HasClaim))
        {
            context.Result = new ForbidResult();
            return;
        }

        await next();
    }
}
