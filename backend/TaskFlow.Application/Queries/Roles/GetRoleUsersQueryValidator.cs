using TaskFlow.Application.Common.Filtering;

namespace TaskFlow.Application.Queries.Roles;

public sealed class GetRoleUsersQueryValidator() : FilterParamsValidator<GetRoleUsersQuery>(RoleUserSortFields.All);
