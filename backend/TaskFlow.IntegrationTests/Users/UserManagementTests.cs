using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Filtering;
using TaskFlow.Application.Queries.Users;
using TaskFlow.Domain.Constants;
// using TaskFlow.Domain.Entities.Tasks;
using TaskFlow.Domain.Entities.Users;
using TaskFlow.Domain.Enums;
using TaskFlow.IntegrationTests.Authentication;
using TaskFlow.IntegrationTests.Infrastructure;

namespace TaskFlow.IntegrationTests.Users;

public sealed class UserManagementTests(TaskFlowFixture fixture) : IntegrationTestBase(fixture)
{
    private const string InitialPassword = "Initial-pass-1";

    [Fact]
    public async Task Grid_ListsUsersWithTheirRolesAndTeams_SortedByName()
    {
        var laura = await Data.UserAsync(displayName: "Laura Bakker");
        await Data.TeamAsync([laura], name: "Finance");
        await Data.TeamAsync([laura], name: "Facilities");

        var users = await GetAsync<FilteringResult<UserRowDto>>(AdminEmail, "/api/user");

        users.Data.Select(user => user.DisplayName).Should().Equal("Laura Bakker", "Test Administrator");
        var row = users.Data[0];
        row.UserName.Should().Be(laura.UserName);
        row.IsActive.Should().BeTrue();
        row.Roles.Select(role => role.Name).Should().Equal(DefaultRoleBundles.UserRoleName);
        row.Teams.Select(team => team.Name).Should().Equal("Facilities", "Finance");
    }

    [Fact]
    public async Task Grid_FiltersByRoleAndActiveState_AndSearchesNamesAndEmailAddresses()
    {
        var readOnlyRole = await Data.RoleNamedAsync(DefaultRoleBundles.ReadOnlyUserRoleName);
        await Data.UserAsync(DefaultRoleBundles.ReadOnlyUserRoleName, displayName: "Eva Mulder");
        await Data.UserAsync(DefaultRoleBundles.ReadOnlyUserRoleName, isActive: false, displayName: "Joris van Dam");
        await Data.UserAsync(displayName: "Noor Hendriks");

        (await NamesAsync($"/api/user?roleId={readOnlyRole.Id}")).Should().Equal("Eva Mulder", "Joris van Dam");
        (await NamesAsync("/api/user?isActive=false")).Should().Equal("Joris van Dam");
        (await NamesAsync($"/api/user?roleId={readOnlyRole.Id}&isActive=true")).Should().Equal("Eva Mulder");
        (await NamesAsync("/api/user?query=HENDRIKS")).Should().Equal("Noor Hendriks");
        (await NamesAsync("/api/user?query=admin%40taskflow")).Should().Equal("Test Administrator");
    }

    [Fact]
    public async Task Grid_SortsAndPages()
    {
        await Data.UserAsync(displayName: "Anna", isActive: false);
        await Data.UserAsync(displayName: "Bram");

        (await NamesAsync("/api/user?orderByFields[0].field=displayName&orderByFields[0].dir=desc")).Should().Equal("Test Administrator", "Bram", "Anna");
        (await NamesAsync("/api/user?orderByFields[0].field=isActive&orderByFields[1].field=displayName")).Should().Equal("Anna", "Bram", "Test Administrator");

        var page = await GetAsync<FilteringResult<UserRowDto>>(AdminEmail, "/api/user?skip=1&take=1");
        page.Total.Should().Be(3);
        page.Data.Select(user => user.DisplayName).Should().Equal("Bram");
    }

    // GET /api/user/{id} is commented out in UserQueryController: the design has no user details page.
    // [Fact]
    // public async Task Details_ReturnRolesAndTeams_AndNotFoundForAnUnknownUser()
    // {
    //     var user = await Data.UserInRolesAsync([DefaultRoleBundles.UserRoleName, DefaultRoleBundles.ReadOnlyUserRoleName]);
    //     await Data.TeamAsync([user], name: "Marketing");
    //
    //     var details = await GetAsync<UserDetailDto>(AdminEmail, $"/api/user/{user.Id}");
    //
    //     details.Roles.Select(role => role.Name).Should().Equal(DefaultRoleBundles.ReadOnlyUserRoleName, DefaultRoleBundles.UserRoleName);
    //     details.Teams.Select(team => team.Name).Should().Equal("Marketing");
    //     await (await SendAsync(AdminEmail, HttpMethod.Get, $"/api/user/{Guid.NewGuid()}")).ShouldHaveStatusAsync(HttpStatusCode.NotFound);
    // }

    [Fact]
    public async Task Create_StoresTheUser_WhoThenSignsInWithTheChosenRole()
    {
        var readOnlyRole = await Data.RoleNamedAsync(DefaultRoleBundles.ReadOnlyUserRoleName);

        var response = await SendAsync(AdminEmail, HttpMethod.Post, "/api/user",
            new { userName = "  Laura.Bakker@Example.com ", displayName = "Laura Bakker", password = InitialPassword, roleIds = new[] { readOnlyRole.Id } });

        await response.ShouldHaveStatusAsync(HttpStatusCode.Created);
        var created = (await response.Content.ReadFromJsonAsync<UserDetailDto>(TestJson.Options))!;
        var currentUser = await GetAsync<CurrentUserResponse>("laura.bakker@example.com", "/api/user/current");
        currentUser.Id.Should().Be(created.Id);
        currentUser.DisplayName.Should().Be("Laura Bakker");
        currentUser.Roles.Select(role => role.Name).Should().Equal(DefaultRoleBundles.ReadOnlyUserRoleName);
    }

    [Fact]
    public async Task Create_WithAnEmailAddressInUse_IgnoringCase_Returns400()
    {
        var user = await Data.UserAsync();
        var userRole = await Data.RoleNamedAsync(DefaultRoleBundles.UserRoleName);

        var response = await SendAsync(AdminEmail, HttpMethod.Post, "/api/user",
            new { userName = user.UserName.ToUpperInvariant(), displayName = "Duplicate", password = InitialPassword, roleIds = new[] { userRole.Id } });

        await response.ShouldHaveStatusAsync(HttpStatusCode.BadRequest);
        (await response.ReadValidationErrorsAsync())["UserName"].Should().Equal(ErrorMessages.UserNameAlreadyInUse);
    }

    [Fact]
    public async Task Create_WithAnUnknownRole_AnInvalidEmail_OrAWeakPassword_Returns400()
    {
        var userRole = await Data.RoleNamedAsync(DefaultRoleBundles.UserRoleName);

        var unknownRole = await SendAsync(AdminEmail, HttpMethod.Post, "/api/user",
            new { userName = "new@taskflow.test", displayName = "New", password = InitialPassword, roleIds = new[] { Guid.NewGuid() } });
        var invalidEmail = await SendAsync(AdminEmail, HttpMethod.Post, "/api/user",
            new { userName = "no-at-sign", displayName = "New", password = InitialPassword, roleIds = new[] { userRole.Id } });
        var weakPassword = await SendAsync(AdminEmail, HttpMethod.Post, "/api/user",
            new { userName = "new@taskflow.test", displayName = "New", password = "weak", roleIds = new[] { userRole.Id } });

        (await unknownRole.ReadValidationErrorsAsync())["RoleIds"].Should().Equal(ErrorMessages.RolesNotFound);
        (await invalidEmail.ReadValidationErrorsAsync())["UserName"].Should().Equal("Enter a valid e-mail address.");
        (await weakPassword.ReadValidationErrorsAsync())["Password"].Should().Equal("Password must be 8 to 128 characters long.");
        (await Data.QueryAsync(dbContext => dbContext.Users.AnyAsync(user => user.UserName == "new@taskflow.test"))).Should().BeFalse();
    }

    [Fact]
    public async Task Deactivation_RevokesAccessOnTheNextRequest_AndActivationRestoresIt()
    {
        var user = await Data.UserAsync(displayName: "Joris");
        await (await SendAsync(user.UserName, HttpMethod.Get, "/api/team")).ShouldHaveStatusAsync(HttpStatusCode.OK);

        await SendCommandExpectingDoneAsync(AdminEmail, HttpMethod.Put, $"/api/user/{user.Id}",
            new { displayName = "Joris", isActive = false, rowVersion = user.RowVersion });

        var refused = await SendAsync(user.UserName, HttpMethod.Get, "/api/team");
        await refused.ShouldHaveStatusAsync(HttpStatusCode.Unauthorized);
        using (var body = JsonDocument.Parse(await refused.Content.ReadAsStringAsync()))
        {
            body.RootElement.GetProperty("error").GetString().Should().Be("account_revoked");
        }

        await SendCommandExpectingDoneAsync(AdminEmail, HttpMethod.Put, $"/api/user/{user.Id}",
            new { displayName = "Joris van Dam", isActive = true, rowVersion = await Data.RowVersionOfAsync<UserEntity>(user.Id) });

        (await GetAsync<CurrentUserResponse>(user.UserName, "/api/user/current")).DisplayName.Should().Be("Joris van Dam");
    }

    [Fact]
    public async Task Deactivation_EndsTheUsersSessions_SoReactivationDoesNotBringOldTokensBack()
    {
        var user = await Data.UserAsync(displayName: "Joris");
        var oldClient = await ClientForAsync(user.UserName);

        await SendCommandExpectingDoneAsync(AdminEmail, HttpMethod.Put, $"/api/user/{user.Id}",
            new { displayName = "Joris", isActive = false, rowVersion = user.RowVersion });
        var whileDeactivated = await oldClient.GetAsync("/api/user/current");
        await SendCommandExpectingDoneAsync(AdminEmail, HttpMethod.Put, $"/api/user/{user.Id}",
            new { displayName = "Joris", isActive = true, rowVersion = await Data.RowVersionOfAsync<UserEntity>(user.Id) });

        await whileDeactivated.ShouldHaveStatusAsync(HttpStatusCode.Unauthorized);
        (await whileDeactivated.ReadErrorAsync()).Should().Be("account_revoked", "an open session learns why it ended");
        await (await oldClient.GetAsync("/api/user/current")).ShouldHaveStatusAsync(HttpStatusCode.Unauthorized);
        await (await SendAsync(user.UserName, HttpMethod.Get, "/api/user/current")).ShouldHaveStatusAsync(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Deactivating_YourOwnAccount_IsRefused()
    {
        var admin = await Data.AdminAsync();

        var notification = await SendCommandAsync(AdminEmail, HttpMethod.Put, $"/api/user/{admin.Id}",
            new { displayName = "Me", isActive = false, rowVersion = admin.RowVersion });

        notification.ShouldBeError(ErrorMessages.CannotDeactivateOwnAccount);
    }

    // Changing a user's roles and deleting a user are commented out in UserController: the design has neither.
    // [Fact]
    // public async Task UpdateRoles_ReplacesTheRoles_AndAppliesThemOnTheNextRequest()
    // {
    //     var user = await Data.UserAsync();
    //     var adminRole = await Data.RoleNamedAsync(DefaultRoleBundles.AdminRoleName);
    //     await (await SendAsync(user.UserName, HttpMethod.Get, "/api/user")).ShouldHaveStatusAsync(HttpStatusCode.Forbidden);
    //
    //     await SendCommandExpectingDoneAsync(AdminEmail, HttpMethod.Put, $"/api/user/{user.Id}/permissions", new { userId = Guid.NewGuid(), roleIds = new[] { adminRole.Id } });
    //
    //     await (await SendAsync(user.UserName, HttpMethod.Get, "/api/user")).ShouldHaveStatusAsync(HttpStatusCode.OK);
    //     var roleNames = await Data.QueryAsync(dbContext => dbContext.UserRoles.Where(userRole => userRole.UserId == user.Id).Select(userRole => userRole.Role!.Name).ToListAsync());
    //     roleNames.Should().Equal(DefaultRoleBundles.AdminRoleName);
    // }
    //
    // [Fact]
    // public async Task UpdateRoles_RemovingYourOwnManagementAccess_IsRefused()
    // {
    //     var admin = await Data.AdminAsync();
    //     var userRole = await Data.RoleNamedAsync(DefaultRoleBundles.UserRoleName);
    //
    //     var notification = await SendCommandAsync(AdminEmail, HttpMethod.Put, $"/api/user/{admin.Id}/permissions", new { roleIds = new[] { userRole.Id } });
    //
    //     notification.ShouldBeError(ErrorMessages.CannotRemoveOwnManagementAccess);
    //     await (await SendAsync(AdminEmail, HttpMethod.Get, "/api/user")).ShouldHaveStatusAsync(HttpStatusCode.OK);
    // }
    //
    // [Fact]
    // public async Task Delete_SoftDeletesTheUser_AndRemovesRolesMembershipsAndAssignments()
    // {
    //     var admin = await Data.AdminAsync();
    //     var user = await Data.UserAsync(displayName: "Leaving");
    //     var team = await Data.TeamAsync([user]);
    //     var task = await Data.TaskAsync(await Data.ProjectAsync(team), admin, assignee: user);
    //     var client = await ClientForAsync(user.UserName);
    //     await (await client.GetAsync("/api/user/current")).ShouldHaveStatusAsync(HttpStatusCode.OK);
    //
    //     await SendCommandExpectingDoneAsync(AdminEmail, HttpMethod.Delete, $"/api/user/{user.Id}");
    //
    //     (await Data.FindIncludingDeletedAsync<UserEntity>(user.Id))!.DateTimeDeleted.Should().NotBeNull();
    //     (await Data.QueryAsync(dbContext => dbContext.UserRoles.AnyAsync(userRole => userRole.UserId == user.Id))).Should().BeFalse();
    //     (await Data.QueryAsync(dbContext => dbContext.TeamMembers.AnyAsync(member => member.UserId == user.Id))).Should().BeFalse();
    //     (await Data.FindIncludingDeletedAsync<TaskEntity>(task.Id))!.AssigneeId.Should().BeNull();
    //     (await NamesAsync("/api/user")).Should().NotContain("Leaving");
    //     await (await client.GetAsync("/api/user/current")).ShouldHaveStatusAsync(HttpStatusCode.Unauthorized);
    //
    //     var registerAgain = await Factory.CreateClient().PostAsJsonAsync("/api/auth/register",
    //         new { email = user.UserName, displayName = "Back again", password = InitialPassword }, TestJson.Options);
    //     await registerAgain.ShouldHaveStatusAsync(HttpStatusCode.Created);
    //     (await UserByEmailAsync(user.UserName)).Id.Should().NotBe(user.Id, "the e-mail address of a deleted user can start over as a new account");
    // }
    //
    // [Fact]
    // public async Task Delete_WorksForAUserWhoCreatedATaskAssignedToThemselves()
    // {
    //     var user = await Data.UserAsync(displayName: "Self-assigner");
    //     var task = await Data.TaskAsync(await Data.ProjectAsync(await Data.TeamAsync([user])), user, assignee: user);
    //
    //     await SendCommandExpectingDoneAsync(AdminEmail, HttpMethod.Delete, $"/api/user/{user.Id}");
    //
    //     (await Data.FindIncludingDeletedAsync<UserEntity>(user.Id))!.DateTimeDeleted.Should().NotBeNull();
    //     var storedTask = (await Data.FindIncludingDeletedAsync<TaskEntity>(task.Id))!;
    //     storedTask.AssigneeId.Should().BeNull();
    //     storedTask.CreatedById.Should().Be(user.Id);
    //     storedTask.DateTimeDeleted.Should().BeNull();
    // }

    [Fact]
    public async Task Update_WithoutIsActive_IsRefused_InsteadOfDeactivatingTheUser()
    {
        var user = await Data.UserAsync(displayName: "Still here");

        var response = await SendAsync(AdminEmail, HttpMethod.Put, $"/api/user/{user.Id}", new { displayName = "Renamed", rowVersion = user.RowVersion });

        await response.ShouldHaveStatusAsync(HttpStatusCode.BadRequest);
        var stored = (await Data.FindIncludingDeletedAsync<UserEntity>(user.Id))!;
        stored.IsActive.Should().BeTrue();
        stored.DisplayName.Should().Be("Still here");
    }

    // Deleting a user is commented out in UserController.
    // [Fact]
    // public async Task Deleting_YourOwnAccount_IsRefused()
    // {
    //     var admin = await Data.AdminAsync();
    //
    //     (await SendCommandAsync(AdminEmail, HttpMethod.Delete, $"/api/user/{admin.Id}")).ShouldBeError(ErrorMessages.CannotDeleteOwnAccount);
    // }

    private async Task<List<string>> NamesAsync(string url)
    {
        return (await GetAsync<FilteringResult<UserRowDto>>(AdminEmail, url)).Data.Select(user => user.DisplayName).ToList();
    }
}
