using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.DTOs;
using TaskFlow.Application.Common.Filtering;
using TaskFlow.Application.Identity;
using TaskFlow.Application.Interfaces.DataReaders;
using TaskFlow.Application.Queries.Users;
using TaskFlow.Domain.Constants;
using TaskFlow.Domain.Entities.Users;
using TaskFlow.Domain.Exceptions;
using TaskFlow.Infrastructure.Helper;
using TaskFlow.Infrastructure.Persistence.Extensions;
using TaskFlow.Infrastructure.Services;

namespace TaskFlow.Infrastructure.Persistence.DataReaders;

internal sealed class UserDataReader(
    TaskFlowDbContext dbContext,
    ICurrentUserRetriever currentUserRetriever,
    IValidator<GetUsersQuery> usersQueryValidator,
    IValidator<GetUserSelectListQuery> selectListQueryValidator) : IUserDataReader
{
    private static readonly SortMap<UserRowDto> SortMap = new SortMap<UserRowDto>()
        .Add(UserSortFields.DisplayName, user => user.DisplayName)
        .Add(UserSortFields.UserName, user => user.UserName)
        .Add(UserSortFields.IsActive, user => user.IsActive)
        .Add(UserSortFields.DateTimeCreated, user => user.DateTimeCreated);

    private static readonly OrderBy DefaultOrder = new() { Field = UserSortFields.DisplayName };

    public async Task<FilteringResult<UserRowDto>> GetGridAsync(GetUsersQuery query)
    {
        await usersQueryValidator.ValidateAndThrowAsync(query);

        // Roles and teams are loaded with one query each instead of one joined (cartesian) result.
        var users = dbContext.Users.AsNoTracking().AsSplitQuery();

        if (query.RoleId is { } roleId)
        {
            users = users.Where(user => user.UserRoles.Any(userRole => userRole.RoleId == roleId));
        }

        if (query.IsActive is { } isActive)
        {
            users = users.Where(user => user.IsActive == isActive);
        }

        users = ApplySearch(users, query.Query);

        return await users
            .Select(user => new UserRowDto
            {
                Id = user.Id,
                UserName = user.UserName,
                DisplayName = user.DisplayName,
                IsActive = user.IsActive,
                Roles = user.UserRoles
                    .OrderBy(userRole => userRole.Role!.Name)
                    .Select(userRole => new EntityReferenceDto(userRole.RoleId, userRole.Role!.Name))
                    .ToList(),
                Teams = user.TeamMemberships
                    .OrderBy(member => member.Team!.Name)
                    .Select(member => new EntityReferenceDto(member.TeamId, member.Team!.Name))
                    .ToList(),
                DateTimeCreated = user.DateTimeCreated,
                RowVersion = user.RowVersion
            })
            .ToFilteringResultAsync(query, SortMap, DefaultOrder, user => user.Id);
    }

    public async Task<UserDetailDto> GetByIdAsync(Guid id)
    {
        return await dbContext.Users
                   .AsNoTracking()
                   .AsSplitQuery()
                   .Where(user => user.Id == id)
                   .Select(user => new UserDetailDto
                   {
                       Id = user.Id,
                       UserName = user.UserName,
                       DisplayName = user.DisplayName,
                       IsActive = user.IsActive,
                       Roles = user.UserRoles
                           .OrderBy(userRole => userRole.Role!.Name)
                           .Select(userRole => new EntityReferenceDto(userRole.RoleId, userRole.Role!.Name))
                           .ToList(),
                       Teams = user.TeamMemberships
                           .OrderBy(member => member.Team!.Name)
                           .Select(member => new EntityReferenceDto(member.TeamId, member.Team!.Name))
                           .ToList(),
                       DateTimeCreated = user.DateTimeCreated,
                       DateTimeModified = user.DateTimeModified,
                       RowVersion = user.RowVersion
                   })
                   .SingleOrDefaultAsync()
               ?? throw new TaskFlowNotFoundException(ErrorMessages.UserNotFound);
    }

    /// <summary>
    /// Active users for the pickers. With a team id: the members of that team, when the current user can see it.
    /// Without one: every user for team managers, otherwise only the users who share a team with the current user.
    /// </summary>
    public async Task<IReadOnlyList<UserSelectItemDto>> GetSelectListAsync(GetUserSelectListQuery query)
    {
        await selectListQueryValidator.ValidateAndThrowAsync(query);

        var currentUser = currentUserRetriever.GetAuthenticatedCurrentUser();
        var currentUserId = currentUser.Id;
        var users = dbContext.Users.AsNoTracking().Where(user => user.IsActive);

        if (query.TeamId is { } teamId)
        {
            if (!await CanSeeTeamAsync(currentUser, teamId))
            {
                return [];
            }

            users = users.Where(user => user.TeamMemberships.Any(member => member.TeamId == teamId));
        }
        else if (!currentUser.HasClaim(ClaimTypes.TeamsManage) && !currentUser.HasClaim(ClaimTypes.TeamsMembersManage))
        {
            users = users.Where(user => user.TeamMemberships.Any(member =>
                member.Team!.Members.Any(colleague => colleague.UserId == currentUserId)));
        }

        return await ApplySearch(users, query.Query)
            .OrderBy(user => user.DisplayName)
            .ThenBy(user => user.Id)
            .Take(query.Take)
            .Select(user => new UserSelectItemDto
            {
                Id = user.Id,
                DisplayName = user.DisplayName,
                UserName = user.UserName
            })
            .ToListAsync();
    }

    public Task<bool> IsUserNameInUseAsync(string userName)
    {
        return dbContext.Users.AnyAsync(user => user.UserName == userName);
    }

    /// <summary>Whoever can see the projects or tasks of a team may list its members, e.g. to assign a task.</summary>
    private async Task<bool> CanSeeTeamAsync(CurrentUser currentUser, Guid teamId)
    {
        if (currentUser.HasClaim(ClaimTypes.TeamsViewAll)
            || currentUser.HasClaim(ClaimTypes.ProjectsViewAll)
            || currentUser.HasClaim(ClaimTypes.TasksViewAll))
        {
            return true;
        }

        var currentUserId = currentUser.Id;
        return await dbContext.TeamMembers.AnyAsync(member => member.TeamId == teamId && member.UserId == currentUserId);
    }

    private static IQueryable<UserEntity> ApplySearch(IQueryable<UserEntity> users, string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return users;
        }

        var pattern = StringHelper.ToContainsPattern(search);
        return users.Where(user => EF.Functions.Like(user.DisplayName, pattern) || EF.Functions.Like(user.UserName, pattern));
    }
}
