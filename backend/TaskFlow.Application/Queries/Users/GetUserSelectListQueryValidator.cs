using FluentValidation;
using TaskFlow.Application.Common.Filtering;

namespace TaskFlow.Application.Queries.Users;

public sealed class GetUserSelectListQueryValidator : AbstractValidator<GetUserSelectListQuery>
{
    public GetUserSelectListQueryValidator()
    {
        RuleFor(query => query.TeamId)
            .NotEqual(Guid.Empty)
            .WithMessage("The team filter must be a valid team id.");

        RuleFor(query => query.Query)
            .MaximumLength(FilterLimits.MaxQueryLength)
            .WithMessage($"The search text cannot exceed {FilterLimits.MaxQueryLength} characters.");

        RuleFor(query => query.Take)
            .InclusiveBetween(1, FilterLimits.MaxTake)
            .WithMessage($"Take must be between 1 and {FilterLimits.MaxTake}.");
    }
}
