using FluentValidation;
using TaskFlow.Application.Commands.Tasks.Common;
using TaskFlow.Application.Common.Validation;

namespace TaskFlow.Application.Commands.Tasks.CreateTask;

public sealed class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommand>
{
    public CreateTaskCommandValidator()
    {
        RuleFor(command => command.ProjectId).RequiredId("Project");

        this.AddTaskFieldRules();
    }
}
