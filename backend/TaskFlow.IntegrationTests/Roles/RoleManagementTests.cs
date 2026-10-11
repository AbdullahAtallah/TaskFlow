using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Filtering;
using TaskFlow.Application.Queries.Roles;
using TaskFlow.Domain.Constants;
using TaskFlow.Domain.Entities.Authorization;
using TaskFlow.Domain.Enums;
using TaskFlow.IntegrationTests.Infrastructure;

namespace TaskFlow.IntegrationTests.Roles;

public sealed class RoleManagementTests(TaskFlowFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task Grid_ListsTheDefaultRolesWithTheirCounts()
    {
        await Data.UserAsync();
        await Data.UserAsync();

        var roles = (await GetAsync<FilteringResult<RoleRowDto>>(AdminEmail, "/api/role")).Data;

        roles.Select(role => role.Name).Should().Equal("Admin", "ReadOnlyUser", "User");
        roles.Should().OnlyContain(role => role.IsBuiltIn);
        roles.Single(role => role.IsDefault).Name.Should().Be(DefaultRoleBundles.UserRoleName);
        var admin = roles.Single(role => role.Name == DefaultRoleBundles.AdminRoleName);
        admin.PackageCount.Should().Be(PackageTypes.All.Count);
        admin.ClaimCount.Should().Be(ClaimTypes.All.Count);
        admin.UserCount.Should().Be(1);
        roles.Single(role => role.Name == DefaultRoleBundles.UserRoleName).UserCount.Should().Be(2);
    }

    [Fact]
    public async Task Grid_SortsByUserCount_AndSearches()
    {
        await Data.UserAsync();
        await Data.UserAsync();

        (await GetAsync<FilteringResult<RoleRowDto>>(AdminEmail, "/api/role?orderByFields[0].field=userCount&orderByFields[0].dir=desc"))
            .Data.Select(role => role.Name).Should().Equal("User", "Admin", "ReadOnlyUser");
        (await GetAsync<FilteringResult<RoleRowDto>>(AdminEmail, "/api/role?query=read-only"))
            .Data.Select(role => role.Name).Should().Equal("ReadOnlyUser");
    }

    [Fact]
    public async Task Details_ReturnThePackagesAndClaims()
    {
        var role = await Data.RoleNamedAsync(DefaultRoleBundles.ReadOnlyUserRoleName);

        var details = await GetAsync<RoleDetailDto>(AdminEmail, $"/api/role/{role.Id}");

        details.IsBuiltIn.Should().BeTrue();
        details.IsDefault.Should().BeFalse();
        details.Packages.Should().BeEquivalentTo(DefaultRoleBundles.ReadOnlyUser.PackageIds);
        details.Claims.Should().BeEquivalentTo(DefaultRoleBundles.ReadOnlyUser.ClaimIds);
        await (await SendAsync(AdminEmail, HttpMethod.Get, $"/api/role/{Guid.NewGuid()}")).ShouldHaveStatusAsync(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Users_ListsTheUsersOfTheRole_WithSearchAndPaging()
    {
        await Data.UserAsync(DefaultRoleBundles.ReadOnlyUserRoleName, displayName: "Eva Mulder");
        await Data.UserAsync(DefaultRoleBundles.ReadOnlyUserRoleName, displayName: "Tim Smit", isActive: false);
        await Data.UserAsync(displayName: "Not read-only");
        var role = await Data.RoleNamedAsync(DefaultRoleBundles.ReadOnlyUserRoleName);

        var users = await GetAsync<FilteringResult<RoleUserDto>>(AdminEmail, $"/api/role/{role.Id}/user");
        users.Data.Select(user => user.DisplayName).Should().Equal("Eva Mulder", "Tim Smit");
        users.Data[1].IsActive.Should().BeFalse();

        (await GetAsync<FilteringResult<RoleUserDto>>(AdminEmail, $"/api/role/{role.Id}/user?query=smit&take=1"))
            .Data.Select(user => user.DisplayName).Should().Equal("Tim Smit");
        await (await SendAsync(AdminEmail, HttpMethod.Get, $"/api/role/{Guid.NewGuid()}/user")).ShouldHaveStatusAsync(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_StoresTheRole_AndAlwaysGrantsGeneral()
    {
        var notification = await SendCommandExpectingDoneAsync(AdminEmail, HttpMethod.Post, "/api/role", new
        {
            name = "Planner",
            description = "Plans the work",
            isDefault = false,
            packages = new[] { PackageTypes.Projects, PackageTypes.Tasks },
            claims = new[] { ClaimTypes.ProjectsCreate, ClaimTypes.TasksBoardView }
        });

        notification.EntityType.Should().Be(EntityType.Role);
        var details = await GetAsync<RoleDetailDto>(AdminEmail, $"/api/role/{notification.EntityId}");
        details.Name.Should().Be("Planner");
        details.IsBuiltIn.Should().BeFalse();
        details.Packages.Should().BeEquivalentTo(PackageTypes.General, PackageTypes.Projects, PackageTypes.Tasks);
        details.Claims.Should().BeEquivalentTo(ClaimTypes.ProjectsCreate, ClaimTypes.TasksBoardView);
    }

    [Fact]
    public async Task Create_WithANameInUseOrAClaimOutsideItsPackages_IsRefused()
    {
        (await SendCommandAsync(AdminEmail, HttpMethod.Post, "/api/role",
                new { name = "admin", isDefault = false, packages = Array.Empty<string>(), claims = Array.Empty<string>() }))
            .ShouldBeError(ErrorMessages.RoleNameAlreadyInUse);
        (await SendCommandAsync(AdminEmail, HttpMethod.Post, "/api/role",
                new { name = "Planner", isDefault = false, packages = new[] { PackageTypes.Projects }, claims = new[] { ClaimTypes.TasksManage } }))
            .ShouldBeError("The claim 'Tasks.Manage' requires the Tasks package.");
    }

    [Fact]
    public async Task TheRoleForNewUsers_CanNeverGrantManagement()
    {
        var userRole = await Data.RoleNamedAsync(DefaultRoleBundles.UserRoleName);

        (await SendCommandAsync(AdminEmail, HttpMethod.Post, "/api/role", new
            {
                name = "Everyone an admin", isDefault = true,
                packages = new[] { PackageTypes.Management }, claims = Array.Empty<string>()
            }))
            .ShouldBeError(ErrorMessages.RoleForNewUsersCannotGrantManagement);
        (await SendCommandAsync(AdminEmail, HttpMethod.Put, $"/api/role/{userRole.Id}", new
            {
                name = DefaultRoleBundles.UserRoleName, isDefault = true,
                packages = DefaultRoleBundles.User.PackageIds.Append(PackageTypes.Management), claims = DefaultRoleBundles.User.ClaimIds,
                rowVersion = userRole.RowVersion
            }))
            .ShouldBeError(ErrorMessages.RoleForNewUsersCannotGrantManagement);
    }

    [Fact]
    public async Task Create_AsTheDefault_TakesOverFromThePreviousDefault()
    {
        var notification = await SendCommandExpectingDoneAsync(AdminEmail, HttpMethod.Post, "/api/role",
            new { name = "Newcomer", isDefault = true, packages = new[] { PackageTypes.Teams }, claims = Array.Empty<string>() });

        var defaultRoles = await Data.QueryAsync(dbContext => dbContext.Roles.Where(role => role.IsDefault).Select(role => role.Id).ToListAsync());
        defaultRoles.Should().Equal(notification.EntityId!.Value);

        var registration = await Factory.CreateClient().PostAsJsonAsync("/api/auth/register",
            new { email = "brand.new@taskflow.test", displayName = "Brand New", password = "Brand-new-1" }, TestJson.Options);
        await registration.ShouldHaveStatusAsync(HttpStatusCode.Created);

        var newcomer = await GetAsync<Authentication.CurrentUserResponse>("brand.new@taskflow.test", "/api/user/current");
        newcomer.Roles.Select(role => role.Name).Should().Equal("Newcomer");
    }

    [Fact]
    public async Task Update_ChangesTheClaims_AndTheyApplyOnTheUsersNextRequest()
    {
        var member = await Data.UserAsync();
        var team = await Data.TeamAsync([member]);
        await SendCommandExpectingDoneAsync(member.UserName, HttpMethod.Post, "/api/project", new { teamId = team.Id, name = "Allowed" });
        var userRole = await Data.RoleNamedAsync(DefaultRoleBundles.UserRoleName);

        await SendCommandExpectingDoneAsync(AdminEmail, HttpMethod.Put, $"/api/role/{userRole.Id}", new
        {
            name = DefaultRoleBundles.UserRoleName,
            description = "Works in teams",
            isDefault = true,
            packages = DefaultRoleBundles.User.PackageIds,
            claims = DefaultRoleBundles.User.ClaimIds.Where(claimId => claimId != ClaimTypes.ProjectsCreate),
            rowVersion = userRole.RowVersion
        });

        var refused = await SendAsync(member.UserName, HttpMethod.Post, "/api/project", new { teamId = team.Id, name = "Refused" });
        await refused.ShouldHaveStatusAsync(HttpStatusCode.Forbidden);
        (await GetAsync<RoleDetailDto>(AdminEmail, $"/api/role/{userRole.Id}")).Description.Should().Be("Works in teams");
    }

    [Fact]
    public async Task Update_MakingTheDefaultRoleNotDefault_OrRenamingADefaultRole_IsRefused()
    {
        var userRole = await Data.RoleNamedAsync(DefaultRoleBundles.UserRoleName);
        var readOnlyRole = await Data.RoleNamedAsync(DefaultRoleBundles.ReadOnlyUserRoleName);

        (await SendCommandAsync(AdminEmail, HttpMethod.Put, $"/api/role/{userRole.Id}", new
            {
                name = DefaultRoleBundles.UserRoleName, isDefault = false,
                packages = DefaultRoleBundles.User.PackageIds, claims = DefaultRoleBundles.User.ClaimIds,
                rowVersion = userRole.RowVersion
            }))
            .ShouldBeError(ErrorMessages.RoleForNewUsersRequired);
        (await SendCommandAsync(AdminEmail, HttpMethod.Put, $"/api/role/{readOnlyRole.Id}", new
            {
                name = "Viewers", isDefault = false,
                packages = DefaultRoleBundles.ReadOnlyUser.PackageIds, claims = DefaultRoleBundles.ReadOnlyUser.ClaimIds,
                rowVersion = readOnlyRole.RowVersion
            }))
            .ShouldBeError(ErrorMessages.DefaultRoleCannotBeRenamed);
    }

    [Fact]
    public async Task Update_RemovingManagementFromYourOnlyManagementRole_IsRefused()
    {
        var adminRole = await Data.RoleNamedAsync(DefaultRoleBundles.AdminRoleName);

        var notification = await SendCommandAsync(AdminEmail, HttpMethod.Put, $"/api/role/{adminRole.Id}", new
        {
            name = DefaultRoleBundles.AdminRoleName,
            isDefault = false,
            packages = DefaultRoleBundles.User.PackageIds,
            claims = DefaultRoleBundles.User.ClaimIds,
            rowVersion = adminRole.RowVersion
        });

        notification.ShouldBeError(ErrorMessages.CannotRemoveOwnManagementAccess);
        (await GetAsync<RoleDetailDto>(AdminEmail, $"/api/role/{adminRole.Id}")).Packages.Should().Contain(PackageTypes.Management);
    }

    [Fact]
    public async Task Delete_OfADefaultRole_TheRoleForNewUsers_OrAnAssignedRole_IsRefused()
    {
        var adminRole = await Data.RoleNamedAsync(DefaultRoleBundles.AdminRoleName);
        var newcomerRole = await Data.RoleAsync([PackageTypes.Teams], [], name: "Newcomer");
        await Data.ExecuteAsync(async dbContext =>
        {
            foreach (var role in await dbContext.Roles.ToListAsync())
            {
                role.IsDefault = role.Id == newcomerRole.Id;
            }
        });
        var assignedRole = await Data.RoleAsync([PackageTypes.Teams], [], name: "Assigned");
        await Data.UserInRolesAsync([assignedRole.Name]);

        (await SendCommandAsync(AdminEmail, HttpMethod.Delete, $"/api/role/{adminRole.Id}")).ShouldBeError(ErrorMessages.DefaultRoleCannotBeDeleted);
        (await SendCommandAsync(AdminEmail, HttpMethod.Delete, $"/api/role/{newcomerRole.Id}")).ShouldBeError(ErrorMessages.RoleForNewUsersCannotBeDeleted);
        (await SendCommandAsync(AdminEmail, HttpMethod.Delete, $"/api/role/{assignedRole.Id}")).ShouldBeError(ErrorMessages.RoleStillAssigned);
    }

    [Fact]
    public async Task Delete_OfAnUnusedRole_SoftDeletesIt_AndFreesItsName()
    {
        var role = await Data.RoleAsync([PackageTypes.Teams], [ClaimTypes.TeamsViewAll], name: "Temporary");

        await SendCommandExpectingDoneAsync(AdminEmail, HttpMethod.Delete, $"/api/role/{role.Id}");

        (await Data.FindIncludingDeletedAsync<RoleEntity>(role.Id))!.DateTimeDeleted.Should().NotBeNull();
        (await Data.QueryAsync(dbContext => dbContext.RoleClaims.AnyAsync(roleClaim => roleClaim.RoleId == role.Id))).Should().BeFalse();
        await (await SendAsync(AdminEmail, HttpMethod.Get, $"/api/role/{role.Id}")).ShouldHaveStatusAsync(HttpStatusCode.NotFound);
        await SendCommandExpectingDoneAsync(AdminEmail, HttpMethod.Post, "/api/role",
            new { name = "Temporary", isDefault = false, packages = Array.Empty<string>(), claims = Array.Empty<string>() });
    }
}
