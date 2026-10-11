using FluentValidation;
using TaskFlow.Application.Common.Validation;

namespace TaskFlow.Application.Commands.Tasks.AssignTask;

public sealed class AssignTaskCommandValidator : AbstractValidator<AssignTaskCommand>
{
    public AssignTaskCommandValidator()
    {
        RuleFor(command => command.Id).RequiredId("Task");

        RuleFor(command => command.AssigneeId)
            .NotEqual(Guid.Empty)
            .WithMessage("The assignee must be a valid user id.");
    }
}
