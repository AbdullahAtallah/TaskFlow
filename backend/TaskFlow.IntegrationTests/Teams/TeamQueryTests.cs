using System.Net;
using System.Text.Json;
using AwesomeAssertions;
using TaskFlow.Application.Common.Filtering;
using TaskFlow.Application.Queries.Teams;
using TaskFlow.Domain.Constants;
using TaskFlow.Domain.Entities.Users;
using TaskFlow.IntegrationTests.Infrastructure;

namespace TaskFlow.IntegrationTests.Teams;

public sealed class TeamQueryTests(TaskFlowFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task Grid_ShowsAMemberOnlyTheirOwnTeams()
    {
        var member = await Data.UserAsync();
        var ownTeam = await Data.TeamAsync([member]);
        await Data.TeamAsync();

        var result = await GetAsync<FilteringResult<TeamRowDto>>(member.UserName, "/api/team");

        result.Total.Should().Be(1);
        result.Data.Should().ContainSingle().Which.Id.Should().Be(ownTeam.Id);
    }

    [Fact]
    public async Task Grid_ShowsEveryTeamWithTeamsViewAll_AndOnlyMineNarrowsItToOwnTeams()
    {
        var admin = await Data.AdminAsync();
        var adminTeam = await Data.TeamAsync([admin]);
        await Data.TeamAsync();
        await Data.TeamAsync();

        (await GetAsync<FilteringResult<TeamRowDto>>(AdminEmail, "/api/team")).Total.Should().Be(3);

        var mine = await GetAsync<FilteringResult<TeamRowDto>>(AdminEmail, "/api/team?onlyMine=true");
        mine.Total.Should().Be(1);
        mine.Data.Single().Id.Should().Be(adminTeam.Id);
    }

    [Fact]
    public async Task Grid_SearchesNameAndDescription_PartiallyAndIgnoringCase()
    {
        await Data.TeamAsync(name: "Facilities", description: "Building operations");
        await Data.TeamAsync(name: "Marketing", description: "Brand and website");
        await Data.TeamAsync(name: "IT Services", description: "Workplace hardware");

        (await TeamNamesAsync("/api/team?query=FACIL")).Should().Equal("Facilities");
        (await TeamNamesAsync("/api/team?query=website")).Should().Equal("Marketing");
        (await TeamNamesAsync("/api/team?query=%20services%20")).Should().Equal("IT Services");
        (await GetAsync<FilteringResult<TeamRowDto>>(AdminEmail, "/api/team?query=payroll")).Total.Should().Be(0);
    }

    [Fact]
    public async Task Grid_MatchesLikeWildcardsInTheSearchTextLiterally()
    {
        await Data.TeamAsync(name: "100% Growth");
        await Data.TeamAsync(name: "Growth Hackers");

        (await TeamNamesAsync($"/api/team?query={Uri.EscapeDataString("100%")}")).Should().Equal("100% Growth");
        (await TeamNamesAsync("/api/team?query=_")).Should().BeEmpty();
    }

    [Fact]
    public async Task Grid_CountsMembersAndOnlyActiveProjects()
    {
        var team = await Data.TeamAsync([await Data.UserAsync(), await Data.UserAsync()]);
        await Data.ProjectAsync(team);
        var deletedProject = await Data.ProjectAsync(team);
        await Data.ExecuteAsync(async dbContext => dbContext.Projects.Remove((await dbContext.Projects.FindAsync(deletedProject.Id))!));

        var row = (await GetAsync<FilteringResult<TeamRowDto>>(AdminEmail, "/api/team")).Data.Single();

        row.MemberCount.Should().Be(2);
        row.ProjectCount.Should().Be(1);
    }

    [Theory]
    [InlineData("name", "asc", "Alpha,Bravo,Charlie")]
    [InlineData("name", "desc", "Charlie,Bravo,Alpha")]
    [InlineData("memberCount", "asc", "Alpha,Charlie,Bravo")]
    [InlineData("memberCount", "desc", "Bravo,Charlie,Alpha")]
    [InlineData("projectCount", "asc", "Bravo,Charlie,Alpha")]
    [InlineData("projectCount", "desc", "Alpha,Charlie,Bravo")]
    [InlineData("dateTimeCreated", "asc", "Bravo,Charlie,Alpha")]
    [InlineData("dateTimeCreated", "desc", "Alpha,Charlie,Bravo")]
    public async Task Grid_SortsOnEveryColumn(string field, string direction, string expectedOrder)
    {
        await CreateTeamAsync("Alpha", members: 1, projects: 3, created: new DateTime(2026, 1, 3, 0, 0, 0, DateTimeKind.Utc));
        await CreateTeamAsync("Bravo", members: 3, projects: 1, created: new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        await CreateTeamAsync("Charlie", members: 2, projects: 2, created: new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc));

        var names = await TeamNamesAsync($"/api/team?orderByFields[0].field={field}&orderByFields[0].dir={direction}");

        names.Should().Equal(expectedOrder.Split(','));
    }

    [Fact]
    public async Task Grid_SortsOnSeveralColumns()
    {
        await CreateTeamAsync("Bravo", members: 1, projects: 0);
        await CreateTeamAsync("Alpha", members: 1, projects: 0);
        await CreateTeamAsync("Charlie", members: 2, projects: 0);

        var names = await TeamNamesAsync("/api/team?orderByFields[0].field=memberCount&orderByFields[0].dir=desc&orderByFields[1].field=name");

        names.Should().Equal("Charlie", "Alpha", "Bravo");
    }

    [Fact]
    public async Task Grid_PagesTheSortedTeams_AndReportsTheTotal()
    {
        foreach (var number in Enumerable.Range(1, 5))
        {
            await Data.TeamAsync(name: $"Team {number}");
        }

        var page = await GetAsync<FilteringResult<TeamRowDto>>(AdminEmail, "/api/team?skip=2&take=2");
        page.Total.Should().Be(5);
        page.Data.Select(team => team.Name).Should().Equal("Team 3", "Team 4");

        var beyondTheEnd = await GetAsync<FilteringResult<TeamRowDto>>(AdminEmail, "/api/team?skip=10&take=2");
        beyondTheEnd.Total.Should().Be(5);
        beyondTheEnd.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task Grid_ReturnsAnEmptyPage_WhenNothingMatches()
    {
        var result = await GetAsync<FilteringResult<TeamRowDto>>(AdminEmail, "/api/team");

        result.Total.Should().Be(0);
        result.Data.Should().BeEmpty();
    }

    [Theory]
    [InlineData("/api/team?orderByFields[0].field=password", "OrderByFields[0].Field")]
    [InlineData("/api/team?take=0", "Take")]
    [InlineData("/api/team?take=1000", "Take")]
    [InlineData("/api/team?skip=-1", "Skip")]
    public async Task Grid_RejectsInvalidParameters_WithValidationProblemDetails(string url, string invalidField)
    {
        var response = await SendAsync(AdminEmail, HttpMethod.Get, url);

        await response.ShouldHaveStatusAsync(HttpStatusCode.BadRequest);
        using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        problem.RootElement.GetProperty("errors").TryGetProperty(invalidField, out _).Should().BeTrue();
    }

    [Fact]
    public async Task Grid_RejectsAnUnknownSortDirection()
    {
        var response = await SendAsync(AdminEmail, HttpMethod.Get, "/api/team?orderByFields[0].field=name&orderByFields[0].dir=sideways");

        await response.ShouldHaveStatusAsync(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Details_ReturnATeamInScope_AndNotFoundOutsideIt()
    {
        var member = await Data.UserAsync();
        var ownTeam = await Data.TeamAsync([member], name: "Facilities", description: "Building operations");
        await Data.ProjectAsync(ownTeam);
        var otherTeam = await Data.TeamAsync();

        var details = await GetAsync<TeamDetailDto>(member.UserName, $"/api/team/{ownTeam.Id}");
        details.Name.Should().Be("Facilities");
        details.MemberCount.Should().Be(1);
        details.ProjectCount.Should().Be(1);
        details.DateTimeCreated.Kind.Should().Be(DateTimeKind.Utc);

        await (await SendAsync(member.UserName, HttpMethod.Get, $"/api/team/{otherTeam.Id}")).ShouldHaveStatusAsync(HttpStatusCode.NotFound);
        await (await SendAsync(member.UserName, HttpMethod.Get, $"/api/team/{Guid.NewGuid()}")).ShouldHaveStatusAsync(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Members_AreListedWithTheirRoles_SortedByName()
    {
        var bob = await Data.UserInRolesAsync([DefaultRoleBundles.UserRoleName, DefaultRoleBundles.ReadOnlyUserRoleName], displayName: "Bob");
        var anna = await Data.UserAsync(displayName: "Anna");
        var team = await Data.TeamAsync([bob, anna]);

        var members = await GetAsync<List<TeamMemberDto>>(anna.UserName, $"/api/team/{team.Id}/member");

        members.Select(member => member.DisplayName).Should().Equal("Anna", "Bob");
        members[0].UserName.Should().Be(anna.UserName);
        members[0].Roles.Should().Equal(DefaultRoleBundles.UserRoleName);
        members[1].Roles.Should().Equal(DefaultRoleBundles.ReadOnlyUserRoleName, DefaultRoleBundles.UserRoleName);
    }

    [Fact]
    public async Task Members_OfATeamOutsideScope_AreNotFound()
    {
        var member = await Data.UserAsync();
        var otherTeam = await Data.TeamAsync([await Data.UserAsync()]);

        var response = await SendAsync(member.UserName, HttpMethod.Get, $"/api/team/{otherTeam.Id}/member");

        await response.ShouldHaveStatusAsync(HttpStatusCode.NotFound);
    }

    private async Task<List<string>> TeamNamesAsync(string url)
    {
        return (await GetAsync<FilteringResult<TeamRowDto>>(AdminEmail, url)).Data.Select(team => team.Name).ToList();
    }

    private async Task CreateTeamAsync(string name, int members, int projects, DateTime? created = null)
    {
        var users = new List<UserEntity>();
        for (var index = 0; index < members; index++)
        {
            users.Add(await Data.UserAsync());
        }

        var team = await Data.TeamAsync([.. users], name: name, created: created);
        for (var index = 0; index < projects; index++)
        {
            await Data.ProjectAsync(team);
        }
    }
}
