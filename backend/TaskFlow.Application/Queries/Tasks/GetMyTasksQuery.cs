using TaskFlow.Application.Common.Filtering;

namespace TaskFlow.Application.Queries.Tasks;

/// <summary>Dashboard: the open tasks assigned to the current user, by default ordered by due date.</summary>
public sealed class GetMyTasksQuery : FilterParams;
