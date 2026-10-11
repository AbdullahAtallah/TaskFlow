using FluentValidation;
using TaskFlow.Application.Common.Validation;

namespace TaskFlow.Application.Commands.Teams.AddTeamMembers;

public sealed class AddTeamMembersCommandValidator : AbstractValidator<AddTeamMembersCommand>
{
    public AddTeamMembersCommandValidator()
    {
        RuleFor(command => command.TeamId).RequiredId("Team");

        RuleFor(command => command.UserIds)
            .NotEmpty()
            .WithMessage("Select at least one user.")
            .Must(userIds => ValidationRules.BeDistinct(userIds))
            .WithMessage("A user can only be added to a team once.");

        RuleForEach(command => command.UserIds)
            .NotEmpty()
            .WithMessage("Members must be valid user ids.");
    }
}
