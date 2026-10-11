using FluentValidation;
using TaskFlow.Application.Common.Filtering;

namespace TaskFlow.Application.Queries.Projects;

public sealed class GetProjectsQueryValidator : FilterParamsValidator<GetProjectsQuery>
{
    public GetProjectsQueryValidator() : base(ProjectSortFields.All)
    {
        RuleFor(query => query.TeamId)
            .NotEqual(Guid.Empty)
            .WithMessage("The team filter must be a valid team id.");
    }
}
