using TaskFlow.Application.Common.Filtering;

namespace TaskFlow.Application.Queries.Roles;

public sealed class GetRolesQuery : FilterParams;

public static class RoleSortFields
{
    public const string Name = "name";
    public const string UserCount = "userCount";

    public static IReadOnlyList<string> All { get; } = [Name, UserCount];
}
