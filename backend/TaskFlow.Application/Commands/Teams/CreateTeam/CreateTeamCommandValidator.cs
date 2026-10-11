using FluentValidation;
using TaskFlow.Application.Common.Validation;
using TaskFlow.Domain.Constants;

namespace TaskFlow.Application.Commands.Teams.CreateTeam;

public sealed class CreateTeamCommandValidator : AbstractValidator<CreateTeamCommand>
{
    public CreateTeamCommandValidator()
    {
        RuleFor(command => command.Name).RequiredText("Name", FieldLengths.TeamName);

        RuleFor(command => command.Description).OptionalText("Description", FieldLengths.Description);

        RuleForEach(command => command.MemberIds)
            .NotEmpty()
            .WithMessage("Members must be valid user ids.");

        RuleFor(command => command.MemberIds)
            .Must(memberIds => ValidationRules.BeDistinct(memberIds))
            .WithMessage("A user can only be added to a team once.");
    }
}
