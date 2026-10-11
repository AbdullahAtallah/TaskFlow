using FluentValidation;
using TaskFlow.Application.Common.Validation;
using TaskFlow.Domain.Constants;

namespace TaskFlow.Application.Commands.Projects.UpdateProject;

public sealed class UpdateProjectCommandValidator : AbstractValidator<UpdateProjectCommand>
{
    public UpdateProjectCommandValidator()
    {
        RuleFor(command => command.Id).RequiredId("Project");

        RuleFor(command => command.Name).RequiredText("Name", FieldLengths.ProjectName);

        RuleFor(command => command.Description).OptionalText("Description", FieldLengths.Description);

        RuleFor(command => command.RowVersion).RequiredRowVersion();
    }
}
