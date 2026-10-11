using TaskFlow.Application.Common;
using TaskFlow.Application.Common.DTOs;
using TaskFlow.Application.Common.Interfaces;

namespace TaskFlow.Application.Identity;

/// <summary>
/// The signed-in user with the union of the packages, features and claims of all their roles.
/// Cached for an hour and returned by GET /api/user/current.
/// </summary>
public sealed record CurrentUser(
    Guid Id,
    string UserName,
    string DisplayName,
    IReadOnlyList<EntityReferenceDto> Roles,
    IReadOnlySet<string> Packages,
    IReadOnlySet<string> Features,
    IReadOnlySet<string> Claims) : IUserScope
{
    public bool HasClaim(string claimId)
    {
        return Claims.Contains(claimId);
    }

    public bool HasPackage(string packageId)
    {
        return Packages.Contains(packageId);
    }

    public bool HasFeature(string featureId)
    {
        return Features.Contains(featureId);
    }

    public UserCommandContext ToCommandContext()
    {
        return new UserCommandContext(Id, UserName, [.. Claims]);
    }
}
