using System.Net;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using TaskFlow.Application.Interfaces;
using TaskFlow.Application.Queries.Tasks;
using TaskFlow.Application.Queries.Teams;
using TaskFlow.Domain.Constants;
using TaskFlow.Domain.Entities.Tasks;
using TaskFlow.Domain.Exceptions;
using TaskFlow.IntegrationTests.Infrastructure;
using TaskStatus = TaskFlow.Domain.Enums.TaskStatus;

namespace TaskFlow.IntegrationTests.Persistence;

/// <summary>
/// Optimistic concurrency: every record has a row version. An edit form sends back the version it showed, so a save
/// over a change that was saved in the meantime is refused instead of silently overwriting it.
/// </summary>
public sealed class ConcurrencyTests(TaskFlowFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task Edit_WithTheVersionTheFormShowed_IsSaved_AndGivesTheRecordANewVersion()
    {
        var team = await Data.TeamAsync(name: "Facilities");
        var form = await GetAsync<TeamDetailDto>(AdminEmail, $"/api/team/{team.Id}");

        await SendCommandExpectingDoneAsync(AdminEmail, HttpMethod.Put, $"/api/team/{team.Id}",
            new { name = "Facility Services", rowVersion = form.RowVersion });

        (await GetAsync<TeamDetailDto>(AdminEmail, $"/api/team/{team.Id}")).RowVersion.Should().NotEqual(form.RowVersion);
    }

    [Fact]
    public async Task Edit_FromAnOutdatedForm_IsRefused_AndKeepsTheChangeSavedFirst()
    {
        var member = await Data.UserAsync();
        var task = await Data.TaskAsync(await Data.ProjectAsync(await Data.TeamAsync([member])), member, "Original title");
        var formOfMember = await GetAsync<TaskDetailDto>(member.UserName, $"/api/task/{task.Id}");
        var formOfAdmin = await GetAsync<TaskDetailDto>(AdminEmail, $"/api/task/{task.Id}");

        await SendCommandExpectingDoneAsync(member.UserName, HttpMethod.Put, $"/api/task/{task.Id}", EditOf(formOfMember, "Member's title"));
        var refused = await SendCommandAsync(AdminEmail, HttpMethod.Put, $"/api/task/{task.Id}", EditOf(formOfAdmin, "Admin's title"));

        refused.ShouldBeError(ErrorMessages.ChangedByAnotherUser);
        (await Data.FindIncludingDeletedAsync<TaskEntity>(task.Id))!.Title.Should().Be("Member's title");
    }

    [Fact]
    public async Task Edit_WithoutARowVersion_Returns400()
    {
        var team = await Data.TeamAsync();

        var response = await SendAsync(AdminEmail, HttpMethod.Put, $"/api/team/{team.Id}", new { name = "No version" });

        await response.ShouldHaveStatusAsync(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ChangingOnlyTheGrantsOfARole_GivesItANewVersion_SoAnOlderFormCannotUndoThem()
    {
        var role = await Data.RoleAsync([PackageTypes.Tasks], [ClaimTypes.TasksCreate], name: "Planner");

        await SendCommandExpectingDoneAsync(AdminEmail, HttpMethod.Put, $"/api/role/{role.Id}", new
        {
            name = "Planner", isDefault = false, packages = new[] { PackageTypes.Tasks },
            claims = new[] { ClaimTypes.TasksCreate, ClaimTypes.TasksManage }, rowVersion = role.RowVersion
        });
        var refused = await SendCommandAsync(AdminEmail, HttpMethod.Put, $"/api/role/{role.Id}", new
        {
            name = "Planner", isDefault = false, packages = new[] { PackageTypes.Tasks },
            claims = new[] { ClaimTypes.TasksCreate }, rowVersion = role.RowVersion
        });

        refused.ShouldBeError(ErrorMessages.ChangedByAnotherUser);
    }

    [Fact]
    public async Task ASave_OverAChangeSavedAfterTheLoad_IsRefusedBySqlServer()
    {
        var member = await Data.UserAsync();
        var task = await Data.TaskAsync(await Data.ProjectAsync(await Data.TeamAsync([member])), member);
        await using var statusChange = Writer();
        await using var assignment = Writer();
        var taskForStatus = await statusChange.LoadAsync(task.Id);
        var taskForAssignee = await assignment.LoadAsync(task.Id);

        taskForStatus.Status = TaskStatus.Done;
        await statusChange.UpdateAsync(taskForStatus);
        taskForAssignee.AssigneeId = member.Id;
        var saveOverTheChange = () => assignment.UpdateAsync(taskForAssignee);

        await saveOverTheChange.Should().ThrowAsync<ConcurrencyConflictException>();
        var stored = (await Data.FindIncludingDeletedAsync<TaskEntity>(task.Id))!;
        stored.Status.Should().Be(TaskStatus.Done);
        stored.AssigneeId.Should().BeNull();
    }

    [Fact]
    public async Task AnUpdate_AfterADeleteSavedSinceTheLoad_IsRefused_AndTheRecordStaysDeleted()
    {
        var member = await Data.UserAsync();
        var task = await Data.TaskAsync(await Data.ProjectAsync(await Data.TeamAsync([member])), member, "Original title");
        await using var edit = Writer();
        await using var delete = Writer();
        var taskToEdit = await edit.LoadAsync(task.Id);

        await delete.DeleteAsync(await delete.LoadAsync(task.Id));
        taskToEdit.Title = "Renamed";
        var saveOverTheDelete = () => edit.UpdateAsync(taskToEdit);

        await saveOverTheDelete.Should().ThrowAsync<ConcurrencyConflictException>();
        var stored = (await Data.FindIncludingDeletedAsync<TaskEntity>(task.Id))!;
        stored.DateTimeDeleted.Should().NotBeNull();
        stored.Title.Should().Be("Original title");
    }

    private static object EditOf(TaskDetailDto form, string title)
    {
        return new
        {
            title,
            description = form.Description,
            status = form.Status,
            priority = form.Priority,
            dueDate = form.DueDate,
            assigneeId = form.AssigneeId,
            rowVersion = form.RowVersion
        };
    }

    private TaskWriter Writer()
    {
        return new TaskWriter(Factory.Services.CreateAsyncScope());
    }

    /// <summary>One command handler's view: a scope with its own DbContext, used through the repository like a handler does.</summary>
    private sealed class TaskWriter(AsyncServiceScope scope) : IAsyncDisposable
    {
        private readonly IRepository<TaskEntity> _repository = scope.ServiceProvider.GetRequiredService<IRepository<TaskEntity>>();
        private readonly IUnitOfWork _unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        public async Task<TaskEntity> LoadAsync(Guid id)
        {
            return (await _repository.GetByIdAsync(id))!;
        }

        public async Task UpdateAsync(TaskEntity task)
        {
            await _repository.UpdateAsync(task);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAsync(TaskEntity task)
        {
            await _repository.DeleteAsync(task);
            await _unitOfWork.SaveChangesAsync();
        }

        public ValueTask DisposeAsync()
        {
            return scope.DisposeAsync();
        }
    }
}
