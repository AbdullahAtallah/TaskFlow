using FluentValidation;
using TaskFlow.Application.Commands.Roles.Common;

namespace TaskFlow.Application.Commands.Roles.CreateRole;

public sealed class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator()
    {
        this.AddRoleFieldRules();
    }
}
