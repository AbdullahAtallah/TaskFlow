using FluentValidation;
using TaskFlow.Application.Common.Validation;

namespace TaskFlow.Application.Commands.Tasks.ChangeTaskStatus;

public sealed class ChangeTaskStatusCommandValidator : AbstractValidator<ChangeTaskStatusCommand>
{
    public ChangeTaskStatusCommandValidator()
    {
        RuleFor(command => command.Id).RequiredId("Task");

        RuleFor(command => command.Status)
            .IsInEnum()
            .WithMessage("Status must be ToDo, InProgress or Done.");
    }
}
