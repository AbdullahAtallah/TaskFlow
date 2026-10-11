using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Filtering;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Common.Scopes;
using TaskFlow.Application.Interfaces.DataReaders;
using TaskFlow.Application.Queries.Teams;
using TaskFlow.Domain.Constants;
using TaskFlow.Domain.Entities.Teams;
using TaskFlow.Domain.Exceptions;
using TaskFlow.Infrastructure.Helper;
using TaskFlow.Infrastructure.Persistence.Extensions;
using TaskFlow.Infrastructure.Services;

namespace TaskFlow.Infrastructure.Persistence.DataReaders;

internal sealed class TeamDataReader(
    TaskFlowDbContext dbContext,
    ICurrentUserRetriever currentUserRetriever,
    IValidator<GetTeamsQuery> validator) : ITeamDataReader
{
    private static readonly SortMap<TeamRowDto> SortMap = new SortMap<TeamRowDto>()
        .Add(TeamSortFields.Name, team => team.Name)
        .Add(TeamSortFields.MemberCount, team => team.MemberCount)
        .Add(TeamSortFields.ProjectCount, team => team.ProjectCount)
        .Add(TeamSortFields.DateTimeCreated, team => team.DateTimeCreated);

    private static readonly OrderBy DefaultOrder = new() { Field = TeamSortFields.Name };

    public async Task<FilteringResult<TeamRowDto>> GetGridAsync(GetTeamsQuery query)
    {
        await validator.ValidateAndThrowAsync(query);

        var currentUser = currentUserRetriever.GetAuthenticatedCurrentUser();
        var teams = VisibleTeams(currentUser);

        if (query.OnlyMine)
        {
            var userId = currentUser.Id;
            teams = teams.Where(team => team.Members.Any(member => member.UserId == userId));
        }

        if (!string.IsNullOrWhiteSpace(query.Query))
        {
            var pattern = StringHelper.ToContainsPattern(query.Query);
            teams = teams.Where(team => EF.Functions.Like(team.Name, pattern)
                                        || (team.Description != null && EF.Functions.Like(team.Description, pattern)));
        }

        return await teams
            .Select(team => new TeamRowDto
            {
                Id = team.Id,
                Name = team.Name,
                Description = team.Description,
                MemberCount = team.Members.Count,
                ProjectCount = team.Projects.Count,
                DateTimeCreated = team.DateTimeCreated,
                RowVersion = team.RowVersion
            })
            .ToFilteringResultAsync(query, SortMap, DefaultOrder, team => team.Id);
    }

    public async Task<TeamDetailDto> GetByIdAsync(Guid id)
    {
        var currentUser = currentUserRetriever.GetAuthenticatedCurrentUser();

        return await VisibleTeams(currentUser)
                   .Where(team => team.Id == id)
                   .Select(team => new TeamDetailDto
                   {
                       Id = team.Id,
                       Name = team.Name,
                       Description = team.Description,
                       MemberCount = team.Members.Count,
                       ProjectCount = team.Projects.Count,
                       DateTimeCreated = team.DateTimeCreated,
                       DateTimeModified = team.DateTimeModified,
                       RowVersion = team.RowVersion
                   })
                   .SingleOrDefaultAsync()
               ?? throw new TaskFlowNotFoundException(ErrorMessages.TeamNotFound);
    }

    public async Task<IReadOnlyList<TeamMemberDto>> GetMembersAsync(Guid teamId)
    {
        var currentUser = currentUserRetriever.GetAuthenticatedCurrentUser();

        if (!await VisibleTeams(currentUser).AnyAsync(team => team.Id == teamId))
        {
            throw new TaskFlowNotFoundException(ErrorMessages.TeamNotFound);
        }

        return await dbContext.TeamMembers
            .AsNoTracking()
            .Where(member => member.TeamId == teamId)
            .OrderBy(member => member.User!.DisplayName)
            .ThenBy(member => member.UserId)
            .Select(member => new TeamMemberDto
            {
                UserId = member.UserId,
                DisplayName = member.User!.DisplayName,
                UserName = member.User.UserName,
                IsActive = member.User.IsActive,
                Roles = member.User.UserRoles
                    .Select(userRole => userRole.Role!.Name)
                    .OrderBy(roleName => roleName)
                    .ToList()
            })
            .ToListAsync();
    }

    public Task<bool> IsNameInUseAsync(string name, Guid? excludeTeamId)
    {
        return dbContext.Teams.AnyAsync(team => team.Name == name && team.Id != excludeTeamId);
    }

    public Task<bool> HasProjectsAsync(Guid teamId)
    {
        return dbContext.Projects.AnyAsync(project => project.TeamId == teamId);
    }

    private IQueryable<TeamEntity> VisibleTeams(IUserScope user)
    {
        return dbContext.Teams.AsNoTracking().Where(TeamScope.VisibleTo(user));
    }
}
