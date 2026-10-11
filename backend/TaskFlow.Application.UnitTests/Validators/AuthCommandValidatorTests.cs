using AwesomeAssertions;
using FluentValidation.TestHelper;
using TaskFlow.Application.Commands.Auth.Login;
using TaskFlow.Application.Commands.Auth.Register;
using TaskFlow.Application.Commands.Users.CreateUser;
using TaskFlow.Application.Commands.Users.SetUserPassword;
using TaskFlow.Domain.Constants;

namespace TaskFlow.Application.UnitTests.Validators;

public sealed class AuthCommandValidatorTests
{
    private const string ValidPassword = "Correct-horse-1";

    private readonly RegisterCommandValidator _registerValidator = new();

    [Fact]
    public void Register_WithAnAddressANameAndAPassword_IsValid()
    {
        _registerValidator.TestValidate(new RegisterCommand("nina@taskflow.test", "Nina", ValidPassword)).ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("", "E-mail address is required.")]
    [InlineData("no-at-sign", "Enter a valid e-mail address.")]
    public void Register_RequiresAValidEmailAddress(string email, string expectedMessage)
    {
        _registerValidator.TestValidate(new RegisterCommand(email, "Nina", ValidPassword))
            .ShouldHaveValidationErrorFor(command => command.Email)
            .WithErrorMessage(expectedMessage);
    }

    [Fact]
    public void Register_LimitsTheEmailAddressToTheColumnSize()
    {
        var email = new string('a', FieldLengths.UserName) + "@taskflow.test";

        _registerValidator.TestValidate(new RegisterCommand(email, "Nina", ValidPassword))
            .ShouldHaveValidationErrorFor(command => command.Email)
            .WithErrorMessage($"E-mail address cannot exceed {FieldLengths.UserName} characters.");
    }

    [Theory]
    [InlineData("", "Password is required.")]
    [InlineData("Short-1", "Password must be 8 to 128 characters long.")]
    [InlineData("12345678", "Password must contain at least one letter and one digit.")]
    [InlineData("no-digits-here", "Password must contain at least one letter and one digit.")]
    public void Register_AppliesThePasswordPolicy_WithOneMessagePerPassword(string password, string expectedMessage)
    {
        var result = _registerValidator.TestValidate(new RegisterCommand("nina@taskflow.test", "Nina", password));

        result.ShouldHaveValidationErrorFor(command => command.Password).WithErrorMessage(expectedMessage);
        result.Errors.Should().ContainSingle(error => error.PropertyName == nameof(RegisterCommand.Password));
    }

    [Fact]
    public void Register_RequiresADisplayName()
    {
        _registerValidator.TestValidate(new RegisterCommand("nina@taskflow.test", " ", ValidPassword))
            .ShouldHaveValidationErrorFor(command => command.DisplayName)
            .WithErrorMessage("Display name is required.");
    }

    [Fact]
    public void Login_RequiresBothFields_ButNotThePasswordPolicy()
    {
        var validator = new LoginCommandValidator();

        validator.TestValidate(new LoginCommand("nina@taskflow.test", "old")).ShouldNotHaveAnyValidationErrors();
        var empty = validator.TestValidate(new LoginCommand("", ""));
        empty.ShouldHaveValidationErrorFor(command => command.Email);
        empty.ShouldHaveValidationErrorFor(command => command.Password);
    }

    [Fact]
    public void SetUserPassword_RequiresTheUserAndAPasswordThatMeetsThePolicy()
    {
        var validator = new SetUserPasswordCommandValidator();

        validator.TestValidate(new SetUserPasswordCommand(Guid.NewGuid(), ValidPassword)).ShouldNotHaveAnyValidationErrors();
        var invalid = validator.TestValidate(new SetUserPasswordCommand(Guid.Empty, "weak"));
        invalid.ShouldHaveValidationErrorFor(command => command.UserId);
        invalid.ShouldHaveValidationErrorFor(command => command.Password);
    }

    [Fact]
    public void CommandsWithAPassword_LeaveItOutOfTheirText_SoItCannotReachALog()
    {
        var texts = new[]
        {
            new RegisterCommand("nina@taskflow.test", "Nina", ValidPassword).ToString(),
            new LoginCommand("nina@taskflow.test", ValidPassword).ToString(),
            new CreateUserCommand("nina@taskflow.test", "Nina", ValidPassword, [Guid.NewGuid()]).ToString(),
            new SetUserPasswordCommand(Guid.NewGuid(), ValidPassword).ToString()
        };

        texts.Should().OnlyContain(text => !text.Contains(ValidPassword));
        texts[0].Should().Contain("nina@taskflow.test");
    }
}
