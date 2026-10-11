using FluentValidation.TestHelper;
using TaskFlow.Application.Common.Filtering;
using TaskFlow.Application.Common.Filtering.Enums;
using TaskFlow.Application.Queries.Projects;
using TaskFlow.Application.Queries.Tasks;
using TaskFlow.Application.Queries.Teams;
using TaskFlow.Application.Queries.Users;
using TaskStatus = TaskFlow.Domain.Enums.TaskStatus;

namespace TaskFlow.Application.UnitTests.Validators;

public sealed class QueryValidatorTests
{
    private readonly GetTeamsQueryValidator _teamsValidator = new();
    private readonly GetTasksQueryValidator _tasksValidator = new();

    [Fact]
    public void Paging_WithDefaults_IsValid()
    {
        _teamsValidator.TestValidate(new GetTeamsQuery()).ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(FilterLimits.MaxTake + 1)]
    public void Paging_RejectsATakeOutsideTheLimits(int take)
    {
        _teamsValidator.TestValidate(new GetTeamsQuery { Take = take })
            .ShouldHaveValidationErrorFor(query => query.Take)
            .WithErrorMessage($"Take must be between 1 and {FilterLimits.MaxTake}.");
    }

    [Fact]
    public void Paging_RejectsANegativeSkip_AndATooLongSearchText()
    {
        var result = _teamsValidator.TestValidate(new GetTeamsQuery { Skip = -1, Query = new string('q', FilterLimits.MaxQueryLength + 1) });

        result.ShouldHaveValidationErrorFor(query => query.Skip);
        result.ShouldHaveValidationErrorFor(query => query.Query);
    }

    [Fact]
    public void Sorting_AcceptsTheGridColumns_IgnoringCase()
    {
        _teamsValidator.TestValidate(new GetTeamsQuery
            {
                OrderByFields = [new OrderBy { Field = "MemberCount", Dir = OrderDirection.Desc }, new OrderBy { Field = "name" }]
            })
            .ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Sorting_RejectsAnUnknownField_AndNamesTheSupportedOnes()
    {
        _teamsValidator.TestValidate(new GetTeamsQuery { OrderByFields = [new OrderBy { Field = "password" }] })
            .ShouldHaveValidationErrorFor("OrderByFields[0].Field")
            .WithErrorMessage("Sorting on 'password' is not supported. Supported fields: name, memberCount, projectCount, dateTimeCreated.");
    }

    [Fact]
    public void Sorting_RejectsAnUnknownDirection_AndTooManyFields()
    {
        _teamsValidator.TestValidate(new GetTeamsQuery { OrderByFields = [new OrderBy { Field = "name", Dir = (OrderDirection)5 }] })
            .ShouldHaveValidationErrorFor("OrderByFields[0].Dir");
        _teamsValidator.TestValidate(new GetTeamsQuery
            {
                OrderByFields = Enumerable.Repeat(new OrderBy { Field = "name" }, FilterLimits.MaxOrderByFields + 1).ToList()
            })
            .ShouldHaveValidationErrorFor(query => query.OrderByFields);
    }

    [Fact]
    public void Tasks_CannotFilterOnAnAssigneeAndOnUnassignedTasksTogether()
    {
        _tasksValidator.TestValidate(new GetTasksQuery { AssigneeId = Guid.NewGuid(), Unassigned = true })
            .ShouldHaveValidationErrorFor(query => query.Unassigned);
    }

    [Fact]
    public void Tasks_RejectAnInvertedDueDateRange_ButAcceptASingleDay()
    {
        var day = new DateOnly(2026, 10, 1);

        _tasksValidator.TestValidate(new GetTasksQuery { DueFrom = day, DueTo = day.AddDays(-1) })
            .ShouldHaveValidationErrorFor(query => query.DueTo);
        _tasksValidator.TestValidate(new GetTasksQuery { DueFrom = day, DueTo = day })
            .ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Tasks_RejectAnUnknownStatus()
    {
        _tasksValidator.TestValidate(new GetTasksQuery { Status = (TaskStatus)8 })
            .ShouldHaveValidationErrorFor(query => query.Status);
    }

    [Fact]
    public void Tasks_AcceptSortingOnTheDueDate()
    {
        _tasksValidator.TestValidate(new GetTasksQuery { OrderByFields = [new OrderBy { Field = TaskSortFields.DueDate }] })
            .ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Board_RequiresAProject()
    {
        new GetTaskBoardQueryValidator().TestValidate(new GetTaskBoardQuery())
            .ShouldHaveValidationErrorFor(query => query.ProjectId);
    }

    [Fact]
    public void Projects_RejectAnEmptyTeamFilter()
    {
        new GetProjectsQueryValidator().TestValidate(new GetProjectsQuery { TeamId = Guid.Empty })
            .ShouldHaveValidationErrorFor(query => query.TeamId);
    }

    [Fact]
    public void UserSelectList_LimitsTake()
    {
        new GetUserSelectListQueryValidator().TestValidate(new GetUserSelectListQuery { Take = 0 })
            .ShouldHaveValidationErrorFor(query => query.Take);
    }

    [Fact]
    public void Users_RejectAnEmptyRoleFilter()
    {
        new GetUsersQueryValidator().TestValidate(new GetUsersQuery { RoleId = Guid.Empty })
            .ShouldHaveValidationErrorFor(query => query.RoleId);
    }
}
