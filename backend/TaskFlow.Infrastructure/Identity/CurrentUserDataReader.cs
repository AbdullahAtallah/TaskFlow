using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.DTOs;
using TaskFlow.Application.Identity;
using TaskFlow.Domain.Exceptions;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Infrastructure.Identity;

/// <summary>Loads the current user with the union of the packages, features and claims of all their roles.</summary>
internal sealed class CurrentUserDataReader(TaskFlowDbContext dbContext)
{
    /// <summary>
    /// Null when the user no longer exists or the token's security stamp is outdated (after a logout, a new password or
    /// a deactivation); throws <see cref="AccountRevokedException"/> while the user is deactivated.
    /// </summary>
    public async Task<CurrentUser?> ReadAsync(Guid userId, Guid securityStamp, CancellationToken cancellationToken)
    {
        // One round trip on every request: the grants of a user are a few dozen rows.
        var user = await dbContext.Users
            .AsNoTracking()
            .AsSingleQuery()
            .Where(user => user.Id == userId)
            .Select(user => new
            {
                user.Id,
                user.UserName,
                user.DisplayName,
                user.IsActive,
                user.SecurityStamp,
                Roles = user.UserRoles
                    .OrderBy(userRole => userRole.Role!.Name)
                    .Select(userRole => new EntityReferenceDto(userRole.RoleId, userRole.Role!.Name))
                    .ToList(),
                Packages = user.UserRoles
                    .SelectMany(userRole => userRole.Role!.RolePackages.Select(rolePackage => rolePackage.PackageId))
                    .Distinct()
                    .ToList(),
                Claims = user.UserRoles
                    .SelectMany(userRole => userRole.Role!.RoleClaims.Select(roleClaim => roleClaim.ClaimId))
                    .Distinct()
                    .ToList()
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (user is null)
        {
            return null;
        }

        // Before the stamp: deactivation also replaces the stamp, and an open session should learn why it ended.
        if (!user.IsActive)
        {
            throw new AccountRevokedException();
        }

        if (user.SecurityStamp != securityStamp)
        {
            return null;
        }

        var packages = user.Packages;
        var features = await dbContext.Features
            .AsNoTracking()
            .Where(feature => feature.IsEnabled && packages.Contains(feature.PackageId))
            .Select(feature => feature.Id)
            .ToListAsync(cancellationToken);

        return new CurrentUser(
            user.Id,
            user.UserName,
            user.DisplayName,
            user.Roles,
            new SortedSet<string>(user.Packages, StringComparer.Ordinal),
            new SortedSet<string>(features, StringComparer.Ordinal),
            new SortedSet<string>(user.Claims, StringComparer.Ordinal));
    }
}
