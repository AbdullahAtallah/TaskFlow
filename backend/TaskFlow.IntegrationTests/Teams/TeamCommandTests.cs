using System.Net;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Constants;
using TaskFlow.Domain.Entities.Tasks;
using TaskFlow.Domain.Entities.Teams;
using TaskFlow.Domain.Enums;
using TaskFlow.IntegrationTests.Infrastructure;

namespace TaskFlow.IntegrationTests.Teams;

public sealed class TeamCommandTests(TaskFlowFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task Create_StoresTheTeamWithItsMembers()
    {
        var member = await Data.UserAsync();

        var notification = await SendCommandExpectingDoneAsync(AdminEmail, HttpMethod.Post, "/api/team",
            new { name = "  Facilities  ", description = "   ", memberIds = new[] { member.Id } });

        notification.EntityType.Should().Be(EntityType.Team);
        notification.ActionType.Should().Be(ActionType.Created);
        var team = await Data.QueryAsync(dbContext => dbContext.Teams.Include(team => team.Members).SingleAsync(team => team.Id == notification.EntityId));
        team.Name.Should().Be("Facilities");
        team.Description.Should().BeNull();
        team.Members.Select(teamMember => teamMember.UserId).Should().Equal(member.Id);
        team.DateTimeCreated.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task Create_WithANameInUse_IgnoringCase_IsRefused()
    {
        await Data.TeamAsync(name: "Facilities");

        var notification = await SendCommandAsync(AdminEmail, HttpMethod.Post, "/api/team", new { name = "FACILITIES" });

        notification.ShouldBeError(ErrorMessages.TeamNameAlreadyInUse);
        (await Data.QueryAsync(dbContext => dbContext.Teams.CountAsync())).Should().Be(1);
    }

    [Fact]
    public async Task Create_WithAnUnknownMember_IsRefused()
    {
        var notification = await SendCommandAsync(AdminEmail, HttpMethod.Post, "/api/team", new { name = "Facilities", memberIds = new[] { Guid.NewGuid() } });

        notification.ShouldBeError(ErrorMessages.UsersNotFound);
    }

    [Fact]
    public async Task Create_WithoutAName_ReportsTheValidationMessage()
    {
        var notification = await SendCommandAsync(AdminEmail, HttpMethod.Post, "/api/team", new { name = "" });

        notification.ShouldBeError("Name is required.");
    }

    [Fact]
    public async Task Update_ChangesNameAndDescription()
    {
        var team = await Data.TeamAsync(name: "Facilities");

        await SendCommandExpectingDoneAsync(AdminEmail, HttpMethod.Put, $"/api/team/{team.Id}",
            new { name = "Facility Services", description = "Offices", rowVersion = team.RowVersion });

        var stored = (await Data.FindIncludingDeletedAsync<TeamEntity>(team.Id))!;
        stored.Name.Should().Be("Facility Services");
        stored.Description.Should().Be("Offices");
        stored.DateTimeModified.Should().NotBeNull();
    }

    [Fact]
    public async Task Update_KeepingItsOwnName_IsAllowed()
    {
        var team = await Data.TeamAsync(name: "Facilities");

        await SendCommandExpectingDoneAsync(AdminEmail, HttpMethod.Put, $"/api/team/{team.Id}",
            new { name = "Facilities", description = "New", rowVersion = team.RowVersion });
    }

    [Fact]
    public async Task Update_OfATeamOutsideScope_IsNotFound()
    {
        var teamManager = await Data.UserWithGrantsAsync([PackageTypes.Teams], [ClaimTypes.TeamsManage]);
        var team = await Data.TeamAsync(name: "Facilities");

        var notification = await SendCommandAsync(teamManager.UserName, HttpMethod.Put, $"/api/team/{team.Id}",
            new { name = "Taken over", rowVersion = team.RowVersion });

        notification.ShouldBeError(ErrorMessages.TeamNotFound);
        (await Data.FindIncludingDeletedAsync<TeamEntity>(team.Id))!.Name.Should().Be("Facilities");
    }

    [Fact]
    public async Task Delete_OfATeamThatStillOwnsProjects_IsRefused()
    {
        var team = await Data.TeamAsync();
        await Data.ProjectAsync(team);

        var notification = await SendCommandAsync(AdminEmail, HttpMethod.Delete, $"/api/team/{team.Id}");

        notification.ShouldBeError(ErrorMessages.TeamStillOwnsProjects);
        (await Data.FindIncludingDeletedAsync<TeamEntity>(team.Id))!.DateTimeDeleted.Should().BeNull();
    }

    [Fact]
    public async Task Delete_SoftDeletesTheTeam_RemovesItsMemberships_AndFreesItsName()
    {
        var team = await Data.TeamAsync([await Data.UserAsync()], name: "Facilities");
        var deletedProject = await Data.ProjectAsync(team);
        await Data.ExecuteAsync(async dbContext => dbContext.Projects.Remove((await dbContext.Projects.FindAsync(deletedProject.Id))!));

        var notification = await SendCommandExpectingDoneAsync(AdminEmail, HttpMethod.Delete, $"/api/team/{team.Id}");

        notification.ActionType.Should().Be(ActionType.Deleted);
        (await Data.FindIncludingDeletedAsync<TeamEntity>(team.Id))!.DateTimeDeleted.Should().NotBeNull();
        (await Data.QueryAsync(dbContext => dbContext.TeamMembers.CountAsync(member => member.TeamId == team.Id))).Should().Be(0);
        await (await SendAsync(AdminEmail, HttpMethod.Get, $"/api/team/{team.Id}")).ShouldHaveStatusAsync(HttpStatusCode.NotFound);

        await SendCommandExpectingDoneAsync(AdminEmail, HttpMethod.Post, "/api/team", new { name = "Facilities" });
    }

    [Fact]
    public async Task AddMembers_AddsNewMembers_AndSkipsExistingOnes()
    {
        var existingMember = await Data.UserAsync();
        var newMember = await Data.UserAsync();
        var team = await Data.TeamAsync([existingMember]);

        var notification = await SendCommandExpectingDoneAsync(AdminEmail, HttpMethod.Post, $"/api/team/{team.Id}/member",
            new { userIds = new[] { existingMember.Id, newMember.Id } });

        notification.EntityType.Should().Be(EntityType.TeamMember);
        notification.EntityId.Should().Be(team.Id);
        (await MemberIdsAsync(team.Id)).Should().BeEquivalentTo([existingMember.Id, newMember.Id]);
    }

    [Fact]
    public async Task AddMembers_WithAnUnknownUser_AddsNobody()
    {
        var team = await Data.TeamAsync();

        var notification = await SendCommandAsync(AdminEmail, HttpMethod.Post, $"/api/team/{team.Id}/member",
            new { userIds = new[] { (await Data.UserAsync()).Id, Guid.NewGuid() } });

        notification.ShouldBeError(ErrorMessages.UsersNotFound);
        (await MemberIdsAsync(team.Id)).Should().BeEmpty();
    }

    [Fact]
    public async Task RemoveMember_UnassignsTheirTasksInThisTeamsProjectsOnly()
    {
        var admin = await Data.AdminAsync();
        var member = await Data.UserAsync();
        var teamA = await Data.TeamAsync([member]);
        var teamB = await Data.TeamAsync([member]);
        var taskInA = await Data.TaskAsync(await Data.ProjectAsync(teamA), admin, assignee: member);
        var taskInB = await Data.TaskAsync(await Data.ProjectAsync(teamB), admin, assignee: member);

        await SendCommandExpectingDoneAsync(AdminEmail, HttpMethod.Delete, $"/api/team/{teamA.Id}/member/{member.Id}");

        (await MemberIdsAsync(teamA.Id)).Should().BeEmpty();
        (await Data.FindIncludingDeletedAsync<TaskEntity>(taskInA.Id))!.AssigneeId.Should().BeNull();
        (await Data.FindIncludingDeletedAsync<TaskEntity>(taskInB.Id))!.AssigneeId.Should().Be(member.Id);
    }

    [Fact]
    public async Task RemoveMember_WhoIsNotAMember_IsRefused()
    {
        var team = await Data.TeamAsync();

        var notification = await SendCommandAsync(AdminEmail, HttpMethod.Delete, $"/api/team/{team.Id}/member/{(await Data.UserAsync()).Id}");

        notification.ShouldBeError(ErrorMessages.TeamMemberNotFound);
    }

    private Task<List<Guid>> MemberIdsAsync(Guid teamId)
    {
        return Data.QueryAsync(dbContext => dbContext.TeamMembers.Where(member => member.TeamId == teamId).Select(member => member.UserId).ToListAsync());
    }
}
