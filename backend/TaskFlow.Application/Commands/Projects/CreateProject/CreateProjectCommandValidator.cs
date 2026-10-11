using FluentValidation;
using TaskFlow.Application.Common.Validation;
using TaskFlow.Domain.Constants;

namespace TaskFlow.Application.Commands.Projects.CreateProject;

public sealed class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(command => command.TeamId).RequiredId("Team");

        RuleFor(command => command.Name).RequiredText("Name", FieldLengths.ProjectName);

        RuleFor(command => command.Description).OptionalText("Description", FieldLengths.Description);
    }
}
