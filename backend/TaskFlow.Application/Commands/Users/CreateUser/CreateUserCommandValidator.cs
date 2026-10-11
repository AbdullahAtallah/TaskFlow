using FluentValidation;
using TaskFlow.Application.Commands.Users.Common;
using TaskFlow.Application.Common.Validation;
using TaskFlow.Domain.Constants;

namespace TaskFlow.Application.Commands.Users.CreateUser;

public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(command => command.UserName).UserEmailAddress();

        RuleFor(command => command.DisplayName).RequiredText("Display name", FieldLengths.DisplayName);

        RuleFor(command => command.Password).Password();

        RuleFor(command => command.RoleIds).RoleIds();
    }
}
