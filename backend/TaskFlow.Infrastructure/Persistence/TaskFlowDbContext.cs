using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Exceptions;
using TaskFlow.Infrastructure.Persistence.Extensions;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Entities.Authorization;
using TaskFlow.Domain.Entities.Projects;
using TaskFlow.Domain.Entities.Tasks;
using TaskFlow.Domain.Entities.Teams;
using TaskFlow.Domain.Entities.Users;

namespace TaskFlow.Infrastructure.Persistence;

public class TaskFlowDbContext : DbContext, IUnitOfWork
{
    public TaskFlowDbContext(DbContextOptions<TaskFlowDbContext> options) : base(options)
    {
        // Remove() of an entity becomes a soft delete in SaveChanges, so EF must not cascade it to related rows before
        // then: it would try to clear required foreign keys, such as the creator of a task.
        ChangeTracker.CascadeDeleteTiming = CascadeTiming.OnSaveChanges;
    }

    public DbSet<UserEntity> Users => Set<UserEntity>();
    public DbSet<UserRoleEntity> UserRoles => Set<UserRoleEntity>();
    public DbSet<PackageEntity> Packages => Set<PackageEntity>();
    public DbSet<FeatureEntity> Features => Set<FeatureEntity>();
    public DbSet<ClaimEntity> Claims => Set<ClaimEntity>();
    public DbSet<RoleEntity> Roles => Set<RoleEntity>();
    public DbSet<RolePackageEntity> RolePackages => Set<RolePackageEntity>();
    public DbSet<RoleClaimEntity> RoleClaims => Set<RoleClaimEntity>();
    public DbSet<TeamEntity> Teams => Set<TeamEntity>();
    public DbSet<TeamMemberEntity> TeamMembers => Set<TeamMemberEntity>();
    public DbSet<ProjectEntity> Projects => Set<ProjectEntity>();
    public DbSet<TaskEntity> Tasks => Set<TaskEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TaskFlowDbContext).Assembly);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplySoftDeleteAndModificationDates();

        try
        {
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }
        catch (DbUpdateException exception) when (ToBusinessRuleFailure(exception) is { } failure)
        {
            throw failure;
        }
    }

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        ApplySoftDeleteAndModificationDates();

        try
        {
            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
        catch (DbUpdateException exception) when (ToBusinessRuleFailure(exception) is { } failure)
        {
            throw failure;
        }
    }

    /// <summary>
    /// A row saved by someone else since it was loaded (row version), or a name taken by a parallel request (unique
    /// index), becomes a message the user can act on.
    /// </summary>
    private static TaskFlowException? ToBusinessRuleFailure(DbUpdateException exception)
    {
        return exception switch
        {
            DbUpdateConcurrencyException => new ConcurrencyConflictException(exception),
            _ when exception.IsUniqueConstraintViolation() => new DuplicateValueException(exception),
            _ => null
        };
    }

    /// <summary>
    /// Turns the delete of an entity into DateTimeDeleted = UtcNow; junction rows are not entities and are really deleted.
    /// A soft delete writes only that column, so it never undoes a change another command saved in the meantime.
    /// </summary>
    private void ApplySoftDeleteAndModificationDates()
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>().ToList())
        {
            switch (entry.State)
            {
                case EntityState.Deleted:
                    entry.State = EntityState.Unchanged;
                    entry.Property(entity => entity.DateTimeDeleted).CurrentValue = now;
                    break;
                case EntityState.Modified:
                    entry.Entity.DateTimeModified = now;
                    break;
            }
        }
    }
}
