using System.Linq.Expressions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Filtering;
using TaskFlow.Application.Common.Filtering.Enums;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Common.Scopes;
using TaskFlow.Application.Interfaces.DataReaders;
using TaskFlow.Application.Queries.Tasks;
using TaskFlow.Domain.Constants;
using TaskFlow.Domain.Entities.Tasks;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Exceptions;
using TaskFlow.Infrastructure.Helper;
using TaskFlow.Infrastructure.Persistence.Extensions;
using TaskFlow.Infrastructure.Services;
using TaskStatus = TaskFlow.Domain.Enums.TaskStatus;

namespace TaskFlow.Infrastructure.Persistence.DataReaders;

internal sealed class TaskDataReader(
    TaskFlowDbContext dbContext,
    ICurrentUserRetriever currentUserRetriever,
    BusinessCalendar calendar,
    IValidator<GetTasksQuery> tasksQueryValidator,
    IValidator<GetMyTasksQuery> myTasksQueryValidator,
    IValidator<GetTaskBoardQuery> boardQueryValidator) : ITaskDataReader
{
    private static readonly SortMap<TaskRowDto> SortMap = new SortMap<TaskRowDto>()
        .Add(TaskSortFields.Title, task => task.Title)
        .Add(TaskSortFields.Status, task => task.Status)
        .Add(TaskSortFields.Priority, task => task.Priority)
        .AddNullsLast(TaskSortFields.DueDate, task => task.DueDate, task => task.DueDate == null)
        .Add(TaskSortFields.DateTimeCreated, task => task.DateTimeCreated);

    private static readonly OrderBy DefaultOrder = new() { Field = TaskSortFields.DueDate, Dir = OrderDirection.Asc };

    public async Task<FilteringResult<TaskRowDto>> GetGridAsync(GetTasksQuery query)
    {
        await tasksQueryValidator.ValidateAndThrowAsync(query);

        var today = calendar.Today;
        var tasks = ApplyFilters(VisibleTasks(currentUserRetriever.GetAuthenticatedCurrentUser()), query, today);

        return await tasks
            .Select(RowProjection(today))
            .ToFilteringResultAsync(query, SortMap, DefaultOrder, task => task.Id);
    }

    public async Task<TaskDetailDto> GetByIdAsync(Guid id)
    {
        var today = calendar.Today;

        return await VisibleTasks(currentUserRetriever.GetAuthenticatedCurrentUser())
                   .Where(task => task.Id == id)
                   .Select(task => new TaskDetailDto
                   {
                       Id = task.Id,
                       ProjectId = task.ProjectId,
                       ProjectName = task.Project!.Name,
                       TeamId = task.Project.TeamId,
                       TeamName = task.Project.Team!.Name,
                       Title = task.Title,
                       Description = task.Description,
                       Status = task.Status,
                       Priority = task.Priority,
                       DueDate = task.DueDate,
                       IsOverdue = task.Status != TaskStatus.Done && task.DueDate < today,
                       AssigneeId = task.AssigneeId,
                       AssigneeName = task.Assignee != null ? task.Assignee.DisplayName : null,
                       CreatedById = task.CreatedById,
                       DateTimeCreated = task.DateTimeCreated,
                       DateTimeModified = task.DateTimeModified,
                       RowVersion = task.RowVersion
                   })
                   .SingleOrDefaultAsync()
               ?? throw new TaskFlowNotFoundException(ErrorMessages.TaskNotFound);
    }

    public async Task<FilteringResult<TaskRowDto>> GetMyTasksAsync(GetMyTasksQuery query)
    {
        await myTasksQueryValidator.ValidateAndThrowAsync(query);

        var currentUser = currentUserRetriever.GetAuthenticatedCurrentUser();
        var userId = currentUser.Id;
        var today = calendar.Today;

        var tasks = VisibleTasks(currentUser)
            .Where(task => task.AssigneeId == userId && task.Status != TaskStatus.Done);

        return await ApplySearch(tasks, query.Query)
            .Select(RowProjection(today))
            .ToFilteringResultAsync(query, SortMap, DefaultOrder, task => task.Id);
    }

    /// <summary>Dashboard "Tasks per status": counted in SQL over every task the user can see.</summary>
    public async Task<TaskStatusCountsDto> GetStatusCountsAsync()
    {
        var today = calendar.Today;

        var counts = await VisibleTasks(currentUserRetriever.GetAuthenticatedCurrentUser())
            .GroupBy(_ => 1)
            .Select(tasks => new TaskStatusCountsDto
            {
                ToDo = tasks.Count(task => task.Status == TaskStatus.ToDo),
                InProgress = tasks.Count(task => task.Status == TaskStatus.InProgress),
                Done = tasks.Count(task => task.Status == TaskStatus.Done),
                Overdue = tasks.Count(task => task.Status != TaskStatus.Done && task.DueDate < today)
            })
            .SingleOrDefaultAsync();

        return counts ?? new TaskStatusCountsDto();
    }

    public async Task<TaskBoardDto> GetBoardAsync(GetTaskBoardQuery query)
    {
        await boardQueryValidator.ValidateAndThrowAsync(query);

        var currentUser = currentUserRetriever.GetAuthenticatedCurrentUser();
        var project = await dbContext.Projects
                          .AsNoTracking()
                          .Where(TaskScope.ProjectsVisibleTo(currentUser))
                          .Where(project => project.Id == query.ProjectId)
                          .Select(project => new { project.Id, project.Name })
                          .SingleOrDefaultAsync()
                      ?? throw new TaskFlowNotFoundException(ErrorMessages.ProjectNotFound);

        var tasks = dbContext.Tasks.AsNoTracking().Where(task => task.ProjectId == project.Id);
        tasks = ApplyAssigneeAndPriorityFilters(tasks, query.Priority, query.AssigneeId, query.Unassigned);

        var today = calendar.Today;
        var cards = await tasks
            .OrderBy(task => task.DueDate == null)
            .ThenBy(task => task.DueDate)
            .ThenBy(task => task.Title)
            .ThenBy(task => task.Id)
            .Select(task => new TaskCardDto
            {
                Id = task.Id,
                Title = task.Title,
                Status = task.Status,
                Priority = task.Priority,
                DueDate = task.DueDate,
                IsOverdue = task.Status != TaskStatus.Done && task.DueDate < today,
                AssigneeId = task.AssigneeId,
                AssigneeName = task.Assignee != null ? task.Assignee.DisplayName : null
            })
            .ToListAsync();

        return new TaskBoardDto
        {
            ProjectId = project.Id,
            ProjectName = project.Name,
            Columns = Enum.GetValues<TaskStatus>()
                .Select(status => BuildColumn(status, cards))
                .ToList()
        };
    }

    private static TaskBoardColumnDto BuildColumn(TaskStatus status, List<TaskCardDto> cards)
    {
        var columnCards = cards.Where(card => card.Status == status).ToList();

        return new TaskBoardColumnDto
        {
            Status = status,
            Count = columnCards.Count,
            Tasks = columnCards
        };
    }

    private IQueryable<TaskEntity> VisibleTasks(IUserScope user)
    {
        return dbContext.Tasks.AsNoTracking().Where(TaskScope.VisibleTo(user));
    }

    private static IQueryable<TaskEntity> ApplyFilters(IQueryable<TaskEntity> tasks, GetTasksQuery query, DateOnly today)
    {
        if (query.ProjectId is { } projectId)
        {
            tasks = tasks.Where(task => task.ProjectId == projectId);
        }

        if (query.Status is { } status)
        {
            tasks = tasks.Where(task => task.Status == status);
        }

        tasks = ApplyAssigneeAndPriorityFilters(tasks, query.Priority, query.AssigneeId, query.Unassigned);

        if (query.IsOverdue is { } isOverdue)
        {
            tasks = isOverdue
                ? tasks.Where(task => task.Status != TaskStatus.Done && task.DueDate < today)
                : tasks.Where(task => task.Status == TaskStatus.Done || task.DueDate == null || task.DueDate >= today);
        }

        if (query.DueFrom is { } dueFrom)
        {
            tasks = tasks.Where(task => task.DueDate >= dueFrom);
        }

        if (query.DueTo is { } dueTo)
        {
            tasks = tasks.Where(task => task.DueDate <= dueTo);
        }

        return ApplySearch(tasks, query.Query);
    }

    private static IQueryable<TaskEntity> ApplySearch(IQueryable<TaskEntity> tasks, string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return tasks;
        }

        var pattern = StringHelper.ToContainsPattern(search);
        return tasks.Where(task => EF.Functions.Like(task.Title, pattern)
                                   || (task.Description != null && EF.Functions.Like(task.Description, pattern)));
    }

    private static IQueryable<TaskEntity> ApplyAssigneeAndPriorityFilters(
        IQueryable<TaskEntity> tasks,
        TaskPriority? priority,
        Guid? assigneeId,
        bool unassigned)
    {
        if (priority is { } taskPriority)
        {
            tasks = tasks.Where(task => task.Priority == taskPriority);
        }

        if (assigneeId is { } userId)
        {
            tasks = tasks.Where(task => task.AssigneeId == userId);
        }

        if (unassigned)
        {
            tasks = tasks.Where(task => task.AssigneeId == null);
        }

        return tasks;
    }

    private static Expression<Func<TaskEntity, TaskRowDto>> RowProjection(DateOnly today)
    {
        return task => new TaskRowDto
        {
            Id = task.Id,
            ProjectId = task.ProjectId,
            ProjectName = task.Project!.Name,
            TeamId = task.Project.TeamId,
            Title = task.Title,
            Status = task.Status,
            Priority = task.Priority,
            DueDate = task.DueDate,
            IsOverdue = task.Status != TaskStatus.Done && task.DueDate < today,
            AssigneeId = task.AssigneeId,
            AssigneeName = task.Assignee != null ? task.Assignee.DisplayName : null,
            DateTimeCreated = task.DateTimeCreated,
            RowVersion = task.RowVersion
        };
    }
}
