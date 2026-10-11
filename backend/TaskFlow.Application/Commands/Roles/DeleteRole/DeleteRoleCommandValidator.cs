using FluentValidation;
using TaskFlow.Application.Common.Validation;

namespace TaskFlow.Application.Commands.Roles.DeleteRole;

public sealed class DeleteRoleCommandValidator : AbstractValidator<DeleteRoleCommand>
{
    public DeleteRoleCommandValidator()
    {
        RuleFor(command => command.Id).RequiredId("Role");
    }
}
