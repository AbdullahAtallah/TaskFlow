using Bogus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskFlow.Application.Identity;
using TaskFlow.Domain.Constants;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Entities.Authorization;
using TaskFlow.Domain.Entities.Projects;
using TaskFlow.Domain.Entities.Tasks;
using TaskFlow.Domain.Entities.Teams;
using TaskFlow.Domain.Entities.Users;
using TaskFlow.Domain.Enums;
using TaskFlow.Infrastructure.Persistence;
using TaskStatus = TaskFlow.Domain.Enums.TaskStatus;

namespace TaskFlow.IntegrationTests.Infrastructure;

/// <summary>Arranges test data straight in the database, the way Spinnr's test seeders do.</summary>
public sealed class TestDataBuilder(TaskFlowFixture fixture)
{
    private readonly Faker _faker = new();

    public async Task<UserEntity> AdminAsync()
    {
        await using var dbContext = fixture.CreateDbContext();
        return await dbContext.Users.AsNoTracking().SingleAsync(user => user.UserName == TaskFlowApiFactory.AdministratorEmail);
    }

    public Task<UserEntity> UserAsync(string roleName = DefaultRoleBundles.UserRoleName, bool isActive = true, string? displayName = null)
    {
        return UserInRolesAsync([roleName], isActive, displayName);
    }

    /// <summary>A user with the User role who can sign in with this password; other users only get tokens from the tests.</summary>
    public Task<UserEntity> UserWithPasswordAsync(string password, bool isActive = true, string? email = null)
    {
        return UserInRolesAsync([DefaultRoleBundles.UserRoleName], isActive, email: email, password: password);
    }

    public async Task<UserEntity> UserInRolesAsync(
        IReadOnlyCollection<string> roleNames,
        bool isActive = true,
        string? displayName = null,
        string? email = null,
        string? password = null)
    {
        await using var dbContext = fixture.CreateDbContext();
        var roleIds = await dbContext.Roles.Where(role => roleNames.Contains(role.Name)).Select(role => role.Id).ToListAsync();

        var user = new UserEntity
        {
            Id = Guid.NewGuid(),
            UserName = email ?? NewEmail(),
            DisplayName = displayName ?? _faker.Name.FullName(),
            IsActive = isActive,
            PasswordHash = password is null ? null : fixture.Factory.Services.GetRequiredService<IPasswordHasher>().Hash(password)
        };

        foreach (var roleId in roleIds)
        {
            user.UserRoles.Add(new UserRoleEntity { UserId = user.Id, RoleId = roleId });
        }

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        return user;
    }

    /// <summary>A user whose only role grants exactly these packages (plus General) and claims.</summary>
    public async Task<UserEntity> UserWithGrantsAsync(IReadOnlyCollection<string> packages, IReadOnlyCollection<string> claims)
    {
        var role = await RoleAsync(packages, claims);
        return await UserInRolesAsync([role.Name]);
    }

    public async Task<RoleEntity> RoleAsync(IReadOnlyCollection<string> packages, IReadOnlyCollection<string> claims, string? name = null, string? description = null)
    {
        await using var dbContext = fixture.CreateDbContext();

        var role = new RoleEntity
        {
            Id = Guid.NewGuid(),
            Name = name ?? $"Role {Guid.NewGuid():N}",
            Description = description
        };

        foreach (var packageId in packages.Append(PackageTypes.General).Distinct())
        {
            role.RolePackages.Add(new RolePackageEntity { RoleId = role.Id, PackageId = packageId });
        }

        foreach (var claimId in claims)
        {
            role.RoleClaims.Add(new RoleClaimEntity { RoleId = role.Id, ClaimId = claimId });
        }

        dbContext.Roles.Add(role);
        await dbContext.SaveChangesAsync();

        return role;
    }

    public async Task<RoleEntity> RoleNamedAsync(string name)
    {
        await using var dbContext = fixture.CreateDbContext();
        return await dbContext.Roles.AsNoTracking().SingleAsync(role => role.Name == name);
    }

    public async Task<TeamEntity> TeamAsync(UserEntity[]? members = null, string? name = null, string? description = null, DateTime? created = null)
    {
        await using var dbContext = fixture.CreateDbContext();

        var team = new TeamEntity
        {
            Id = Guid.NewGuid(),
            Name = name ?? $"Team {Guid.NewGuid():N}",
            Description = description,
            DateTimeCreated = created ?? default
        };

        foreach (var member in members ?? [])
        {
            team.Members.Add(new TeamMemberEntity { TeamId = team.Id, UserId = member.Id });
        }

        dbContext.Teams.Add(team);
        await dbContext.SaveChangesAsync();

        return team;
    }

    public async Task AddMembersAsync(TeamEntity team, params UserEntity[] members)
    {
        await using var dbContext = fixture.CreateDbContext();
        dbContext.TeamMembers.AddRange(members.Select(member => new TeamMemberEntity { TeamId = team.Id, UserId = member.Id }));
        await dbContext.SaveChangesAsync();
    }

    public async Task<ProjectEntity> ProjectAsync(TeamEntity team, string? name = null, string? description = null, DateTime? created = null)
    {
        await using var dbContext = fixture.CreateDbContext();

        var project = new ProjectEntity
        {
            Id = Guid.NewGuid(),
            TeamId = team.Id,
            Name = name ?? _faker.Commerce.ProductName(),
            Description = description,
            DateTimeCreated = created ?? default
        };

        dbContext.Projects.Add(project);
        await dbContext.SaveChangesAsync();

        return project;
    }

    public async Task<TaskEntity> TaskAsync(
        ProjectEntity project,
        UserEntity creator,
        string? title = null,
        TaskStatus status = TaskStatus.ToDo,
        TaskPriority priority = TaskPriority.Medium,
        DateOnly? due = null,
        UserEntity? assignee = null,
        string? description = null,
        DateTime? created = null)
    {
        await using var dbContext = fixture.CreateDbContext();

        var task = new TaskEntity
        {
            Id = Guid.NewGuid(),
            ProjectId = project.Id,
            Title = title ?? _faker.Lorem.Sentence(3),
            Description = description,
            Status = status,
            Priority = priority,
            DueDate = due,
            AssigneeId = assignee?.Id,
            CreatedById = creator.Id,
            DateTimeCreated = created ?? default
        };

        dbContext.Tasks.Add(task);
        await dbContext.SaveChangesAsync();

        return task;
    }

    /// <summary>Loads an entity including soft-deleted rows, to check what is really stored.</summary>
    public async Task<T?> FindIncludingDeletedAsync<T>(Guid id) where T : BaseEntity
    {
        await using var dbContext = fixture.CreateDbContext();
        return await dbContext.Set<T>().IgnoreQueryFilters().AsNoTracking().SingleOrDefaultAsync(entity => entity.Id == id);
    }

    /// <summary>The current row version, as an edit form gets it with the record.</summary>
    public async Task<byte[]> RowVersionOfAsync<T>(Guid id) where T : BaseEntity
    {
        return (await FindIncludingDeletedAsync<T>(id))!.RowVersion;
    }

    /// <summary>Runs a read against a fresh context; global query filters apply unless the query ignores them.</summary>
    public async Task<TResult> QueryAsync<TResult>(Func<TaskFlowDbContext, Task<TResult>> query)
    {
        await using var dbContext = fixture.CreateDbContext();
        return await query(dbContext);
    }

    /// <summary>Changes the database behind the API's back, e.g. to switch a feature off.</summary>
    public async Task ExecuteAsync(Func<TaskFlowDbContext, Task> change)
    {
        await using var dbContext = fixture.CreateDbContext();
        await change(dbContext);
        await dbContext.SaveChangesAsync();
    }

    private string NewEmail()
    {
        return $"{_faker.Name.FirstName().ToLowerInvariant()}.{Guid.NewGuid():N}@taskflow.test";
    }
}
