using TaskFlow.Application.Identity;

namespace TaskFlow.Infrastructure.Services;

public interface ICurrentUserRetriever
{
    /// <summary>The user the [Authorization] filter resolved for this request.</summary>
    CurrentUser GetAuthenticatedCurrentUser();
}
