using Microsoft.AspNetCore.Http;
using TaskFlow.Application.Identity;
using TaskFlow.Infrastructure.Identity;

namespace TaskFlow.Infrastructure.Services;

internal sealed class CurrentUserRetriever(IHttpContextAccessor httpContextAccessor) : ICurrentUserRetriever
{
    public CurrentUser GetAuthenticatedCurrentUser()
    {
        return httpContextAccessor.HttpContext?.Items[CurrentAuthContextResolver.HttpContextItemKey] as CurrentUser
               ?? throw new UnauthorizedAccessException();
    }
}
