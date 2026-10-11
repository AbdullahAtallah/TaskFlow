using Microsoft.AspNetCore.Mvc;
using TaskFlow.Infrastructure.Filters;

namespace TaskFlow.Infrastructure.Middleware.Attributes;

/// <summary>
/// Placed on every controller: resolves the current user and checks the package (and feature) of the controller.
/// Without a package it only requires a resolvable, active user.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class AuthorizationAttribute : TypeFilterAttribute
{
    /// <summary>Before MVC's model validation (-2000), so a user without access gets 403 and never a 400.</summary>
    public const int FilterOrder = -3000;

    public AuthorizationAttribute(string? packageName = null, string? featureName = null)
        : base(typeof(AuthorizationFilter))
    {
        Arguments = [packageName is null ? Array.Empty<string>() : new[] { packageName }, featureName ?? string.Empty];
        Order = FilterOrder;
    }
}
