using Microsoft.AspNetCore.Mvc;
using TaskFlow.Infrastructure.Filters;

namespace TaskFlow.Infrastructure.Middleware.Attributes;

/// <summary>Placed on an action: the user needs at least one of the listed claims.</summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class ClaimAttribute : TypeFilterAttribute
{
    public ClaimAttribute(params string[] claimIds)
        : base(typeof(ClaimAuthorizationFilter))
    {
        Arguments = [claimIds];

        // Same order as [Authorization]; the controller-level filter still runs first.
        Order = AuthorizationAttribute.FilterOrder;
    }
}
