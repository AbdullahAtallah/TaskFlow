using TaskFlow.Application.Common.Filtering;

namespace TaskFlow.Application.Queries.Projects;

public sealed class GetProjectsQuery : FilterParams
{
    public Guid? TeamId { get; set; }
}

public static class ProjectSortFields
{
    public const string Name = "name";
    public const string TeamName = "teamName";
    public const string TaskCount = "taskCount";
    public const string OpenTaskCount = "openTaskCount";
    public const string DateTimeCreated = "dateTimeCreated";

    public static IReadOnlyList<string> All { get; } = [Name, TeamName, TaskCount, OpenTaskCount, DateTimeCreated];
}
