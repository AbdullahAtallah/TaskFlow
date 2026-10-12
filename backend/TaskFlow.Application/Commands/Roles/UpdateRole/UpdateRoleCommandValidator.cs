using FluentValidation;
using TaskFlow.Application.Commands.Roles.Common;
using TaskFlow.Application.Common.Validation;

namespace TaskFlow.Application.Commands.Roles.UpdateRole;

public sealed class UpdateRoleCommandValidator : AbstractValidator<UpdateRoleCommand>
{
    public UpdateRoleCommandValidator()
    {
        RuleFor(command => command.Id).RequiredId("Role");

        this.AddRolePermissionRules();
    }
}
