using TaskFlow.Application.Common.Filtering;

namespace TaskFlow.Application.Queries.Roles;

public sealed class GetRolesQueryValidator() : FilterParamsValidator<GetRolesQuery>(RoleSortFields.All);
