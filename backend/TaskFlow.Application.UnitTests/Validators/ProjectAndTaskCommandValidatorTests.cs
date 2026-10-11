using FluentValidation.TestHelper;
using TaskFlow.Application.Commands.Projects.CreateProject;
using TaskFlow.Application.Commands.Projects.UpdateProject;
using TaskFlow.Application.Commands.Tasks.AssignTask;
using TaskFlow.Application.Commands.Tasks.ChangeTaskStatus;
using TaskFlow.Application.Commands.Tasks.CreateTask;
using TaskFlow.Application.Commands.Tasks.UpdateTask;
using TaskFlow.Domain.Constants;
using TaskFlow.Domain.Enums;
using TaskStatus = TaskFlow.Domain.Enums.TaskStatus;

namespace TaskFlow.Application.UnitTests.Validators;

public sealed class ProjectAndTaskCommandValidatorTests
{
    private readonly CreateTaskCommandValidator _createTaskValidator = new();

    [Fact]
    public void CreateProject_RequiresATeamAndAName()
    {
        var result = new CreateProjectCommandValidator().TestValidate(new CreateProjectCommand(Guid.Empty, " ", null));

        result.ShouldHaveValidationErrorFor(command => command.TeamId).WithErrorMessage("Team is required.");
        result.ShouldHaveValidationErrorFor(command => command.Name).WithErrorMessage("Name is required.");
    }

    [Fact]
    public void UpdateProject_LimitsTheNameLength()
    {
        new UpdateProjectCommandValidator()
            .TestValidate(new UpdateProjectCommand(Guid.NewGuid(), new string('a', FieldLengths.ProjectName + 1), null, [1]))
            .ShouldHaveValidationErrorFor(command => command.Name);
    }

    [Fact]
    public void CreateTask_WithAllFields_IsValid()
    {
        _createTaskValidator.TestValidate(ValidTask()).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateTask_RequiresAProjectAndATitle()
    {
        var result = _createTaskValidator.TestValidate(ValidTask() with { ProjectId = Guid.Empty, Title = "" });

        result.ShouldHaveValidationErrorFor(command => command.ProjectId);
        result.ShouldHaveValidationErrorFor(command => command.Title).WithErrorMessage("Title is required.");
    }

    [Fact]
    public void CreateTask_RejectsUnknownStatusAndPriorityValues()
    {
        var result = _createTaskValidator.TestValidate(ValidTask() with { Status = (TaskStatus)7, Priority = (TaskPriority)9 });

        result.ShouldHaveValidationErrorFor(command => command.Status);
        result.ShouldHaveValidationErrorFor(command => command.Priority);
    }

    [Fact]
    public void CreateTask_RejectsAnEmptyAssigneeId_ButAllowsNoAssignee()
    {
        _createTaskValidator.TestValidate(ValidTask() with { AssigneeId = Guid.Empty })
            .ShouldHaveValidationErrorFor(command => command.AssigneeId);
        _createTaskValidator.TestValidate(ValidTask() with { AssigneeId = null })
            .ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UpdateTask_RequiresTheTaskId_AndSharesTheTaskFieldRules()
    {
        var result = new UpdateTaskCommandValidator().TestValidate(
            new UpdateTaskCommand(Guid.Empty, new string('a', FieldLengths.TaskTitle + 1), null, TaskStatus.Done, TaskPriority.High, null, null, []));

        result.ShouldHaveValidationErrorFor(command => command.Id);
        result.ShouldHaveValidationErrorFor(command => command.Title);
    }

    [Fact]
    public void ChangeTaskStatus_RejectsAnUnknownStatus()
    {
        new ChangeTaskStatusCommandValidator().TestValidate(new ChangeTaskStatusCommand(Guid.NewGuid(), (TaskStatus)3))
            .ShouldHaveValidationErrorFor(command => command.Status);
    }

    [Fact]
    public void AssignTask_AllowsUnassigning_ButNotAnEmptyId()
    {
        var validator = new AssignTaskCommandValidator();

        validator.TestValidate(new AssignTaskCommand(Guid.NewGuid(), null)).ShouldNotHaveAnyValidationErrors();
        validator.TestValidate(new AssignTaskCommand(Guid.NewGuid(), Guid.Empty)).ShouldHaveValidationErrorFor(command => command.AssigneeId);
    }

    private static CreateTaskCommand ValidTask()
    {
        return new CreateTaskCommand(
            Guid.NewGuid(),
            "Book elevator slots",
            "For the office move",
            TaskStatus.InProgress,
            TaskPriority.High,
            new DateOnly(2026, 10, 3),
            Guid.NewGuid());
    }
}
