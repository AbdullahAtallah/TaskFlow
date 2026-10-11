namespace TaskFlow.Domain.Constants;

/// <summary>
/// Optional parts of a package. The seeder keeps the Features table in sync with <see cref="All"/>.
/// </summary>
public static class FeatureTypes
{
    public const string TasksBoard = "Tasks.Board";

    public static IReadOnlyList<(string PackageId, string Id)> All { get; } =
    [
        (PackageTypes.Tasks, TasksBoard)
    ];
}
