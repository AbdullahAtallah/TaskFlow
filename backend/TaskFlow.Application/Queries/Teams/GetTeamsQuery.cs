using TaskFlow.Application.Common.Filtering;

namespace TaskFlow.Application.Queries.Teams;

public sealed class GetTeamsQuery : FilterParams
{
    /// <summary>Only the teams the current user is a member of, even with Teams.View.All (dashboard "My teams").</summary>
    public bool OnlyMine { get; set; }
}

public static class TeamSortFields
{
    public const string Name = "name";
    public const string MemberCount = "memberCount";
    public const string ProjectCount = "projectCount";
    public const string DateTimeCreated = "dateTimeCreated";

    public static IReadOnlyList<string> All { get; } = [Name, MemberCount, ProjectCount, DateTimeCreated];
}
