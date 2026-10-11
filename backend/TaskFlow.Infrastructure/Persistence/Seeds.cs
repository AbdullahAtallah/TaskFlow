using TaskFlow.Domain.Constants;
using TaskFlow.Domain.Entities.Authorization;

namespace TaskFlow.Infrastructure.Persistence;

/// <summary>
/// The rows every TaskFlow database starts with: the permission catalogue and the three default roles with their grants.
/// The entity configurations hand them to EF Core with HasData, so the migrations insert, update and delete them. After
/// changing a constant in TaskFlow.Domain/Constants or a role below, add a migration.
/// </summary>
internal static class Seeds
{
    /// <summary>Fixed, so a seeded row is the same in every database and its migrations never change it by accident.</summary>
    private static readonly DateTime SeededAt = new(2026, 10, 11, 0, 0, 0, DateTimeKind.Utc);

    internal static List<PackageEntity> GetPackageSeed()
    {
        return PackageTypes.All.Select(packageId => new PackageEntity { Id = packageId }).ToList();
    }

    internal static List<FeatureEntity> GetFeatureSeed()
    {
        return FeatureTypes.All
            .Select(feature => new FeatureEntity { Id = feature.Id, PackageId = feature.PackageId, IsEnabled = true })
            .ToList();
    }

    internal static List<ClaimEntity> GetClaimSeed()
    {
        return ClaimTypes.All
            .Select(claim => new ClaimEntity { Id = claim.Id, PackageId = claim.PackageId, FeatureId = claim.FeatureId })
            .ToList();
    }

    internal static List<RoleEntity> GetRoleSeed()
    {
        return [GetAdminRoleSeed(), GetUserRoleSeed(), GetReadOnlyUserRoleSeed()];
    }

    internal static RoleEntity GetAdminRoleSeed()
    {
        return RoleSeed(new Guid("039d8370-1848-4397-b389-34fff78a9ec0"), DefaultRoleBundles.Admin);
    }

    internal static RoleEntity GetUserRoleSeed()
    {
        return RoleSeed(new Guid("50b3a389-3edf-4444-89c3-5e96b8887da0"), DefaultRoleBundles.User);
    }

    internal static RoleEntity GetReadOnlyUserRoleSeed()
    {
        return RoleSeed(new Guid("f12cc9d1-75eb-4735-ba0e-1b126f39cbd3"), DefaultRoleBundles.ReadOnlyUser);
    }

    /// <summary>The packages of each default role, from its bundle; every bundle includes General.</summary>
    internal static List<RolePackageEntity> GetRolePackageSeed()
    {
        return SeededRoles()
            .SelectMany(seeded => seeded.Bundle.PackageIds
                .Select(packageId => new RolePackageEntity { RoleId = seeded.RoleId, PackageId = packageId }))
            .ToList();
    }

    internal static List<RoleClaimEntity> GetRoleClaimSeed()
    {
        return SeededRoles()
            .SelectMany(seeded => seeded.Bundle.ClaimIds
                .Select(claimId => new RoleClaimEntity { RoleId = seeded.RoleId, ClaimId = claimId }))
            .ToList();
    }

    private static IEnumerable<(Guid RoleId, DefaultRoleBundle Bundle)> SeededRoles()
    {
        yield return (GetAdminRoleSeed().Id, DefaultRoleBundles.Admin);
        yield return (GetUserRoleSeed().Id, DefaultRoleBundles.User);
        yield return (GetReadOnlyUserRoleSeed().Id, DefaultRoleBundles.ReadOnlyUser);
    }

    private static RoleEntity RoleSeed(Guid id, DefaultRoleBundle bundle)
    {
        return new RoleEntity
        {
            Id = id,
            Name = bundle.RoleName,
            Description = bundle.Description,
            IsDefault = bundle.IsDefaultForNewUsers,
            DateTimeCreated = SeededAt
        };
    }
}
