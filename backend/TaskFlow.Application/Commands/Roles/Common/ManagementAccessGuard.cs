using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Constants;
using TaskFlow.Domain.Entities.Authorization;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Application.Commands.Roles.Common;

/// <summary>Prevents administrators from locking themselves out of user and role management.</summary>
public sealed class ManagementAccessGuard(IRepository<RolePackageEntity> rolePackageRepository)
{
    public async Task EnsureRolesGrantManagementAsync(IReadOnlyCollection<Guid> roleIds)
    {
        var managementGrants = await rolePackageRepository.FindAsync(rolePackage =>
            roleIds.Contains(rolePackage.RoleId) && rolePackage.PackageId == PackageTypes.Management);

        if (managementGrants.Count == 0)
        {
            throw new TaskFlowException(ErrorMessages.CannotRemoveOwnManagementAccess);
        }
    }
}
