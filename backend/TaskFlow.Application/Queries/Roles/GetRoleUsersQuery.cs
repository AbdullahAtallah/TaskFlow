using TaskFlow.Application.Common.Filtering;

namespace TaskFlow.Application.Queries.Roles;

/// <summary>The users assigned to a role (role details, "Assigned users" tab).</summary>
public sealed class GetRoleUsersQuery : FilterParams;

public static class RoleUserSortFields
{
    public const string DisplayName = "displayName";
    public const string UserName = "userName";
    public const string IsActive = "isActive";

    public static IReadOnlyList<string> All { get; } = [DisplayName, UserName, IsActive];
}
