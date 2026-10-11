using FluentValidation;
using TaskFlow.Application.Commands.Users.Common;
using TaskFlow.Application.Common.Validation;

namespace TaskFlow.Application.Commands.Users.UpdateUserRoles;

public sealed class UpdateUserRolesCommandValidator : AbstractValidator<UpdateUserRolesCommand>
{
    public UpdateUserRolesCommandValidator()
    {
        RuleFor(command => command.UserId).RequiredId("User");

        RuleFor(command => command.RoleIds).RoleIds();
    }
}
