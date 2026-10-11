using FluentValidation;

namespace TaskFlow.Application.Queries.Tasks;

public sealed class GetTaskBoardQueryValidator : AbstractValidator<GetTaskBoardQuery>
{
    public GetTaskBoardQueryValidator()
    {
        RuleFor(query => query.ProjectId)
            .NotEmpty()
            .WithMessage("The project id is required.");

        RuleFor(query => query.AssigneeId)
            .NotEqual(Guid.Empty)
            .WithMessage("The assignee filter must be a valid user id.");

        RuleFor(query => query.Priority)
            .IsInEnum()
            .WithMessage("The priority filter must be Low, Medium or High.");

        RuleFor(query => query.Unassigned)
            .Equal(false)
            .When(query => query.AssigneeId.HasValue)
            .WithMessage("Filter on an assignee or on unassigned tasks, not both.");
    }
}
