using TaskFlow.Application.Common.Filtering;

namespace TaskFlow.Application.Queries.Users;

public sealed class GetUsersQuery : FilterParams
{
    public Guid? RoleId { get; set; }
    public bool? IsActive { get; set; }
}

public static class UserSortFields
{
    public const string DisplayName = "displayName";
    public const string UserName = "userName";
    public const string IsActive = "isActive";
    public const string DateTimeCreated = "dateTimeCreated";

    public static IReadOnlyList<string> All { get; } = [DisplayName, UserName, IsActive, DateTimeCreated];
}
