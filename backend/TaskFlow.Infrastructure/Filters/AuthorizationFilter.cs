using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TaskFlow.Application.Identity;
using TaskFlow.Infrastructure.Identity;

namespace TaskFlow.Infrastructure.Filters;

/// <summary>
/// The user must be resolvable (else 401), their roles must grant every package, and the feature, when given,
/// must be switched on and belong to a granted package (else 403).
/// </summary>
internal sealed class AuthorizationFilter(
    ICurrentAuthContextResolver authContextResolver,
    string[] packageNames,
    string featureName) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var currentUser = await authContextResolver.GetUserAsync(context.HttpContext.RequestAborted);

        if (currentUser is null)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        if (!packageNames.All(currentUser.HasPackage) || !HasFeatureWhenRequired(currentUser))
        {
            context.Result = new ForbidResult();
            return;
        }

        await next();
    }

    private bool HasFeatureWhenRequired(CurrentUser currentUser)
    {
        return string.IsNullOrEmpty(featureName) || currentUser.HasFeature(featureName);
    }
}
