namespace TaskFlow.Domain.Constants;

/// <summary>
/// The packages (modules) of TaskFlow. The seeder keeps the Packages table in sync with <see cref="All"/>.
/// </summary>
public static class PackageTypes
{
    /// <summary>Always granted to every role: dashboard and profile.</summary>
    public const string General = "General";
    public const string Teams = "Teams";
    public const string Projects = "Projects";
    public const string Tasks = "Tasks";

    /// <summary>Administration of users and roles — package only, no claims.</summary>
    public const string Management = "Management";

    public static IReadOnlyList<string> All { get; } =
    [
        General,
        Teams,
        Projects,
        Tasks,
        Management
    ];
}
