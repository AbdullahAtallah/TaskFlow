using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaskFlow.Application.Common.Extensions;
using TaskFlow.Application.Identity;
using TaskFlow.Domain.Constants;
using TaskFlow.Domain.Entities.Users;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Infrastructure.Identity;

/// <summary>
/// Runs every time the API starts. The migrations seed the catalogue and the default roles (see <see cref="Seeds"/>); this
/// checks that the database is up to date, keeps exactly one role the default for new users, and creates the first
/// administrator, whose e-mail address and password come from the configuration.
/// </summary>
public sealed class TaskFlowSeeder(
    TaskFlowDbContext dbContext,
    IOptions<SeedSettings> seedSettings,
    IPasswordHasher passwordHasher,
    ILogger<TaskFlowSeeder> logger)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await EnsureDatabaseIsUpToDateAsync(cancellationToken);

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        await EnsureExactlyOneDefaultRoleAsync(cancellationToken);
        await SeedAdministratorAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);
    }

    /// <summary>
    /// The catalogue and the default roles live in the migrations, so a missing migration, or a change of the seed data
    /// that no migration contains yet, would leave the database behind the code.
    /// </summary>
    private async Task EnsureDatabaseIsUpToDateAsync(CancellationToken cancellationToken)
    {
        if (dbContext.Database.HasPendingModelChanges())
        {
            throw new InvalidOperationException(
                "The model or its seed data has changes that no migration contains yet. Run 'dotnet ef migrations add <Name> " +
                "--project TaskFlow.Infrastructure --startup-project TaskFlow.Api --output-dir Persistence/Migrations' first.");
        }

        var pendingMigrations = (await dbContext.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();

        if (pendingMigrations.Count > 0)
        {
            throw new InvalidOperationException(
                $"The TaskFlow database is missing {pendingMigrations.Count} migration(s). Run " +
                "'dotnet ef database update --project TaskFlow.Infrastructure --startup-project TaskFlow.Api' first.");
        }
    }

    /// <summary>
    /// Repairs the rare case where two administrators changing roles at the same moment left two default roles, or none.
    /// It makes the same choice registration makes meanwhile: the first default role by name that does not grant
    /// Management, or else the built-in User role.
    /// </summary>
    private async Task EnsureExactlyOneDefaultRoleAsync(CancellationToken cancellationToken)
    {
        var roles = await dbContext.Roles.Include(role => role.RolePackages).ToListAsync(cancellationToken);
        var defaultRoles = roles.Where(role => role.IsDefault).ToList();
        if (defaultRoles.Count == 1)
        {
            return;
        }

        var bundleRoleName = DefaultRoleBundles.All.Single(bundle => bundle.IsDefaultForNewUsers).RoleName;
        var roleForNewUsers = defaultRoles
                                  .Where(role => role.RolePackages.All(rolePackage => rolePackage.PackageId != PackageTypes.Management))
                                  .MinBy(role => role.Name, StringComparer.OrdinalIgnoreCase)
                              ?? roles.Single(role => role.Name == bundleRoleName);

        foreach (var role in roles)
        {
            role.IsDefault = role == roleForNewUsers;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogWarning("Made {RoleName} the only role for new users", roleForNewUsers.Name);
    }

    /// <summary>
    /// Creates the first administrator with the Admin role, and keeps managing that account only until it has a password:
    /// then the account belongs to a person, and later starts leave it alone.
    /// </summary>
    private async Task SeedAdministratorAsync(CancellationToken cancellationToken)
    {
        var administratorEmail = seedSettings.Value.AdministratorEmail;
        if (string.IsNullOrWhiteSpace(administratorEmail))
        {
            logger.LogInformation("No first administrator is configured (Seed:AdministratorEmail)");
            return;
        }

        var userName = administratorEmail.NormalizeUserName();
        if (!userName.Contains('@'))
        {
            logger.LogWarning("Seed:AdministratorEmail is not an e-mail address, so no administrator was seeded");
            return;
        }

        var adminRoleId = Seeds.GetAdminRoleSeed().Id;
        var user = await dbContext.Users
            .Include(existing => existing.UserRoles)
            .SingleOrDefaultAsync(existing => existing.UserName == userName, cancellationToken);

        if (user is null)
        {
            user = new UserEntity
            {
                Id = Guid.NewGuid(),
                UserName = userName,
                DisplayName = seedSettings.Value.AdministratorDisplayName.TrimToNull() ?? userName.Split('@')[0],
                IsActive = true
            };
            dbContext.Users.Add(user);
        }
        else if (user.PasswordHash is not null)
        {
            // Bootstrap only: an account with a password of its own is no longer the seeder's. It may even be an account
            // somebody registered with this address after the original administrator was deleted, so it is never promoted.
            if (user.UserRoles.All(userRole => userRole.RoleId != adminRoleId))
            {
                logger.LogWarning("Seed:AdministratorEmail belongs to an account with its own password and without the Admin role; " +
                                  "the seeder does not promote it");
            }

            return;
        }

        if (user.UserRoles.All(userRole => userRole.RoleId != adminRoleId))
        {
            user.UserRoles.Add(new UserRoleEntity { UserId = user.Id, RoleId = adminRoleId });
            logger.LogInformation("Gave the Admin role to the configured first administrator {UserId}", user.Id);
        }

        SetInitialAdministratorPassword(user);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private void SetInitialAdministratorPassword(UserEntity administrator)
    {
        var password = seedSettings.Value.AdministratorPassword;
        if (string.IsNullOrEmpty(password))
        {
            logger.LogWarning("The first administrator has no password yet; set Seed:AdministratorPassword to sign in");
            return;
        }

        if (!PasswordPolicy.IsSatisfiedBy(password))
        {
            throw new InvalidOperationException(
                "Seed:AdministratorPassword must be 8 to 128 characters long and contain at least one letter and one digit.");
        }

        administrator.PasswordHash = passwordHasher.Hash(password);
        logger.LogInformation("Set the configured password of the first administrator {UserId}", administrator.Id);
    }
}
