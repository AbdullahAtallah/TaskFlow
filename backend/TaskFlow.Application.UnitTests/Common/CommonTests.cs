using AwesomeAssertions;
using FluentValidation;
using FluentValidation.Results;
using TaskFlow.Application.Common;
using TaskFlow.Application.Common.Extensions;
using TaskFlow.Application.Common.Notifying;
using TaskFlow.Application.Identity;
using TaskFlow.Domain.Constants;
using TaskFlow.Domain.Entities.Teams;
using TaskFlow.Domain.Exceptions;
using TaskFlow.Domain.Extensions;

namespace TaskFlow.Application.UnitTests.Common;

public sealed class CommonTests
{
    [Fact]
    public void HandlerExceptionMessage_ShowsValidationAndBusinessMessages_AndHidesTheRest()
    {
        var validation = new ValidationException([new ValidationFailure("Name", "Name is required.")]);

        HandlerExceptionMessage.ForUser(validation).Should().Be("Name is required.");
        HandlerExceptionMessage.ForUser(new TaskFlowException(ErrorMessages.TeamNotFound)).Should().Be(ErrorMessages.TeamNotFound);
        HandlerExceptionMessage.ForUser(new InvalidOperationException("Connection string leaked")).Should().BeNull();
    }

    [Theory]
    [InlineData("Correct-horse-1", true)]
    [InlineData("abcdefg1", true)]
    [InlineData("abcdef1", false)]
    [InlineData("only-letters", false)]
    [InlineData("1234567890", false)]
    public void PasswordPolicy_NeedsEightCharactersWithALetterAndADigit(string password, bool expected)
    {
        PasswordPolicy.IsSatisfiedBy(password).Should().Be(expected);
    }

    [Fact]
    public void PasswordPolicy_RefusesPasswordsLongerThan128Characters()
    {
        PasswordPolicy.IsSatisfiedBy("a1" + new string('x', FieldLengths.PasswordMaximum - 2)).Should().BeTrue();
        PasswordPolicy.IsSatisfiedBy("a1" + new string('x', FieldLengths.PasswordMaximum - 1)).Should().BeFalse();
    }

    [Fact]
    public void EnsureUnchangedSince_AcceptsTheSameRowVersion_AndRefusesAnyOther()
    {
        var team = new TeamEntity { Name = "Facilities", RowVersion = [0, 0, 0, 0, 0, 0, 7, 1] };

        team.EnsureUnchangedSince([0, 0, 0, 0, 0, 0, 7, 1]);
        var outdated = () => team.EnsureUnchangedSince([0, 0, 0, 0, 0, 0, 7, 0]);

        outdated.Should().Throw<ConcurrencyConflictException>().WithMessage(ErrorMessages.ChangedByAnotherUser);
    }

    [Fact]
    public void UserCommandContext_HasOnlyItsOwnClaims()
    {
        var context = new UserCommandContext(Guid.NewGuid(), "sara@taskflow.test", [ClaimTypes.TasksAssign]);

        context.HasClaim(ClaimTypes.TasksAssign).Should().BeTrue();
        context.HasClaim(ClaimTypes.TasksManage).Should().BeFalse();
    }

    [Fact]
    public void ExpressionCombiner_RequiresBothPredicates()
    {
        var combined = ExpressionCombiner.AndAlso<int>(number => number > 2, number => number < 5).Compile();

        combined(3).Should().BeTrue();
        combined(1).Should().BeFalse();
        combined(7).Should().BeFalse();
    }

    [Theory]
    [InlineData("  Sara@Example.COM ", "sara@example.com")]
    [InlineData("sara@example.com", "sara@example.com")]
    public void NormalizeUserName_TrimsAndLowersTheEmailAddress(string input, string expected)
    {
        input.NormalizeUserName().Should().Be(expected);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("   ", null)]
    [InlineData(" text ", "text")]
    public void TrimToNull_TurnsBlankTextIntoNull(string? input, string? expected)
    {
        input.TrimToNull().Should().Be(expected);
    }
}
