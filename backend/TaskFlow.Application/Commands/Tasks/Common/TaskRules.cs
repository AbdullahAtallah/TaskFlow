using FluentValidation;
using TaskFlow.Application.Common.Validation;
using TaskFlow.Domain.Constants;

namespace TaskFlow.Application.Commands.Tasks.Common;

public static class TaskRules
{
    public static void AddTaskFieldRules<T>(this AbstractValidator<T> validator) where T : ITaskFieldsCommand
    {
        validator.RuleFor(command => command.Title).RequiredText("Title", FieldLengths.TaskTitle);

        validator.RuleFor(command => command.Description).OptionalText("Description", FieldLengths.Description);

        validator.RuleFor(command => command.Status)
            .IsInEnum()
            .WithMessage("Status must be ToDo, InProgress or Done.");

        validator.RuleFor(command => command.Priority)
            .IsInEnum()
            .WithMessage("Priority must be Low, Medium or High.");

        validator.RuleFor(command => command.AssigneeId)
            .NotEqual(Guid.Empty)
            .WithMessage("The assignee must be a valid user id.");
    }
}
