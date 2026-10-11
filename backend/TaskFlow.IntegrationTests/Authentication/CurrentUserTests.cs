using System.Net.Http.Json;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Constants;
using TaskFlow.IntegrationTests.Infrastructure;

namespace TaskFlow.IntegrationTests.Authentication;

public sealed class CurrentUserTests(TaskFlowFixture fixture) : IntegrationTestBase(fixture)
{
    private const string CurrentUserUrl = "/api/user/current";

    [Fact]
    public async Task Admin_HasEveryPackageFeatureAndClaim()
    {
        var currentUser = await GetAsync<CurrentUserResponse>(AdminEmail, CurrentUserUrl);

        currentUser.UserName.Should().Be(AdminEmail);
        currentUser.DisplayName.Should().Be("Test Administrator");
        currentUser.Roles.Select(role => role.Name).Should().Equal(DefaultRoleBundles.AdminRoleName);
        currentUser.Packages.Should().BeEquivalentTo(PackageTypes.All);
        currentUser.Features.Should().Equal(FeatureTypes.TasksBoard);
        currentUser.Claims.Should().BeEquivalentTo(ClaimTypes.All.Select(claim => claim.Id));
    }

    [Fact]
    public async Task PermissionsAreTheUnionOfAllRoles()
    {
        var planner = await Data.RoleAsync([PackageTypes.Management, PackageTypes.Tasks], [ClaimTypes.TasksCreate]);
        var user = await Data.UserInRolesAsync([DefaultRoleBundles.ReadOnlyUserRoleName, planner.Name]);

        var currentUser = await GetAsync<CurrentUserResponse>(user.UserName, CurrentUserUrl);

        currentUser.Roles.Should().HaveCount(2);
        currentUser.Claims.Should().BeEquivalentTo(DefaultRoleBundles.ReadOnlyUser.ClaimIds.Append(ClaimTypes.TasksCreate));
        currentUser.Packages.Should().BeEquivalentTo(DefaultRoleBundles.ReadOnlyUser.PackageIds.Append(PackageTypes.Management));
    }

    [Fact]
    public async Task FeatureOfAPackageTheUserLacks_IsNotListed()
    {
        var user = await Data.UserWithGrantsAsync([PackageTypes.Teams], []);

        var currentUser = await GetAsync<CurrentUserResponse>(user.UserName, CurrentUserUrl);

        currentUser.Packages.Should().BeEquivalentTo(PackageTypes.General, PackageTypes.Teams);
        currentUser.Features.Should().BeEmpty();
    }

    [Fact]
    public async Task PermissionChanges_ApplyToTheNextRequest_WithTheSameToken()
    {
        var user = await Data.UserAsync();
        var client = await ClientForAsync(user.UserName);
        (await client.GetFromJsonAsync<CurrentUserResponse>(CurrentUserUrl, TestJson.Options))!.Claims.Should().Contain(ClaimTypes.TasksAssign);

        await Data.ExecuteAsync(async dbContext =>
        {
            var userRole = await dbContext.Roles.SingleAsync(role => role.Name == DefaultRoleBundles.UserRoleName);
            dbContext.RoleClaims.Remove(await dbContext.RoleClaims.SingleAsync(roleClaim =>
                roleClaim.RoleId == userRole.Id && roleClaim.ClaimId == ClaimTypes.TasksAssign));
        });

        (await client.GetFromJsonAsync<CurrentUserResponse>(CurrentUserUrl, TestJson.Options))!.Claims.Should().NotContain(ClaimTypes.TasksAssign);
    }
}
