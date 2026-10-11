using FluentValidation.TestHelper;
using TaskFlow.Application.Commands.Teams.AddTeamMembers;
using TaskFlow.Application.Commands.Teams.CreateTeam;
using TaskFlow.Application.Commands.Teams.DeleteTeam;
using TaskFlow.Application.Commands.Teams.RemoveTeamMember;
using TaskFlow.Application.Commands.Teams.UpdateTeam;
using TaskFlow.Domain.Constants;

namespace TaskFlow.Application.UnitTests.Validators;

public sealed class TeamCommandValidatorTests
{
    private readonly CreateTeamCommandValidator _createValidator = new();
    private readonly UpdateTeamCommandValidator _updateValidator = new();
    private readonly AddTeamMembersCommandValidator _addMembersValidator = new();

    [Fact]
    public void CreateTeam_WithNameAndMembers_IsValid()
    {
        _createValidator.TestValidate(new CreateTeamCommand("Facilities", "Building operations", [Guid.NewGuid()]))
            .ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateTeam_WithoutMembers_IsValid()
    {
        _createValidator.TestValidate(new CreateTeamCommand("Facilities", null, null)).ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateTeam_WithBlankName_RequiresAName(string name)
    {
        _createValidator.TestValidate(new CreateTeamCommand(name, null, null))
            .ShouldHaveValidationErrorFor(command => command.Name)
            .WithErrorMessage("Name is required.");
    }

    [Fact]
    public void CreateTeam_WithTooLongNameOrDescription_IsInvalid()
    {
        var result = _createValidator.TestValidate(new CreateTeamCommand(
            new string('a', FieldLengths.TeamName + 1),
            new string('b', FieldLengths.Description + 1),
            null));

        result.ShouldHaveValidationErrorFor(command => command.Name);
        result.ShouldHaveValidationErrorFor(command => command.Description);
    }

    [Fact]
    public void CreateTeam_WithDuplicateOrEmptyMemberIds_IsInvalid()
    {
        var userId = Guid.NewGuid();

        _createValidator.TestValidate(new CreateTeamCommand("Facilities", null, [userId, userId]))
            .ShouldHaveValidationErrorFor(command => command.MemberIds);
        _createValidator.TestValidate(new CreateTeamCommand("Facilities", null, [Guid.Empty]))
            .ShouldHaveValidationErrorFor("MemberIds[0]");
    }

    [Fact]
    public void UpdateTeam_RequiresTheTeamIdAndAName()
    {
        var result = _updateValidator.TestValidate(new UpdateTeamCommand(Guid.Empty, "", null, []));

        result.ShouldHaveValidationErrorFor(command => command.Id);
        result.ShouldHaveValidationErrorFor(command => command.Name);
        result.ShouldHaveValidationErrorFor(command => command.RowVersion);
    }

    [Fact]
    public void DeleteTeam_RequiresTheTeamId()
    {
        new DeleteTeamCommandValidator().TestValidate(new DeleteTeamCommand(Guid.Empty))
            .ShouldHaveValidationErrorFor(command => command.Id);
    }

    [Fact]
    public void AddTeamMembers_RequiresAtLeastOneDistinctUser()
    {
        var userId = Guid.NewGuid();

        _addMembersValidator.TestValidate(new AddTeamMembersCommand(Guid.NewGuid(), []))
            .ShouldHaveValidationErrorFor(command => command.UserIds)
            .WithErrorMessage("Select at least one user.");
        _addMembersValidator.TestValidate(new AddTeamMembersCommand(Guid.NewGuid(), [userId, userId]))
            .ShouldHaveValidationErrorFor(command => command.UserIds);
        _addMembersValidator.TestValidate(new AddTeamMembersCommand(Guid.NewGuid(), [userId]))
            .ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void RemoveTeamMember_RequiresBothIds()
    {
        var result = new RemoveTeamMemberCommandValidator().TestValidate(new RemoveTeamMemberCommand(Guid.Empty, Guid.Empty));

        result.ShouldHaveValidationErrorFor(command => command.TeamId);
        result.ShouldHaveValidationErrorFor(command => command.UserId);
    }
}
