using AwesomeAssertions;
using TaskFlow.Application.Queries.Users;
using TaskFlow.IntegrationTests.Infrastructure;

namespace TaskFlow.IntegrationTests.Users;

public sealed class UserSelectListTests(TaskFlowFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task WithATeam_ListsItsActiveMembers()
    {
        var anna = await Data.UserAsync(displayName: "Anna");
        var bram = await Data.UserAsync(displayName: "Bram");
        var inactive = await Data.UserAsync(isActive: false, displayName: "Inactive");
        var team = await Data.TeamAsync([anna, bram, inactive]);
        await Data.UserAsync(displayName: "Not a member");

        var users = await GetAsync<List<UserSelectItemDto>>(anna.UserName, $"/api/user/select-list?teamId={team.Id}");

        users.Select(user => user.DisplayName).Should().Equal("Anna", "Bram");
    }

    [Fact]
    public async Task WithoutATeam_ATeamManagerSeesEveryActiveUser()
    {
        await Data.UserAsync(displayName: "Anna");
        await Data.UserAsync(isActive: false, displayName: "Inactive");

        var users = await GetAsync<List<UserSelectItemDto>>(AdminEmail, "/api/user/select-list");

        users.Select(user => user.DisplayName).Should().Equal("Anna", "Test Administrator");
    }

    [Fact]
    public async Task WithoutATeam_AnAssignerOnlySeesColleagues()
    {
        var assigner = await Data.UserAsync(displayName: "Assigner");
        var colleague = await Data.UserAsync(displayName: "Colleague");
        await Data.TeamAsync([assigner, colleague]);
        await Data.UserAsync(displayName: "Stranger");

        var users = await GetAsync<List<UserSelectItemDto>>(assigner.UserName, "/api/user/select-list");

        users.Select(user => user.DisplayName).Should().Equal("Assigner", "Colleague");
    }

    [Fact]
    public async Task WithATeamOutsideScope_IsEmpty()
    {
        var assigner = await Data.UserAsync();
        var otherTeam = await Data.TeamAsync([await Data.UserAsync()]);

        (await GetAsync<List<UserSelectItemDto>>(assigner.UserName, $"/api/user/select-list?teamId={otherTeam.Id}")).Should().BeEmpty();
    }

    [Fact]
    public async Task Search_MatchesNameOrEmail_AndTakeLimitsTheList()
    {
        await Data.UserAsync(displayName: "Laura Bakker");
        await Data.UserAsync(displayName: "Mees Visser");

        (await GetAsync<List<UserSelectItemDto>>(AdminEmail, "/api/user/select-list?query=bakker")).Select(user => user.DisplayName)
            .Should().Equal("Laura Bakker");
        (await GetAsync<List<UserSelectItemDto>>(AdminEmail, "/api/user/select-list?take=1")).Should().ContainSingle();
    }
}
