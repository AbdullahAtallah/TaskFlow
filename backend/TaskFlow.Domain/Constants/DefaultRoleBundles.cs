namespace TaskFlow.Domain.Constants;

public sealed record DefaultRoleBundle(
    string RoleName,
    string Description,
    bool IsDefaultForNewUsers,
    IReadOnlyList<string> PackageIds,
    IReadOnlyList<string> ClaimIds);

/// <summary>
/// The roles the seeder creates, mirroring Spinnr's DefaultRoleBundles. Default roles cannot be deleted or renamed.
/// </summary>
public static class DefaultRoleBundles
{
    public const string AdminRoleName = "Admin";
    public const string UserRoleName = "User";
    public const string ReadOnlyUserRoleName = "ReadOnlyUser";

    private const string ViewClaimMarker = ".View";
    private const string ViewAllClaimSuffix = ".View.All";

    private static readonly string[] TeamAdministrationClaims =
    [
        ClaimTypes.TeamsManage,
        ClaimTypes.TeamsMembersManage
    ];

    public static DefaultRoleBundle Admin { get; } = new(
        AdminRoleName,
        "Full access, including users and roles",
        IsDefaultForNewUsers: false,
        PackageTypes.All,
        AllClaimIds().ToList());

    public static DefaultRoleBundle User { get; } = new(
        UserRoleName,
        "Works in teams, projects and tasks",
        IsDefaultForNewUsers: true,
        PackagesWithoutManagement(),
        AllClaimIds()
            .Where(claimId => !TeamAdministrationClaims.Contains(claimId))
            .Where(claimId => !claimId.EndsWith(ViewAllClaimSuffix, StringComparison.Ordinal))
            .ToList());

    public static DefaultRoleBundle ReadOnlyUser { get; } = new(
        ReadOnlyUserRoleName,
        "Read-only access to teams, projects and tasks",
        IsDefaultForNewUsers: false,
        PackagesWithoutManagement(),
        AllClaimIds()
            .Where(claimId => claimId.Contains(ViewClaimMarker, StringComparison.Ordinal))
            .ToList());

    public static IReadOnlyList<DefaultRoleBundle> All { get; } =
    [
        Admin,
        User,
        ReadOnlyUser
    ];

    public static IReadOnlyList<string> RoleNames { get; } = All.Select(bundle => bundle.RoleName).ToList();

    public static bool IsDefaultRoleName(string? roleName)
    {
        return RoleNames.Contains(roleName, StringComparer.OrdinalIgnoreCase);
    }

    private static IEnumerable<string> AllClaimIds()
    {
        return ClaimTypes.All.Select(claim => claim.Id);
    }

    private static IReadOnlyList<string> PackagesWithoutManagement()
    {
        return PackageTypes.All
            .Where(packageId => packageId != PackageTypes.Management)
            .ToList();
    }
}
