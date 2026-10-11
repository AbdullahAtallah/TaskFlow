using FluentValidation;
using TaskFlow.Application.Common.Validation;

namespace TaskFlow.Application.Commands.Teams.RemoveTeamMember;

public sealed class RemoveTeamMemberCommandValidator : AbstractValidator<RemoveTeamMemberCommand>
{
    public RemoveTeamMemberCommandValidator()
    {
        RuleFor(command => command.TeamId).RequiredId("Team");

        RuleFor(command => command.UserId).RequiredId("User");
    }
}
