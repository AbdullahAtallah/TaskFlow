using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Constants;
using TaskFlow.Domain.Entities.Authorization;

namespace TaskFlow.Application.Commands.Roles.Common;

public sealed class RolePermissionsUpdater(
    IRepository<RoleEntity> roleRepository,
    IRepository<RolePackageEntity> rolePackageRepository,
    IRepository<RoleClaimEntity> roleClaimRepository)
{
    /// <summary>
    /// Replaces the packages and claims of the role; the General package is always granted. Returns whether any grant
    /// was added or removed.
    /// </summary>
    public async Task<bool> ReplaceAsync(Guid roleId, IReadOnlyCollection<string> packageIds, IReadOnlyCollection<string> claimIds)
    {
        var packages = packageIds.Append(PackageTypes.General).Distinct().ToList();

        var currentPackages = await rolePackageRepository.FindAsync(rolePackage => rolePackage.RoleId == roleId);
        var removedPackages = currentPackages.Where(rolePackage => !packages.Contains(rolePackage.PackageId)).ToList();
        var addedPackageIds = packages.Except(currentPackages.Select(rolePackage => rolePackage.PackageId)).ToList();

        rolePackageRepository.DeleteRange(removedPackages);
        foreach (var packageId in addedPackageIds)
        {
            await rolePackageRepository.AddAsync(new RolePackageEntity { RoleId = roleId, PackageId = packageId });
        }

        var currentClaims = await roleClaimRepository.FindAsync(roleClaim => roleClaim.RoleId == roleId);
        var removedClaims = currentClaims.Where(roleClaim => !claimIds.Contains(roleClaim.ClaimId)).ToList();
        var addedClaimIds = claimIds.Except(currentClaims.Select(roleClaim => roleClaim.ClaimId)).ToList();

        roleClaimRepository.DeleteRange(removedClaims);
        foreach (var claimId in addedClaimIds)
        {
            await roleClaimRepository.AddAsync(new RoleClaimEntity { RoleId = roleId, ClaimId = claimId });
        }

        return removedPackages.Count + addedPackageIds.Count + removedClaims.Count + addedClaimIds.Count > 0;
    }

    /// <summary>Makes this the only role that newly registered users receive.</summary>
    public async Task MakeDefaultAsync(RoleEntity role)
    {
        var otherDefaultRoles = await roleRepository.FindAsync(otherRole => otherRole.IsDefault && otherRole.Id != role.Id);

        foreach (var otherRole in otherDefaultRoles)
        {
            otherRole.IsDefault = false;
        }

        role.IsDefault = true;
    }
}
