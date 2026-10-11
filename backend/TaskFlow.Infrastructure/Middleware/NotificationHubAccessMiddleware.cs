using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using TaskFlow.Domain.Exceptions;
using TaskFlow.Infrastructure.Identity;
using TaskFlow.Infrastructure.SignalR;

namespace TaskFlow.Infrastructure.Middleware;

/// <summary>Only active TaskFlow users may open the notification hub; a valid token alone is not enough.</summary>
public sealed class NotificationHubAccessMiddleware(
    RequestDelegate next,
    ILogger<NotificationHubAccessMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext httpContext, ICurrentAuthContextResolver authContextResolver)
    {
        if (!httpContext.Request.Path.StartsWithSegments(NotificationHubPaths.Hub)
            || httpContext.User.Identity?.IsAuthenticated != true)
        {
            await next(httpContext);
            return;
        }

        if (!await IsActiveUserAsync(httpContext, authContextResolver))
        {
            logger.LogWarning("Refused a notification hub request: the caller is not an active TaskFlow user");
            httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        await next(httpContext);
    }

    private static async Task<bool> IsActiveUserAsync(HttpContext httpContext, ICurrentAuthContextResolver authContextResolver)
    {
        try
        {
            return await authContextResolver.GetUserAsync(httpContext.RequestAborted) is not null;
        }
        catch (TaskFlowException)
        {
            // Deactivated account or unverified e-mail address.
            return false;
        }
    }
}
