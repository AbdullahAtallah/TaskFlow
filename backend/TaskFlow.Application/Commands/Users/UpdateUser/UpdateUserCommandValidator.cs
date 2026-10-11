using FluentValidation;
using TaskFlow.Application.Common.Validation;
using TaskFlow.Domain.Constants;

namespace TaskFlow.Application.Commands.Users.UpdateUser;

public sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(command => command.Id).RequiredId("User");

        RuleFor(command => command.DisplayName).RequiredText("Display name", FieldLengths.DisplayName);

        RuleFor(command => command.RowVersion).RequiredRowVersion();
    }
}
