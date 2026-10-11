using System.Linq.Expressions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Filtering;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Common.Scopes;
using TaskFlow.Application.Interfaces.DataReaders;
using TaskFlow.Application.Queries.Projects;
using TaskFlow.Domain.Constants;
using TaskFlow.Domain.Entities.Projects;
using TaskFlow.Domain.Exceptions;
using TaskFlow.Infrastructure.Helper;
using TaskFlow.Infrastructure.Persistence.Extensions;
using TaskFlow.Infrastructure.Services;
using TaskStatus = TaskFlow.Domain.Enums.TaskStatus;

namespace TaskFlow.Infrastructure.Persistence.DataReaders;

internal sealed class ProjectDataReader(
    TaskFlowDbContext dbContext,
    ICurrentUserRetriever currentUserRetriever,
    BusinessCalendar calendar,
    IValidator<GetProjectsQuery> validator) : IProjectDataReader
{
    private static readonly SortMap<ProjectRowDto> SortMap = new SortMap<ProjectRowDto>()
        .Add(ProjectSortFields.Name, project => project.Name)
        .Add(ProjectSortFields.TeamName, project => project.TeamName)
        .Add(ProjectSortFields.TaskCount, project => project.TaskCount)
        .Add(ProjectSortFields.OpenTaskCount, project => project.OpenTaskCount)
        .Add(ProjectSortFields.DateTimeCreated, project => project.DateTimeCreated);

    private static readonly OrderBy DefaultOrder = new() { Field = ProjectSortFields.Name };

    public async Task<FilteringResult<ProjectRowDto>> GetGridAsync(GetProjectsQuery query)
    {
        await validator.ValidateAndThrowAsync(query);

        var projects = VisibleProjects(currentUserRetriever.GetAuthenticatedCurrentUser());

        if (query.TeamId is { } teamId)
        {
            projects = projects.Where(project => project.TeamId == teamId);
        }

        if (!string.IsNullOrWhiteSpace(query.Query))
        {
            var pattern = StringHelper.ToContainsPattern(query.Query);
            projects = projects.Where(project => EF.Functions.Like(project.Name, pattern)
                                                 || (project.Description != null && EF.Functions.Like(project.Description, pattern)));
        }

        return await projects
            .Select(RowProjection(calendar.Today))
            .ToFilteringResultAsync(query, SortMap, DefaultOrder, project => project.Id);
    }

    public async Task<ProjectDetailDto> GetByIdAsync(Guid id)
    {
        return await VisibleProjects(currentUserRetriever.GetAuthenticatedCurrentUser())
                   .Where(project => project.Id == id)
                   .Select(project => new ProjectDetailDto
                   {
                       Id = project.Id,
                       Name = project.Name,
                       Description = project.Description,
                       TeamId = project.TeamId,
                       TeamName = project.Team!.Name,
                       TaskCount = project.Tasks.Count,
                       OpenTaskCount = project.Tasks.Count(task => task.Status != TaskStatus.Done),
                       DoneTaskCount = project.Tasks.Count(task => task.Status == TaskStatus.Done),
                       DateTimeCreated = project.DateTimeCreated,
                       DateTimeModified = project.DateTimeModified,
                       RowVersion = project.RowVersion
                   })
                   .SingleOrDefaultAsync()
               ?? throw new TaskFlowNotFoundException(ErrorMessages.ProjectNotFound);
    }

    /// <summary>Dashboard: the visible teams, each with the number of its projects the user sees.</summary>
    public async Task<IReadOnlyList<ProjectCountPerTeamDto>> GetCountsPerTeamAsync()
    {
        var currentUser = currentUserRetriever.GetAuthenticatedCurrentUser();
        var projectScope = ProjectScope.VisibleTo(currentUser);

        return await dbContext.Teams
            .AsNoTracking()
            .Where(TeamScope.VisibleTo(currentUser))
            .OrderBy(team => team.Name)
            .Select(team => new ProjectCountPerTeamDto
            {
                TeamId = team.Id,
                TeamName = team.Name,
                ProjectCount = team.Projects.AsQueryable().Count(projectScope)
            })
            .ToListAsync();
    }

    private IQueryable<ProjectEntity> VisibleProjects(IUserScope user)
    {
        return dbContext.Projects.AsNoTracking().Where(ProjectScope.VisibleTo(user));
    }

    private static Expression<Func<ProjectEntity, ProjectRowDto>> RowProjection(DateOnly today)
    {
        return project => new ProjectRowDto
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            TeamId = project.TeamId,
            TeamName = project.Team!.Name,
            TaskCount = project.Tasks.Count,
            OpenTaskCount = project.Tasks.Count(task => task.Status != TaskStatus.Done),
            DoneTaskCount = project.Tasks.Count(task => task.Status == TaskStatus.Done),
            HasOverdueTasks = project.Tasks.Any(task => task.Status != TaskStatus.Done && task.DueDate < today),
            DateTimeCreated = project.DateTimeCreated,
            RowVersion = project.RowVersion
        };
    }
}
