namespace TaskFlow.Domain.Constants;

/// <summary>
/// Single permissions, named Package.[Feature.]Entity.Action. The seeder keeps the Claims table in sync with <see cref="All"/>.
/// </summary>
public static class ClaimTypes
{
    public const string TeamsViewAll = "Teams.View.All";
    public const string TeamsManage = "Teams.Manage";
    public const string TeamsMembersManage = "Teams.Members.Manage";

    public const string ProjectsViewAll = "Projects.View.All";
    public const string ProjectsCreate = "Projects.Create";
    public const string ProjectsManage = "Projects.Manage";

    public const string TasksViewAll = "Tasks.View.All";
    public const string TasksCreate = "Tasks.Create";
    public const string TasksManage = "Tasks.Manage";
    public const string TasksAssign = "Tasks.Assign";
    public const string TasksBoardView = "Tasks.Board.View";

    public static IReadOnlyList<(string PackageId, string Id, string? FeatureId)> TeamsPackageClaims { get; } =
    [
        (PackageTypes.Teams, TeamsViewAll, null),
        (PackageTypes.Teams, TeamsManage, null),
        (PackageTypes.Teams, TeamsMembersManage, null)
    ];

    public static IReadOnlyList<(string PackageId, string Id, string? FeatureId)> ProjectsPackageClaims { get; } =
    [
        (PackageTypes.Projects, ProjectsViewAll, null),
        (PackageTypes.Projects, ProjectsCreate, null),
        (PackageTypes.Projects, ProjectsManage, null)
    ];

    public static IReadOnlyList<(string PackageId, string Id, string? FeatureId)> TasksPackageClaims { get; } =
    [
        (PackageTypes.Tasks, TasksViewAll, null),
        (PackageTypes.Tasks, TasksCreate, null),
        (PackageTypes.Tasks, TasksManage, null),
        (PackageTypes.Tasks, TasksAssign, null),
        (PackageTypes.Tasks, TasksBoardView, FeatureTypes.TasksBoard)
    ];

    public static IReadOnlyList<(string PackageId, string Id, string? FeatureId)> All { get; } =
    [
        ..TeamsPackageClaims,
        ..ProjectsPackageClaims,
        ..TasksPackageClaims
    ];
}
