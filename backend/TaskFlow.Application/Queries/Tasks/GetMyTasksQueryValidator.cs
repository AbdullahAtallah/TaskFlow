using TaskFlow.Application.Common.Filtering;

namespace TaskFlow.Application.Queries.Tasks;

public sealed class GetMyTasksQueryValidator() : FilterParamsValidator<GetMyTasksQuery>(TaskSortFields.All);
