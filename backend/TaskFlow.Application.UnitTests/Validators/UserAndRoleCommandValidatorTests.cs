using FluentValidation.TestHelper;
using TaskFlow.Application.Commands.Roles.CreateRole;
using TaskFlow.Application.Commands.Roles.UpdateRole;
using TaskFlow.Application.Commands.Users.CreateUser;
using TaskFlow.Application.Commands.Users.UpdateUser;
using TaskFlow.Application.Commands.Users.UpdateUserRoles;
using TaskFlow.Domain.Constants;

namespace TaskFlow.Application.UnitTests.Validators;

public sealed class UserAndRoleCommandValidatorTests
{
    private const string ValidPassword = "Initial-pass-1";

    private readonly CreateUserCommandValidator _createUserValidator = new();
    private readonly CreateRoleCommandValidator _createRoleValidator = new();

    [Fact]
    public void CreateUser_WithEmailNameAndRole_IsValid()
    {
        _createUserValidator.TestValidate(new CreateUserCommand("laura@example.com", "Laura Bakker", ValidPassword, [Guid.NewGuid()]))
            .ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("", "E-mail address is required.")]
    [InlineData("not-an-email", "Enter a valid e-mail address.")]
    public void CreateUser_RequiresAValidEmailAddress(string userName, string expectedMessage)
    {
        _createUserValidator.TestValidate(new CreateUserCommand(userName, "Laura Bakker", ValidPassword, [Guid.NewGuid()]))
            .ShouldHaveValidationErrorFor(command => command.UserName)
            .WithErrorMessage(expectedMessage);
    }

    [Fact]
    public void CreateUser_RequiresADisplayNameAndAtLeastOneDistinctRole()
    {
        var roleId = Guid.NewGuid();

        _createUserValidator.TestValidate(new CreateUserCommand("laura@example.com", "", ValidPassword, []))
            .ShouldHaveValidationErrorFor(command => command.DisplayName);
        _createUserValidator.TestValidate(new CreateUserCommand("laura@example.com", "Laura", ValidPassword, []))
            .ShouldHaveValidationErrorFor(command => command.RoleIds)
            .WithErrorMessage("Select at least one role.");
        _createUserValidator.TestValidate(new CreateUserCommand("laura@example.com", "Laura", ValidPassword, [roleId, roleId]))
            .ShouldHaveValidationErrorFor(command => command.RoleIds);
        _createUserValidator.TestValidate(new CreateUserCommand("laura@example.com", "Laura", ValidPassword, null!))
            .ShouldHaveValidationErrorFor(command => command.RoleIds);
    }

    [Theory]
    [InlineData("", "Password is required.")]
    [InlineData("Short-1", "Password must be 8 to 128 characters long.")]
    [InlineData("no-digits-here", "Password must contain at least one letter and one digit.")]
    public void CreateUser_AppliesThePasswordPolicy(string password, string expectedMessage)
    {
        _createUserValidator.TestValidate(new CreateUserCommand("laura@example.com", "Laura", password, [Guid.NewGuid()]))
            .ShouldHaveValidationErrorFor(command => command.Password)
            .WithErrorMessage(expectedMessage);
    }

    [Fact]
    public void UpdateUser_RequiresTheIdADisplayNameAndTheRowVersion()
    {
        var result = new UpdateUserCommandValidator().TestValidate(new UpdateUserCommand(Guid.Empty, " ", true, []));

        result.ShouldHaveValidationErrorFor(command => command.Id);
        result.ShouldHaveValidationErrorFor(command => command.DisplayName);
        result.ShouldHaveValidationErrorFor(command => command.RowVersion);
    }

    [Fact]
    public void UpdateUserRoles_RejectsEmptyRoleIds()
    {
        new UpdateUserRolesCommandValidator().TestValidate(new UpdateUserRolesCommand(Guid.NewGuid(), [Guid.Empty]))
            .ShouldHaveValidationErrorFor(command => command.RoleIds);
    }

    [Fact]
    public void CreateRole_WithPackagesAndTheirClaims_IsValid()
    {
        _createRoleValidator.TestValidate(new CreateRoleCommand(
                "Planner",
                "Plans the work",
                false,
                [PackageTypes.Projects, PackageTypes.Tasks],
                [ClaimTypes.ProjectsCreate, ClaimTypes.TasksBoardView]))
            .ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateRole_RejectsUnknownPackagesAndClaims()
    {
        var result = _createRoleValidator.TestValidate(new CreateRoleCommand("Planner", null, false, ["Invoices"], ["Invoices.Manage"]));

        result.ShouldHaveValidationErrorFor("Packages[0]").WithErrorMessage("'Invoices' is not a package.");
        result.ShouldHaveValidationErrorFor("Claims[0]").WithErrorMessage("'Invoices.Manage' is not a claim.");
    }

    [Fact]
    public void CreateRole_RejectsAClaimWhosePackageIsNotGranted()
    {
        _createRoleValidator.TestValidate(new CreateRoleCommand("Planner", null, false, [PackageTypes.Projects], [ClaimTypes.TasksCreate]))
            .ShouldHaveValidationErrorFor("Claims[0]")
            .WithErrorMessage("The claim 'Tasks.Create' requires the Tasks package.");
    }

    [Fact]
    public void CreateRole_RejectsMissingListsAndNullClaims()
    {
        _createRoleValidator.TestValidate(new CreateRoleCommand("Planner", null, false, null!, null!))
            .ShouldHaveValidationErrorFor(command => command.Packages);
        _createRoleValidator.TestValidate(new CreateRoleCommand("Planner", null, false, [], [null!]))
            .ShouldHaveValidationErrorFor("Claims[0]");
    }

    [Fact]
    public void CreateRole_LimitsNameAndDescription()
    {
        var result = _createRoleValidator.TestValidate(new CreateRoleCommand(
            new string('a', FieldLengths.RoleName + 1),
            new string('b', FieldLengths.RoleDescription + 1),
            false,
            [],
            []));

        result.ShouldHaveValidationErrorFor(command => command.Name);
        result.ShouldHaveValidationErrorFor(command => command.Description);
    }

    [Fact]
    public void TheRoleForNewUsers_CannotGrantManagement_ButAnyOtherRoleCan()
    {
        _createRoleValidator.TestValidate(new CreateRoleCommand("Everyone", null, true, [PackageTypes.Management], []))
            .ShouldHaveValidationErrorFor(command => command.Packages)
            .WithErrorMessage(ErrorMessages.RoleForNewUsersCannotGrantManagement);
        _createRoleValidator.TestValidate(new CreateRoleCommand("Administrators", null, false, [PackageTypes.Management], []))
            .ShouldNotHaveValidationErrorFor(command => command.Packages);
    }

    [Fact]
    public void UpdateRole_RequiresTheRoleId()
    {
        new UpdateRoleCommandValidator().TestValidate(new UpdateRoleCommand(Guid.Empty, [], []))
            .ShouldHaveValidationErrorFor(command => command.Id);
    }

    [Fact]
    public void UpdateRole_AppliesThePermissionRules()
    {
        var validator = new UpdateRoleCommandValidator();

        validator.TestValidate(new UpdateRoleCommand(Guid.NewGuid(), [PackageTypes.Tasks], [ClaimTypes.TasksBoardView]))
            .ShouldNotHaveAnyValidationErrors();
        validator.TestValidate(new UpdateRoleCommand(Guid.NewGuid(), [PackageTypes.Projects], [ClaimTypes.TasksCreate]))
            .ShouldHaveValidationErrorFor("Claims[0]")
            .WithErrorMessage("The claim 'Tasks.Create' requires the Tasks package.");
        validator.TestValidate(new UpdateRoleCommand(Guid.NewGuid(), null!, null!))
            .ShouldHaveValidationErrorFor(command => command.Packages);
    }
}
