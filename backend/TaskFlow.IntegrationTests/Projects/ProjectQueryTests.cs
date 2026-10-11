using System.Net;
using AwesomeAssertions;
using TaskFlow.Application.Common.Filtering;
using TaskFlow.Application.Queries.Projects;
using TaskFlow.Domain.Constants;
using TaskFlow.Domain.Entities.Tasks;
using TaskFlow.IntegrationTests.Infrastructure;
using TaskStatus = TaskFlow.Domain.Enums.TaskStatus;

namespace TaskFlow.IntegrationTests.Projects;

public sealed class ProjectQueryTests(TaskFlowFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task Grid_ShowsOnlyTheProjectsOfTheUsersTeams()
    {
        var member = await Data.UserAsync();
        var ownProject = await Data.ProjectAsync(await Data.TeamAsync([member]));
        await Data.ProjectAsync(await Data.TeamAsync());

        var result = await GetAsync<FilteringResult<ProjectRowDto>>(member.UserName, "/api/project");

        result.Total.Should().Be(1);
        result.Data.Single().Id.Should().Be(ownProject.Id);
    }

    [Fact]
    public async Task Grid_ShowsEveryProject_WithProjectsViewAll()
    {
        var viewer = await Data.UserWithGrantsAsync([PackageTypes.Projects], [ClaimTypes.ProjectsViewAll]);
        await Data.ProjectAsync(await Data.TeamAsync());
        await Data.ProjectAsync(await Data.TeamAsync());

        (await GetAsync<FilteringResult<ProjectRowDto>>(viewer.UserName, "/api/project")).Total.Should().Be(2);
    }

    [Fact]
    public async Task Grid_FiltersByTeam_AndCombinesItWithTheSearch()
    {
        var facilities = await Data.TeamAsync(name: "Facilities");
        var marketing = await Data.TeamAsync(name: "Marketing");
        await Data.ProjectAsync(facilities, "Office move Utrecht", "Move 120 workplaces");
        await Data.ProjectAsync(facilities, "Fire safety audit");
        await Data.ProjectAsync(marketing, "Website relaunch", "New marketing site");

        var byTeam = await GetAsync<FilteringResult<ProjectRowDto>>(AdminEmail, $"/api/project?teamId={facilities.Id}");
        byTeam.Data.Select(project => project.Name).Should().Equal("Fire safety audit", "Office move Utrecht");
        byTeam.Data.Should().OnlyContain(project => project.TeamName == "Facilities");

        var combined = await GetAsync<FilteringResult<ProjectRowDto>>(AdminEmail, $"/api/project?teamId={facilities.Id}&query=workplaces");
        combined.Data.Select(project => project.Name).Should().Equal("Office move Utrecht");

        (await GetAsync<FilteringResult<ProjectRowDto>>(AdminEmail, $"/api/project?teamId={marketing.Id}&query=audit")).Total.Should().Be(0);
    }

    [Fact]
    public async Task Grid_CountsTasks_AndFlagsOverdueOnes()
    {
        var admin = await Data.AdminAsync();
        var project = await Data.ProjectAsync(await Data.TeamAsync());
        await Data.TaskAsync(project, admin, status: TaskStatus.ToDo, due: Today.AddDays(-1));
        await Data.TaskAsync(project, admin, status: TaskStatus.InProgress, due: Today.AddDays(1));
        await Data.TaskAsync(project, admin, status: TaskStatus.Done, due: Today.AddDays(-10));
        var deletedTask = await Data.TaskAsync(project, admin, status: TaskStatus.ToDo);
        await Data.ExecuteAsync(async dbContext => dbContext.Tasks.Remove((await dbContext.Tasks.FindAsync(deletedTask.Id))!));

        var row = (await GetAsync<FilteringResult<ProjectRowDto>>(AdminEmail, "/api/project")).Data.Single();

        row.TaskCount.Should().Be(3);
        row.OpenTaskCount.Should().Be(2);
        row.DoneTaskCount.Should().Be(1);
        row.HasOverdueTasks.Should().BeTrue();
    }

    [Fact]
    public async Task Grid_DoesNotFlagDoneOrDueTodayTasksAsOverdue()
    {
        var admin = await Data.AdminAsync();
        var project = await Data.ProjectAsync(await Data.TeamAsync());
        await Data.TaskAsync(project, admin, status: TaskStatus.Done, due: Today.AddDays(-3));
        await Data.TaskAsync(project, admin, status: TaskStatus.ToDo, due: Today);
        await Data.TaskAsync(project, admin, status: TaskStatus.ToDo);

        (await GetAsync<FilteringResult<ProjectRowDto>>(AdminEmail, "/api/project")).Data.Single().HasOverdueTasks.Should().BeFalse();
    }

    [Theory]
    [InlineData("name", "asc", "Brand guidelines,Laptop refresh,Website relaunch")]
    [InlineData("name", "desc", "Website relaunch,Laptop refresh,Brand guidelines")]
    [InlineData("teamName", "asc", "Laptop refresh,Brand guidelines,Website relaunch")]
    [InlineData("openTaskCount", "desc", "Website relaunch,Brand guidelines,Laptop refresh")]
    [InlineData("taskCount", "asc", "Laptop refresh,Brand guidelines,Website relaunch")]
    [InlineData("dateTimeCreated", "desc", "Laptop refresh,Website relaunch,Brand guidelines")]
    public async Task Grid_SortsOnEveryColumn(string field, string direction, string expectedOrder)
    {
        var admin = await Data.AdminAsync();
        var marketing = await Data.TeamAsync(name: "Marketing");
        var website = await Data.ProjectAsync(marketing, "Website relaunch", created: new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc));
        var brand = await Data.ProjectAsync(marketing, "Brand guidelines", created: new DateTime(2026, 4, 15, 0, 0, 0, DateTimeKind.Utc));
        await Data.ProjectAsync(await Data.TeamAsync(name: "IT Services"), "Laptop refresh", created: new DateTime(2026, 9, 2, 0, 0, 0, DateTimeKind.Utc));
        await Data.TaskAsync(website, admin);
        await Data.TaskAsync(website, admin);
        await Data.TaskAsync(brand, admin);

        var names = (await GetAsync<FilteringResult<ProjectRowDto>>(AdminEmail,
                $"/api/project?orderByFields[0].field={field}&orderByFields[0].dir={direction}&orderByFields[1].field=name"))
            .Data.Select(project => project.Name);

        names.Should().Equal(expectedOrder.Split(','));
    }

    [Fact]
    public async Task Details_OfAProjectOfAnotherTeam_AreNotFound()
    {
        var memberOfTeamA = await Data.UserAsync();
        var projectOfTeamA = await Data.ProjectAsync(await Data.TeamAsync([memberOfTeamA]), "Office move");
        var projectOfTeamB = await Data.ProjectAsync(await Data.TeamAsync());

        var details = await GetAsync<ProjectDetailDto>(memberOfTeamA.UserName, $"/api/project/{projectOfTeamA.Id}");
        details.Name.Should().Be("Office move");

        var response = await SendAsync(memberOfTeamA.UserName, HttpMethod.Get, $"/api/project/{projectOfTeamB.Id}");
        await response.ShouldHaveStatusAsync(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CountsPerTeam_CountTheVisibleProjectsOfEachVisibleTeam()
    {
        var member = await Data.UserAsync();
        var facilities = await Data.TeamAsync([member], name: "Facilities");
        await Data.TeamAsync([member], name: "Brand");
        await Data.ProjectAsync(facilities);
        await Data.ProjectAsync(facilities);
        await Data.ProjectAsync(await Data.TeamAsync(name: "Invisible"));

        var counts = await GetAsync<List<ProjectCountPerTeamDto>>(member.UserName, "/api/project/counts-per-team");

        counts.Select(count => (count.TeamName, count.ProjectCount)).Should().Equal(("Brand", 0), ("Facilities", 2));
    }

    [Fact]
    public async Task Grid_RejectsAnUnknownSortField()
    {
        var response = await SendAsync(AdminEmail, HttpMethod.Get, "/api/project?orderByFields[0].field=budget");

        await response.ShouldHaveStatusAsync(HttpStatusCode.BadRequest);
    }
}
