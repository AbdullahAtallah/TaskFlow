using FluentValidation;
using TaskFlow.Application.Common.Filtering;

namespace TaskFlow.Application.Queries.Tasks;

public sealed class GetTasksQueryValidator : FilterParamsValidator<GetTasksQuery>
{
    public GetTasksQueryValidator() : base(TaskSortFields.All)
    {
        RuleFor(query => query.ProjectId)
            .NotEqual(Guid.Empty)
            .WithMessage("The project filter must be a valid project id.");

        RuleFor(query => query.AssigneeId)
            .NotEqual(Guid.Empty)
            .WithMessage("The assignee filter must be a valid user id.");

        RuleFor(query => query.Status)
            .IsInEnum()
            .WithMessage("The status filter must be ToDo, InProgress or Done.");

        RuleFor(query => query.Priority)
            .IsInEnum()
            .WithMessage("The priority filter must be Low, Medium or High.");

        RuleFor(query => query.Unassigned)
            .Equal(false)
            .When(query => query.AssigneeId.HasValue)
            .WithMessage("Filter on an assignee or on unassigned tasks, not both.");

        RuleFor(query => query.DueTo)
            .GreaterThanOrEqualTo(query => query.DueFrom)
            .When(query => query.DueFrom.HasValue && query.DueTo.HasValue)
            .WithMessage("The end of the due date range cannot lie before its start.");
    }
}
