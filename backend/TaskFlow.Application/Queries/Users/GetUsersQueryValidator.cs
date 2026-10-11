using FluentValidation;
using TaskFlow.Application.Common.Filtering;

namespace TaskFlow.Application.Queries.Users;

public sealed class GetUsersQueryValidator : FilterParamsValidator<GetUsersQuery>
{
    public GetUsersQueryValidator() : base(UserSortFields.All)
    {
        RuleFor(query => query.RoleId)
            .NotEqual(Guid.Empty)
            .WithMessage("The role filter must be a valid role id.");
    }
}
