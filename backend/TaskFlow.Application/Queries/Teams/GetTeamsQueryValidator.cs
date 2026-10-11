using TaskFlow.Application.Common.Filtering;

namespace TaskFlow.Application.Queries.Teams;

public sealed class GetTeamsQueryValidator() : FilterParamsValidator<GetTeamsQuery>(TeamSortFields.All);
