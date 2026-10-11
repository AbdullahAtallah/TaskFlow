namespace TaskFlow.Application.Queries.Users;

/// <summary>
/// Active users for the member and assignee pickers. With a team id: the members of that team.
/// Without one: every user for team managers, otherwise the users who share a team with the current user.
/// </summary>
public sealed class GetUserSelectListQuery
{
    public const int DefaultTake = 50;

    public Guid? TeamId { get; set; }
    public string? Query { get; set; }
    public int Take { get; set; } = DefaultTake;
}
