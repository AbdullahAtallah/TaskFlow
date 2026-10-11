using FluentValidation;
using TaskFlow.Domain.Constants;

namespace TaskFlow.Application.Commands.Auth.Login;

/// <summary>Only checks that both fields are filled in: the password policy applies when a password is set, not here.</summary>
public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(command => command.Email)
            .NotEmpty()
            .WithMessage("E-mail address is required.")
            .MaximumLength(FieldLengths.UserName)
            .WithMessage($"E-mail address cannot exceed {FieldLengths.UserName} characters.");

        RuleFor(command => command.Password)
            .NotEmpty()
            .WithMessage("Password is required.")
            .MaximumLength(FieldLengths.PasswordMaximum)
            .WithMessage($"Password cannot exceed {FieldLengths.PasswordMaximum} characters.");
    }
}
