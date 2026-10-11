using System.Net;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Filtering;
using TaskFlow.Application.Queries.Tasks;
using TaskFlow.Domain.Constants;
using TaskFlow.Domain.Entities.Projects;
using TaskFlow.Domain.Entities.Tasks;
using TaskFlow.Domain.Entities.Teams;
using TaskFlow.Domain.Entities.Users;
using TaskFlow.Domain.Enums;
using TaskFlow.IntegrationTests.Infrastructure;
using TaskStatus = TaskFlow.Domain.Enums.TaskStatus;

namespace TaskFlow.IntegrationTests.Tasks;

public sealed class TaskCommandTests(TaskFlowFixture fixture) : IntegrationTestBase(fixture)
{
    private UserEntity _member = null!;
    private UserEntity _colleague = null!;
    private TeamEntity _team = null!;
    private ProjectEntity _project = null!;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        _member = await Data.UserAsync();
        _colleague = await Data.UserAsync();
        _team = await Data.TeamAsync([_member, _colleague]);
        _project = await Data.ProjectAsync(_team, "Office move");
    }

    [Fact]
    public async Task Create_WithAnAssigneeFromTheTeam_StoresEveryField()
    {
        var notification = await SendCommandExpectingDoneAsync(_member.UserName, HttpMethod.Post, "/api/task", new
        {
            projectId = _project.Id,
            title = "  Book elevator slots ",
            description = "Saturday morning",
            status = "InProgress",
            priority = "High",
            dueDate = "2026-10-14",
            assigneeId = _colleague.Id
        });

        notification.EntityType.Should().Be(EntityType.Task);
        notification.ActionType.Should().Be(ActionType.Created);
        var task = (await Data.FindIncludingDeletedAsync<TaskEntity>(notification.EntityId!.Value))!;
        task.ProjectId.Should().Be(_project.Id);
        task.Title.Should().Be("Book elevator slots");
        task.Description.Should().Be("Saturday morning");
        task.Status.Should().Be(TaskStatus.InProgress);
        task.Priority.Should().Be(TaskPriority.High);
        task.DueDate.Should().Be(new DateOnly(2026, 10, 14));
        task.AssigneeId.Should().Be(_colleague.Id);
        task.CreatedById.Should().Be(_member.Id);
    }

    [Fact]
    public async Task Create_WithAnAssigneeOutsideTheTeam_IsRefused()
    {
        var outsider = await Data.UserAsync();

        var notification = await SendCommandAsync(_member.UserName, HttpMethod.Post, "/api/task",
            new { projectId = _project.Id, title = "Task", status = "ToDo", priority = "Low", assigneeId = outsider.Id });

        notification.ShouldBeError(ErrorMessages.AssigneeMustBeTeamMember);
    }

    [Fact]
    public async Task Create_WithAnAssigneeButWithoutTasksAssign_IsRefused_ButWithoutAnAssigneeItWorks()
    {
        var creator = await Data.UserWithGrantsAsync([PackageTypes.Tasks], [ClaimTypes.TasksCreate]);
        await Data.AddMembersAsync(_team, creator);

        (await SendCommandAsync(creator.UserName, HttpMethod.Post, "/api/task",
                new { projectId = _project.Id, title = "Assigned", status = "ToDo", priority = "Low", assigneeId = _member.Id }))
            .ShouldBeError(ErrorMessages.AssigningRequiresClaim);

        await SendCommandExpectingDoneAsync(creator.UserName, HttpMethod.Post, "/api/task",
            new { projectId = _project.Id, title = "Unassigned", status = "ToDo", priority = "Low" });
    }

    [Fact]
    public async Task Create_InAnotherTeamsProject_IsNotFound()
    {
        var otherProject = await Data.ProjectAsync(await Data.TeamAsync());

        var notification = await SendCommandAsync(_member.UserName, HttpMethod.Post, "/api/task",
            new { projectId = otherProject.Id, title = "Sneaky", status = "ToDo", priority = "Low" });

        notification.ShouldBeError(ErrorMessages.ProjectNotFound);
    }

    [Fact]
    public async Task Create_WithoutATitle_ReportsTheValidationMessage()
    {
        var notification = await SendCommandAsync(_member.UserName, HttpMethod.Post, "/api/task",
            new { projectId = _project.Id, title = " ", status = "ToDo", priority = "Low" });

        notification.ShouldBeError("Title is required.");
    }

    [Fact]
    public async Task Update_ChangesEveryField()
    {
        var task = await Data.TaskAsync(_project, _member, "Old", TaskStatus.ToDo, TaskPriority.Low, assignee: _member);

        await SendCommandExpectingDoneAsync(_member.UserName, HttpMethod.Put, $"/api/task/{task.Id}", new
        {
            title = "New",
            description = "Details",
            status = "Done",
            priority = "High",
            dueDate = "2026-11-02",
            assigneeId = _colleague.Id,
            rowVersion = task.RowVersion
        });

        var stored = (await Data.FindIncludingDeletedAsync<TaskEntity>(task.Id))!;
        stored.Title.Should().Be("New");
        stored.Description.Should().Be("Details");
        stored.Status.Should().Be(TaskStatus.Done);
        stored.Priority.Should().Be(TaskPriority.High);
        stored.DueDate.Should().Be(new DateOnly(2026, 11, 2));
        stored.AssigneeId.Should().Be(_colleague.Id);
        stored.DateTimeModified.Should().NotBeNull();
    }

    [Fact]
    public async Task Update_ChangingTheAssigneeWithoutTasksAssign_IsRefused_ButKeepingItIsAllowed()
    {
        var editor = await Data.UserWithGrantsAsync([PackageTypes.Tasks], [ClaimTypes.TasksManage, ClaimTypes.TasksViewAll]);
        var task = await Data.TaskAsync(_project, _member, "Task", assignee: _member);

        (await SendCommandAsync(editor.UserName, HttpMethod.Put, $"/api/task/{task.Id}",
                new { title = "Task", status = "ToDo", priority = "Low", assigneeId = _colleague.Id, rowVersion = task.RowVersion }))
            .ShouldBeError(ErrorMessages.AssigningRequiresClaim);

        await SendCommandExpectingDoneAsync(editor.UserName, HttpMethod.Put, $"/api/task/{task.Id}",
            new { title = "Renamed", status = "ToDo", priority = "Low", assigneeId = _member.Id, rowVersion = task.RowVersion });
        (await Data.FindIncludingDeletedAsync<TaskEntity>(task.Id))!.AssigneeId.Should().Be(_member.Id);
    }

    [Fact]
    public async Task ChangeStatus_UpdatesOnlyTheStatus()
    {
        var task = await Data.TaskAsync(_project, _member, "Task", TaskStatus.ToDo, TaskPriority.High, assignee: _member);

        await SendCommandExpectingDoneAsync(_member.UserName, HttpMethod.Put, $"/api/task/{task.Id}/status", new { status = "InProgress" });

        var stored = (await Data.FindIncludingDeletedAsync<TaskEntity>(task.Id))!;
        stored.Status.Should().Be(TaskStatus.InProgress);
        stored.Priority.Should().Be(TaskPriority.High);
        stored.AssigneeId.Should().Be(_member.Id);
    }

    [Fact]
    public async Task ChangeStatus_WithoutAStatus_IsRefused_InsteadOfResettingTheTask()
    {
        var task = await Data.TaskAsync(_project, _member, "Task", TaskStatus.InProgress);

        var misspelled = await SendAsync(_member.UserName, HttpMethod.Put, $"/api/task/{task.Id}/status", new { state = "Done" });

        await misspelled.ShouldHaveStatusAsync(HttpStatusCode.BadRequest);
        (await Data.FindIncludingDeletedAsync<TaskEntity>(task.Id))!.Status.Should().Be(TaskStatus.InProgress);
    }

    [Fact]
    public async Task Create_WithoutStatusOrPriority_IsRefused()
    {
        var response = await SendAsync(_member.UserName, HttpMethod.Post, "/api/task", new { projectId = _project.Id, title = "Task" });

        await response.ShouldHaveStatusAsync(HttpStatusCode.BadRequest);
        (await Data.QueryAsync(dbContext => dbContext.Tasks.CountAsync())).Should().Be(0);
    }

    [Fact]
    public async Task Assign_AssignsATeamMember_AndUnassignsWithNull()
    {
        var task = await Data.TaskAsync(_project, _member);

        await SendCommandExpectingDoneAsync(_member.UserName, HttpMethod.Put, $"/api/task/{task.Id}/assignee", new { assigneeId = _colleague.Id });
        (await Data.FindIncludingDeletedAsync<TaskEntity>(task.Id))!.AssigneeId.Should().Be(_colleague.Id);

        await SendCommandExpectingDoneAsync(_member.UserName, HttpMethod.Put, $"/api/task/{task.Id}/assignee", new { assigneeId = (Guid?)null });
        (await Data.FindIncludingDeletedAsync<TaskEntity>(task.Id))!.AssigneeId.Should().BeNull();
    }

    [Fact]
    public async Task Assign_ToSomeoneOutsideTheTeam_IsRefused()
    {
        var task = await Data.TaskAsync(_project, _member);

        var notification = await SendCommandAsync(_member.UserName, HttpMethod.Put, $"/api/task/{task.Id}/assignee",
            new { assigneeId = (await Data.UserAsync()).Id });

        notification.ShouldBeError(ErrorMessages.AssigneeMustBeTeamMember);
    }

    [Fact]
    public async Task Delete_SoftDeletesTheTask()
    {
        var task = await Data.TaskAsync(_project, _member);

        var notification = await SendCommandExpectingDoneAsync(_member.UserName, HttpMethod.Delete, $"/api/task/{task.Id}");

        notification.ActionType.Should().Be(ActionType.Deleted);
        (await Data.FindIncludingDeletedAsync<TaskEntity>(task.Id))!.DateTimeDeleted.Should().NotBeNull();
        (await GetAsync<FilteringResult<TaskRowDto>>(_member.UserName, "/api/task")).Total.Should().Be(0);
    }

    [Fact]
    public async Task Update_OfADeletedTask_IsNotFound()
    {
        var task = await Data.TaskAsync(_project, _member, "Deleted task");
        await SendCommandExpectingDoneAsync(_member.UserName, HttpMethod.Delete, $"/api/task/{task.Id}");

        var notification = await SendCommandAsync(_member.UserName, HttpMethod.Put, $"/api/task/{task.Id}/status", new { status = "Done" });

        notification.ShouldBeError(ErrorMessages.TaskNotFound);
        (await Data.FindIncludingDeletedAsync<TaskEntity>(task.Id))!.Status.Should().Be(TaskStatus.ToDo);
    }

    [Fact]
    public async Task TasksOfAnotherTeam_CannotBeChanged()
    {
        var outsider = await Data.UserAsync();
        var otherTask = await Data.TaskAsync(await Data.ProjectAsync(await Data.TeamAsync([outsider])), outsider, "Team B task");

        (await SendCommandAsync(_member.UserName, HttpMethod.Put, $"/api/task/{otherTask.Id}",
            new { title = "Hijacked", status = "Done", priority = "Low", rowVersion = otherTask.RowVersion })).ShouldBeError(ErrorMessages.TaskNotFound);
        (await SendCommandAsync(_member.UserName, HttpMethod.Put, $"/api/task/{otherTask.Id}/status", new { status = "Done" }))
            .ShouldBeError(ErrorMessages.TaskNotFound);
        (await SendCommandAsync(_member.UserName, HttpMethod.Put, $"/api/task/{otherTask.Id}/assignee", new { assigneeId = (Guid?)null }))
            .ShouldBeError(ErrorMessages.TaskNotFound);
        (await SendCommandAsync(_member.UserName, HttpMethod.Delete, $"/api/task/{otherTask.Id}"))
            .ShouldBeError(ErrorMessages.TaskNotFound);

        var stored = (await Data.FindIncludingDeletedAsync<TaskEntity>(otherTask.Id))!;
        stored.Title.Should().Be("Team B task");
        stored.DateTimeDeleted.Should().BeNull();
        await (await SendAsync(_member.UserName, HttpMethod.Get, $"/api/task/{otherTask.Id}")).ShouldHaveStatusAsync(HttpStatusCode.NotFound);
    }
}
