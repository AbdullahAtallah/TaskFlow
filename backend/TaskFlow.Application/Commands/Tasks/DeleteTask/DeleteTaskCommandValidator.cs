using FluentValidation;
using TaskFlow.Application.Common.Validation;

namespace TaskFlow.Application.Commands.Tasks.DeleteTask;

public sealed class DeleteTaskCommandValidator : AbstractValidator<DeleteTaskCommand>
{
    public DeleteTaskCommandValidator()
    {
        RuleFor(command => command.Id).RequiredId("Task");
    }
}
