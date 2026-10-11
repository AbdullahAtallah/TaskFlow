using System.Net;
using AwesomeAssertions;
using TaskFlow.Application.Common.Filtering;
using TaskFlow.Application.Queries.Tasks;
using TaskFlow.Domain.Constants;
using TaskFlow.Domain.Entities.Projects;
using TaskFlow.Domain.Entities.Users;
using TaskFlow.Domain.Enums;
using TaskFlow.IntegrationTests.Infrastructure;
using TaskStatus = TaskFlow.Domain.Enums.TaskStatus;

namespace TaskFlow.IntegrationTests.Tasks;

public sealed class TaskQueryTests(TaskFlowFixture fixture) : IntegrationTestBase(fixture)
{
    private UserEntity _member = null!;
    private UserEntity _colleague = null!;
    private ProjectEntity _website = null!;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        _member = await Data.UserAsync(displayName: "Sara Jansen");
        _colleague = await Data.UserAsync(displayName: "Mees Visser");
        var marketing = await Data.TeamAsync([_member, _colleague], name: "Marketing");
        _website = await Data.ProjectAsync(marketing, "Website relaunch");
        var brand = await Data.ProjectAsync(marketing, "Brand guidelines");

        await Data.TaskAsync(_website, _member, "Write homepage copy", TaskStatus.InProgress, TaskPriority.High, Today.AddDays(2), _member, "Hero and pricing sections");
        await Data.TaskAsync(_website, _member, "Select hosting provider", TaskStatus.Done, TaskPriority.Medium, Today.AddDays(-15), _colleague);
        await Data.TaskAsync(_website, _member, "Migrate blog posts", TaskStatus.ToDo, TaskPriority.Low, Today.AddDays(-3));
        await Data.TaskAsync(_website, _member, "Set up redirects", TaskStatus.ToDo, TaskPriority.Medium);
        await Data.TaskAsync(brand, _member, "Define colour palette", TaskStatus.ToDo, TaskPriority.High, Today, _member);
    }

    [Fact]
    public async Task Grid_ShowsOnlyTasksOfTheUsersTeams_UnlessTasksViewAll()
    {
        var outsider = await Data.UserAsync();
        await Data.TaskAsync(await Data.ProjectAsync(await Data.TeamAsync([outsider])), outsider, "Other team's task");
        var viewer = await Data.UserWithGrantsAsync([PackageTypes.Tasks], [ClaimTypes.TasksViewAll]);

        (await TitlesAsync(_member, "/api/task")).Should().HaveCount(5).And.NotContain("Other team's task");
        (await TitlesAsync(outsider, "/api/task")).Should().Equal("Other team's task");
        (await GetAsync<FilteringResult<TaskRowDto>>(viewer.UserName, "/api/task")).Total.Should().Be(6);
    }

    [Fact]
    public async Task Grid_FiltersByProjectStatusAndPriority()
    {
        (await TitlesAsync(_member, $"/api/task?projectId={_website.Id}")).Should().HaveCount(4);
        (await TitlesAsync(_member, $"/api/task?projectId={_website.Id}&status=ToDo")).Should().Equal("Migrate blog posts", "Set up redirects");
        (await TitlesAsync(_member, "/api/task?priority=High")).Should().Equal("Define colour palette", "Write homepage copy");
        (await TitlesAsync(_member, "/api/task?status=1")).Should().Equal("Write homepage copy");
    }

    [Fact]
    public async Task Grid_FiltersByAssignee_ForMeAndUnassignedTasks()
    {
        (await TitlesAsync(_member, $"/api/task?assigneeId={_member.Id}")).Should().Equal("Define colour palette", "Write homepage copy");
        (await TitlesAsync(_member, $"/api/task?assigneeId={_colleague.Id}")).Should().Equal("Select hosting provider");
        (await TitlesAsync(_member, "/api/task?unassigned=true")).Should().Equal("Migrate blog posts", "Set up redirects");
    }

    [Fact]
    public async Task Grid_FiltersOverdueTasks_LeavingOutDoneTasksAndTasksDueToday()
    {
        (await TitlesAsync(_member, "/api/task?isOverdue=true")).Should().Equal("Migrate blog posts");
        (await TitlesAsync(_member, "/api/task?isOverdue=false")).Should().HaveCount(4).And.NotContain("Migrate blog posts");

        var overdue = (await GetAsync<FilteringResult<TaskRowDto>>(_member.UserName, "/api/task")).Data.Where(task => task.IsOverdue);
        overdue.Select(task => task.Title).Should().Equal("Migrate blog posts");
    }

    [Fact]
    public async Task Grid_FiltersAnInclusiveDueDateRange_LeavingOutTasksWithoutADueDate()
    {
        var from = Today.AddDays(-3).ToString("yyyy-MM-dd");
        var to = Today.AddDays(2).ToString("yyyy-MM-dd");

        (await TitlesAsync(_member, $"/api/task?dueFrom={from}&dueTo={to}"))
            .Should().Equal("Migrate blog posts", "Define colour palette", "Write homepage copy");
        (await TitlesAsync(_member, $"/api/task?dueTo={Today.AddDays(-4):yyyy-MM-dd}")).Should().Equal("Select hosting provider");
        (await TitlesAsync(_member, $"/api/task?dueFrom={Today.AddDays(3):yyyy-MM-dd}")).Should().BeEmpty();
    }

    [Fact]
    public async Task Grid_CombinesFilters()
    {
        (await TitlesAsync(_member, $"/api/task?projectId={_website.Id}&status=ToDo&unassigned=true&priority=Low"))
            .Should().Equal("Migrate blog posts");
        (await TitlesAsync(_member, $"/api/task?assigneeId={_member.Id}&priority=High&isOverdue=false&query=homepage"))
            .Should().Equal("Write homepage copy");
    }

    [Fact]
    public async Task Grid_SearchesTitleAndDescription()
    {
        (await TitlesAsync(_member, "/api/task?query=BLOG")).Should().Equal("Migrate blog posts");
        (await TitlesAsync(_member, "/api/task?query=pricing")).Should().Equal("Write homepage copy");
    }

    [Fact]
    public async Task Grid_SortsByDueDate_WithTasksWithoutADueDateLast_InBothDirections()
    {
        (await TitlesAsync(_member, $"/api/task?projectId={_website.Id}"))
            .Should().Equal("Select hosting provider", "Migrate blog posts", "Write homepage copy", "Set up redirects");
        (await TitlesAsync(_member, $"/api/task?projectId={_website.Id}&orderByFields[0].field=dueDate&orderByFields[0].dir=desc"))
            .Should().Equal("Write homepage copy", "Migrate blog posts", "Select hosting provider", "Set up redirects");
    }

    [Fact]
    public async Task Grid_SortsByPriorityStatusAndTitle()
    {
        (await TitlesAsync(_member, "/api/task?orderByFields[0].field=priority&orderByFields[0].dir=desc&orderByFields[1].field=title"))
            .Should().Equal("Define colour palette", "Write homepage copy", "Select hosting provider", "Set up redirects", "Migrate blog posts");
        (await TitlesAsync(_member, "/api/task?orderByFields[0].field=status&orderByFields[1].field=title&orderByFields[1].dir=desc"))
            .Should().Equal("Set up redirects", "Migrate blog posts", "Define colour palette", "Write homepage copy", "Select hosting provider");
    }

    [Fact]
    public async Task Grid_PagesTheFilteredAndSortedTasks()
    {
        var page = await GetAsync<FilteringResult<TaskRowDto>>(_member.UserName, "/api/task?status=ToDo&orderByFields[0].field=title&skip=1&take=1");

        page.Total.Should().Be(3);
        page.Data.Select(task => task.Title).Should().Equal("Migrate blog posts");
    }

    [Fact]
    public async Task Grid_ReturnsProjectAndAssigneeNames()
    {
        var row = (await GetAsync<FilteringResult<TaskRowDto>>(_member.UserName, "/api/task?query=homepage")).Data.Single();

        row.ProjectName.Should().Be("Website relaunch");
        row.AssigneeName.Should().Be("Sara Jansen");
        row.Status.Should().Be(TaskStatus.InProgress);
        row.Priority.Should().Be(TaskPriority.High);
    }

    [Theory]
    [InlineData("/api/task?unassigned=true&assigneeId=8b7e8f5e-3f3c-4bd6-9b9e-6f1f2a3b4c5d")]
    [InlineData("/api/task?status=7")]
    [InlineData("/api/task?status=Blocked")]
    [InlineData("/api/task?dueFrom=2026-10-10&dueTo=2026-10-01")]
    [InlineData("/api/task?dueFrom=yesterday")]
    [InlineData("/api/task?orderByFields[0].field=assignee")]
    public async Task Grid_RejectsInvalidFilters(string url)
    {
        var response = await SendAsync(_member.UserName, HttpMethod.Get, url);

        await response.ShouldHaveStatusAsync(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Details_ReturnTheTask_AndNotFoundForAnotherTeamsTask()
    {
        var outsider = await Data.UserAsync();
        var otherTask = await Data.TaskAsync(await Data.ProjectAsync(await Data.TeamAsync([outsider])), outsider);
        var ownTask = (await GetAsync<FilteringResult<TaskRowDto>>(_member.UserName, "/api/task?query=homepage")).Data.Single();

        var details = await GetAsync<TaskDetailDto>(_member.UserName, $"/api/task/{ownTask.Id}");
        details.Description.Should().Be("Hero and pricing sections");
        details.TeamName.Should().Be("Marketing");
        details.CreatedById.Should().Be(_member.Id);

        await (await SendAsync(_member.UserName, HttpMethod.Get, $"/api/task/{otherTask.Id}")).ShouldHaveStatusAsync(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task MyTasks_ListsMyOpenTasks_ByDueDate()
    {
        var myTasks = await GetAsync<FilteringResult<TaskRowDto>>(_member.UserName, "/api/task/my-tasks");

        myTasks.Total.Should().Be(2);
        myTasks.Data.Select(task => task.Title).Should().Equal("Define colour palette", "Write homepage copy");
        (await GetAsync<FilteringResult<TaskRowDto>>(_member.UserName, "/api/task/my-tasks?take=1")).Data.Should().ContainSingle();
    }

    [Fact]
    public async Task MyTasks_SearchesTitleAndDescription_OfMyOpenTasksOnly()
    {
        (await TitlesAsync(_member, "/api/task/my-tasks?query=PALETTE")).Should().Equal("Define colour palette");
        (await TitlesAsync(_member, "/api/task/my-tasks?query=pricing")).Should().Equal("Write homepage copy");
        (await TitlesAsync(_member, "/api/task/my-tasks?query=blog")).Should().BeEmpty();
    }

    [Fact]
    public async Task MyTasks_LeavesOutTasksOutsideMyScope()
    {
        var formerTeam = await Data.TeamAsync();
        await Data.TaskAsync(await Data.ProjectAsync(formerTeam), _member, "Left behind", assignee: _member);

        (await GetAsync<FilteringResult<TaskRowDto>>(_member.UserName, "/api/task/my-tasks")).Data
            .Should().NotContain(task => task.Title == "Left behind");
    }

    [Fact]
    public async Task StatusCounts_CountTheVisibleTasksPerStatus_AndTheOverdueOnes()
    {
        var outsider = await Data.UserAsync();
        await Data.TaskAsync(await Data.ProjectAsync(await Data.TeamAsync([outsider])), outsider, due: Today.AddDays(-9));

        var counts = await GetAsync<TaskStatusCountsDto>(_member.UserName, "/api/task/status-counts");

        counts.Should().BeEquivalentTo(new TaskStatusCountsDto { ToDo = 3, InProgress = 1, Done = 1, Overdue = 1 });
    }

    [Fact]
    public async Task StatusCounts_ForAUserWithoutTeams_AreZero()
    {
        var newcomer = await Data.UserAsync();

        (await GetAsync<TaskStatusCountsDto>(newcomer.UserName, "/api/task/status-counts")).Should().BeEquivalentTo(new TaskStatusCountsDto());
        (await GetAsync<FilteringResult<TaskRowDto>>(newcomer.UserName, "/api/task")).Total.Should().Be(0);
    }

    [Fact]
    public async Task Board_GroupsTheProjectsTasksInStatusColumns()
    {
        var board = await GetAsync<TaskBoardDto>(_member.UserName, $"/api/task/board?projectId={_website.Id}");

        board.ProjectName.Should().Be("Website relaunch");
        board.Columns.Select(column => column.Status).Should().Equal(TaskStatus.ToDo, TaskStatus.InProgress, TaskStatus.Done);
        board.Columns.Select(column => column.Count).Should().Equal(2, 1, 1);
        board.Columns[0].Tasks.Select(task => task.Title).Should().Equal("Migrate blog posts", "Set up redirects");
        board.Columns[0].Tasks[0].IsOverdue.Should().BeTrue();
    }

    [Fact]
    public async Task Board_AppliesThePriorityAndAssigneeFilters()
    {
        var unassigned = await GetAsync<TaskBoardDto>(_member.UserName, $"/api/task/board?projectId={_website.Id}&unassigned=true&priority=Medium");
        unassigned.Columns.Select(column => column.Count).Should().Equal(1, 0, 0);

        var mine = await GetAsync<TaskBoardDto>(_member.UserName, $"/api/task/board?projectId={_website.Id}&assigneeId={_member.Id}");
        mine.Columns.SelectMany(column => column.Tasks).Select(task => task.AssigneeName).Should().Equal("Sara Jansen");
    }

    [Fact]
    public async Task Board_OfAProjectOutsideScope_IsNotFound()
    {
        var outsider = await Data.UserAsync();
        await Data.TeamAsync([outsider]);

        var response = await SendAsync(outsider.UserName, HttpMethod.Get, $"/api/task/board?projectId={_website.Id}");

        await response.ShouldHaveStatusAsync(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Board_WithoutTheBoardClaim_IsForbidden()
    {
        var user = await Data.UserWithGrantsAsync([PackageTypes.Tasks], [ClaimTypes.TasksViewAll]);

        var response = await SendAsync(user.UserName, HttpMethod.Get, $"/api/task/board?projectId={_website.Id}");

        await response.ShouldHaveStatusAsync(HttpStatusCode.Forbidden);
    }

    private async Task<List<string>> TitlesAsync(UserEntity user, string url)
    {
        return (await GetAsync<FilteringResult<TaskRowDto>>(user.UserName, url)).Data.Select(task => task.Title).ToList();
    }
}
