using FluentValidation;
using TaskFlow.Application.Common.Validation;
using TaskFlow.Domain.Constants;

namespace TaskFlow.Application.Commands.Teams.UpdateTeam;

public sealed class UpdateTeamCommandValidator : AbstractValidator<UpdateTeamCommand>
{
    public UpdateTeamCommandValidator()
    {
        RuleFor(command => command.Id).RequiredId("Team");

        RuleFor(command => command.Name).RequiredText("Name", FieldLengths.TeamName);

        RuleFor(command => command.Description).OptionalText("Description", FieldLengths.Description);

        RuleFor(command => command.RowVersion).RequiredRowVersion();
    }
}
