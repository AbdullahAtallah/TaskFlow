using FluentValidation;
using TaskFlow.Application.Commands.Tasks.Common;
using TaskFlow.Application.Common.Validation;

namespace TaskFlow.Application.Commands.Tasks.UpdateTask;

public sealed class UpdateTaskCommandValidator : AbstractValidator<UpdateTaskCommand>
{
    public UpdateTaskCommandValidator()
    {
        RuleFor(command => command.Id).RequiredId("Task");

        this.AddTaskFieldRules();

        RuleFor(command => command.RowVersion).RequiredRowVersion();
    }
}
