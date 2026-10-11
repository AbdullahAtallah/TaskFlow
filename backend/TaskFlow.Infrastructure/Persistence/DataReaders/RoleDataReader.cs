using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskFlow.Application.Common.Filtering;
using TaskFlow.Application.Interfaces.DataReaders;
using TaskFlow.Application.Queries.Roles;
using TaskFlow.Domain.Constants;
using TaskFlow.Domain.Exceptions;
using TaskFlow.Infrastructure.Helper;
using TaskFlow.Infrastructure.Persistence.Extensions;

namespace TaskFlow.Infrastructure.Persistence.DataReaders;

internal sealed class RoleDataReader(
    TaskFlowDbContext dbContext,
    IValidator<GetRolesQuery> rolesQueryValidator,
    IValidator<GetRoleUsersQuery> roleUsersQueryValidator,
    ILogger<RoleDataReader> logger) : IRoleDataReader
{
    private static readonly IReadOnlyList<string> DefaultRoleNames = DefaultRoleBundles.RoleNames;

    private static readonly SortMap<RoleRowDto> RoleSortMap = new SortMap<RoleRowDto>()
        .Add(RoleSortFields.Name, role => role.Name)
        .Add(RoleSortFields.UserCount, role => role.UserCount);

    private static readonly SortMap<RoleUserDto> RoleUserSortMap = new SortMap<RoleUserDto>()
        .Add(RoleUserSortFields.DisplayName, user => user.DisplayName)
        .Add(RoleUserSortFields.UserName, user => user.UserName)
        .Add(RoleUserSortFields.IsActive, user => user.IsActive);

    private static readonly OrderBy DefaultRoleOrder = new() { Field = RoleSortFields.Name };
    private static readonly OrderBy DefaultRoleUserOrder = new() { Field = RoleUserSortFields.DisplayName };

    public async Task<FilteringResult<RoleRowDto>> GetGridAsync(GetRolesQuery query)
    {
        await rolesQueryValidator.ValidateAndThrowAsync(query);

        var roles = dbContext.Roles.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Query))
        {
            var pattern = StringHelper.ToContainsPattern(query.Query);
            roles = roles.Where(role => EF.Functions.Like(role.Name, pattern)
                                        || (role.Description != null && EF.Functions.Like(role.Description, pattern)));
        }

        return await roles
            .Select(role => new RoleRowDto
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description,
                IsDefault = role.IsDefault,
                IsBuiltIn = DefaultRoleNames.Contains(role.Name),
                PackageCount = role.RolePackages.Count,
                ClaimCount = role.RoleClaims.Count,
                UserCount = role.UserRoles.Count,
                RowVersion = role.RowVersion
            })
            .ToFilteringResultAsync(query, RoleSortMap, DefaultRoleOrder, role => role.Id);
    }

    public async Task<RoleDetailDto> GetByIdAsync(Guid id)
    {
        return await dbContext.Roles
                   .AsNoTracking()
                   .AsSplitQuery()
                   .Where(role => role.Id == id)
                   .Select(role => new RoleDetailDto
                   {
                       Id = role.Id,
                       Name = role.Name,
                       Description = role.Description,
                       IsDefault = role.IsDefault,
                       IsBuiltIn = DefaultRoleNames.Contains(role.Name),
                       Packages = role.RolePackages
                           .Select(rolePackage => rolePackage.PackageId)
                           .OrderBy(packageId => packageId)
                           .ToList(),
                       Claims = role.RoleClaims
                           .Select(roleClaim => roleClaim.ClaimId)
                           .OrderBy(claimId => claimId)
                           .ToList(),
                       UserCount = role.UserRoles.Count,
                       DateTimeCreated = role.DateTimeCreated,
                       DateTimeModified = role.DateTimeModified,
                       RowVersion = role.RowVersion
                   })
                   .SingleOrDefaultAsync()
               ?? throw new TaskFlowNotFoundException(ErrorMessages.RoleNotFound);
    }

    public async Task<FilteringResult<RoleUserDto>> GetUsersAsync(Guid roleId, GetRoleUsersQuery query)
    {
        await roleUsersQueryValidator.ValidateAndThrowAsync(query);

        if (!await dbContext.Roles.AnyAsync(role => role.Id == roleId))
        {
            throw new TaskFlowNotFoundException(ErrorMessages.RoleNotFound);
        }

        var users = dbContext.Users
            .AsNoTracking()
            .Where(user => user.UserRoles.Any(userRole => userRole.RoleId == roleId));

        if (!string.IsNullOrWhiteSpace(query.Query))
        {
            var pattern = StringHelper.ToContainsPattern(query.Query);
            users = users.Where(user => EF.Functions.Like(user.DisplayName, pattern) || EF.Functions.Like(user.UserName, pattern));
        }

        return await users
            .Select(user => new RoleUserDto
            {
                Id = user.Id,
                DisplayName = user.DisplayName,
                UserName = user.UserName,
                IsActive = user.IsActive
            })
            .ToFilteringResultAsync(query, RoleUserSortMap, DefaultRoleUserOrder, user => user.Id);
    }

    public Task<bool> IsNameInUseAsync(string name, Guid? excludeRoleId)
    {
        return dbContext.Roles.AnyAsync(role => role.Name == name && role.Id != excludeRoleId);
    }

    public Task<bool> HasUsersAsync(Guid roleId)
    {
        return dbContext.UserRoles.AnyAsync(userRole => userRole.RoleId == roleId);
    }

    public async Task<Guid> GetRoleIdForNewUsersAsync()
    {
        // Roles that grant user and role management never go to someone who registers, whatever their flag says.
        var rolesForNewUsers = dbContext.Roles
            .Where(role => role.RolePackages.All(rolePackage => rolePackage.PackageId != PackageTypes.Management));

        var defaultRoleId = await rolesForNewUsers
            .Where(role => role.IsDefault)
            .OrderBy(role => role.Name)
            .Select(role => (Guid?)role.Id)
            .FirstOrDefaultAsync();

        if (defaultRoleId is { } roleId)
        {
            return roleId;
        }

        logger.LogWarning("No role is marked as the default for new users, so the new user gets the {RoleName} role",
            DefaultRoleBundles.UserRoleName);

        return await rolesForNewUsers
                   .Where(role => role.Name == DefaultRoleBundles.UserRoleName)
                   .Select(role => (Guid?)role.Id)
                   .SingleOrDefaultAsync()
               ?? throw new InvalidOperationException("No role without user and role management is available for new users.");
    }
}
