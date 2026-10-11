using System.Net;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Filtering;
using TaskFlow.Application.Queries.Projects;
using TaskFlow.Application.Queries.Tasks;
using TaskFlow.Domain.Constants;
using TaskFlow.Domain.Entities.Projects;
using TaskFlow.Domain.Entities.Tasks;
using TaskFlow.Domain.Enums;
using TaskFlow.IntegrationTests.Infrastructure;

namespace TaskFlow.IntegrationTests.Projects;

public sealed class ProjectCommandTests(TaskFlowFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task Create_InOwnTeam_StoresTheProject()
    {
        var member = await Data.UserAsync();
        var team = await Data.TeamAsync([member]);

        var notification = await SendCommandExpectingDoneAsync(member.UserName, HttpMethod.Post, "/api/project",
            new { teamId = team.Id, name = " Website relaunch ", description = "New marketing site for Q4" });

        notification.EntityType.Should().Be(EntityType.Project);
        var project = (await Data.FindIncludingDeletedAsync<ProjectEntity>(notification.EntityId!.Value))!;
        project.TeamId.Should().Be(team.Id);
        project.Name.Should().Be("Website relaunch");
        project.Description.Should().Be("New marketing site for Q4");
    }

    [Fact]
    public async Task Create_InAnotherTeam_IsNotFound_AndCreatesNothing()
    {
        var member = await Data.UserAsync();
        await Data.TeamAsync([member]);
        var otherTeam = await Data.TeamAsync();

        var notification = await SendCommandAsync(member.UserName, HttpMethod.Post, "/api/project", new { teamId = otherTeam.Id, name = "Sneaky" });

        notification.ShouldBeError(ErrorMessages.TeamNotFound);
        (await Data.QueryAsync(dbContext => dbContext.Projects.AnyAsync())).Should().BeFalse();
    }

    [Fact]
    public async Task Create_WithProjectsViewAll_CanUseAnyTeam()
    {
        var team = await Data.TeamAsync();

        await SendCommandExpectingDoneAsync(AdminEmail, HttpMethod.Post, "/api/project", new { teamId = team.Id, name = "Budget Q4" });
    }

    [Fact]
    public async Task Update_ChangesNameAndDescription()
    {
        var member = await Data.UserAsync();
        var project = await Data.ProjectAsync(await Data.TeamAsync([member]), "Website", "Old");

        await SendCommandExpectingDoneAsync(member.UserName, HttpMethod.Put, $"/api/project/{project.Id}",
            new { name = "Website relaunch", description = (string?)null, rowVersion = project.RowVersion });

        var stored = (await Data.FindIncludingDeletedAsync<ProjectEntity>(project.Id))!;
        stored.Name.Should().Be("Website relaunch");
        stored.Description.Should().BeNull();
        stored.DateTimeModified.Should().NotBeNull();
    }

    [Fact]
    public async Task Update_AndDelete_OfAnotherTeamsProject_AreNotFound_AndChangeNothing()
    {
        var memberOfTeamA = await Data.UserAsync();
        await Data.TeamAsync([memberOfTeamA]);
        var projectOfTeamB = await Data.ProjectAsync(await Data.TeamAsync(), "Team B plans");

        (await SendCommandAsync(memberOfTeamA.UserName, HttpMethod.Put, $"/api/project/{projectOfTeamB.Id}",
                new { name = "Hijacked", rowVersion = projectOfTeamB.RowVersion }))
            .ShouldBeError(ErrorMessages.ProjectNotFound);
        (await SendCommandAsync(memberOfTeamA.UserName, HttpMethod.Delete, $"/api/project/{projectOfTeamB.Id}"))
            .ShouldBeError(ErrorMessages.ProjectNotFound);

        var stored = (await Data.FindIncludingDeletedAsync<ProjectEntity>(projectOfTeamB.Id))!;
        stored.Name.Should().Be("Team B plans");
        stored.DateTimeDeleted.Should().BeNull();
    }

    [Fact]
    public async Task Delete_SoftDeletesTheProjectAndItsTasks_SoTheyDisappearEverywhere()
    {
        var admin = await Data.AdminAsync();
        var team = await Data.TeamAsync([admin]);
        var project = await Data.ProjectAsync(team);
        var firstTask = await Data.TaskAsync(project, admin, assignee: admin);
        var secondTask = await Data.TaskAsync(project, admin);

        await SendCommandExpectingDoneAsync(AdminEmail, HttpMethod.Delete, $"/api/project/{project.Id}");

        (await Data.FindIncludingDeletedAsync<ProjectEntity>(project.Id))!.DateTimeDeleted.Should().NotBeNull();
        (await Data.FindIncludingDeletedAsync<TaskEntity>(firstTask.Id))!.DateTimeDeleted.Should().NotBeNull();
        (await Data.FindIncludingDeletedAsync<TaskEntity>(secondTask.Id))!.DateTimeDeleted.Should().NotBeNull();

        (await GetAsync<FilteringResult<ProjectRowDto>>(AdminEmail, "/api/project")).Total.Should().Be(0);
        (await GetAsync<FilteringResult<TaskRowDto>>(AdminEmail, "/api/task")).Total.Should().Be(0);
        (await GetAsync<FilteringResult<TaskRowDto>>(AdminEmail, "/api/task/my-tasks")).Total.Should().Be(0);
        await (await SendAsync(AdminEmail, HttpMethod.Get, $"/api/task/{firstTask.Id}")).ShouldHaveStatusAsync(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_FreesTheTeamForDeletion()
    {
        var team = await Data.TeamAsync();
        var project = await Data.ProjectAsync(team);

        await SendCommandExpectingDoneAsync(AdminEmail, HttpMethod.Delete, $"/api/project/{project.Id}");
        await SendCommandExpectingDoneAsync(AdminEmail, HttpMethod.Delete, $"/api/team/{team.Id}");
    }
}
