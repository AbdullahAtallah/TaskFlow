using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using TaskFlow.Application.Identity;
using TaskFlow.Domain.Constants;
using TaskFlow.Infrastructure.Identity;
using TaskFlow.Infrastructure.Persistence;
using TaskFlow.IntegrationTests.Infrastructure;

namespace TaskFlow.IntegrationTests.Seeding;

public sealed class SeederTests(TaskFlowFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task Database_IsCreatedByTheMigrations_WithNothingPending()
    {
        await using var dbContext = Fixture.CreateDbContext();

        (await dbContext.Database.GetPendingMigrationsAsync()).Should().BeEmpty();
        (await dbContext.Database.GetAppliedMigrationsAsync()).Should().NotBeEmpty();
    }

    [Fact]
    public async Task Catalogue_MatchesTheConstants()
    {
        await using var dbContext = Fixture.CreateDbContext();

        (await dbContext.Packages.Select(package => package.Id).ToListAsync()).Should().BeEquivalentTo(PackageTypes.All);
        (await dbContext.Features.Select(feature => new { feature.Id, feature.PackageId, feature.IsEnabled }).ToListAsync())
            .Should().BeEquivalentTo(FeatureTypes.All.Select(feature => new { feature.Id, feature.PackageId, IsEnabled = true }));
        (await dbContext.Claims.Select(claim => new { claim.Id, claim.PackageId, claim.FeatureId }).ToListAsync())
            .Should().BeEquivalentTo(ClaimTypes.All.Select(claim => new { claim.Id, claim.PackageId, claim.FeatureId }));
    }

    [Fact]
    public async Task DefaultRoles_HaveTheirBundleGrants_AndUserIsTheOnlyRoleForNewUsers()
    {
        await using var dbContext = Fixture.CreateDbContext();

        foreach (var bundle in DefaultRoleBundles.All)
        {
            var role = await dbContext.Roles.Include(role => role.RolePackages).Include(role => role.RoleClaims).SingleAsync(role => role.Name == bundle.RoleName);
            role.RolePackages.Select(rolePackage => rolePackage.PackageId).Should().BeEquivalentTo(bundle.PackageIds);
            role.RoleClaims.Select(roleClaim => roleClaim.ClaimId).Should().BeEquivalentTo(bundle.ClaimIds);
            role.IsDefault.Should().Be(bundle.IsDefaultForNewUsers);
        }
    }

    [Fact]
    public async Task FirstAdministrator_IsCreatedWithTheAdminRole()
    {
        await using var dbContext = Fixture.CreateDbContext();

        var administrator = await dbContext.Users.Include(user => user.UserRoles).ThenInclude(userRole => userRole.Role)
            .SingleAsync(user => user.UserName == TaskFlowApiFactory.AdministratorEmail);

        administrator.DisplayName.Should().Be("Test Administrator");
        administrator.IsActive.Should().BeTrue();
        administrator.UserRoles.Select(userRole => userRole.Role!.Name).Should().Equal(DefaultRoleBundles.AdminRoleName);
    }

    [Fact]
    public async Task Seeding_AgainChangesNothing()
    {
        var before = await CountRowsAsync();

        await Factory.Services.SeedTaskFlowAsync();

        (await CountRowsAsync()).Should().Equal(before);
    }

    [Fact]
    public async Task Migrations_SeedExactlyTheCatalogueAndTheDefaultRolesOfTheSeedFile()
    {
        var connectionString = new SqlConnectionStringBuilder(Fixture.ConnectionString) { InitialCatalog = "TaskFlowSeedCheck" }.ConnectionString;
        await using var dbContext = new TaskFlowDbContext(new DbContextOptionsBuilder<TaskFlowDbContext>().UseSqlServer(connectionString).Options);

        try
        {
            await dbContext.Database.MigrateAsync();

            (await dbContext.Packages.Select(package => package.Id).ToListAsync())
                .Should().BeEquivalentTo(Seeds.GetPackageSeed().Select(package => package.Id));
            (await dbContext.Claims.Select(claim => new { claim.Id, claim.PackageId, claim.FeatureId }).ToListAsync())
                .Should().BeEquivalentTo(Seeds.GetClaimSeed().Select(claim => new { claim.Id, claim.PackageId, claim.FeatureId }));
            (await dbContext.Roles.Select(role => new { role.Id, role.Name, role.Description, role.IsDefault }).ToListAsync())
                .Should().BeEquivalentTo(Seeds.GetRoleSeed().Select(role => new { role.Id, role.Name, role.Description, role.IsDefault }));
            (await dbContext.RolePackages.Select(rolePackage => new { rolePackage.RoleId, rolePackage.PackageId }).ToListAsync())
                .Should().BeEquivalentTo(Seeds.GetRolePackageSeed().Select(rolePackage => new { rolePackage.RoleId, rolePackage.PackageId }));
            (await dbContext.RoleClaims.Select(roleClaim => new { roleClaim.RoleId, roleClaim.ClaimId }).ToListAsync())
                .Should().BeEquivalentTo(Seeds.GetRoleClaimSeed().Select(roleClaim => new { roleClaim.RoleId, roleClaim.ClaimId }));
        }
        finally
        {
            await dbContext.Database.EnsureDeletedAsync();
        }
    }

    [Fact]
    public async Task Model_HasNoChangesThatAreMissingFromTheMigrations()
    {
        await using var dbContext = Fixture.CreateDbContext();

        dbContext.Database.HasPendingModelChanges().Should().BeFalse(
            "a changed constant or seed needs a migration: dotnet ef migrations add <Name> --project TaskFlow.Infrastructure " +
            "--startup-project TaskFlow.Api --output-dir Persistence/Migrations");
    }

    [Fact]
    public async Task DefaultRoles_HaveTheFixedIdsOfTheSeedFile()
    {
        (await Data.RoleNamedAsync(DefaultRoleBundles.AdminRoleName)).Id.Should().Be(Seeds.GetAdminRoleSeed().Id);
        (await Data.RoleNamedAsync(DefaultRoleBundles.UserRoleName)).Id.Should().Be(Seeds.GetUserRoleSeed().Id);
        (await Data.RoleNamedAsync(DefaultRoleBundles.ReadOnlyUserRoleName)).Id.Should().Be(Seeds.GetReadOnlyUserRoleSeed().Id);
    }

    [Fact]
    public async Task Seeding_KeepsWhatAnAdministratorChangedInTheDefaultRoles()
    {
        var userRole = await Data.RoleNamedAsync(DefaultRoleBundles.UserRoleName);
        await Data.ExecuteAsync(dbContext => dbContext.RoleClaims
            .Where(roleClaim => roleClaim.RoleId == userRole.Id && roleClaim.ClaimId == ClaimTypes.TasksAssign)
            .ExecuteDeleteAsync());

        await Factory.Services.SeedTaskFlowAsync();

        (await Data.QueryAsync(dbContext => dbContext.RoleClaims
                .AnyAsync(roleClaim => roleClaim.RoleId == userRole.Id && roleClaim.ClaimId == ClaimTypes.TasksAssign)))
            .Should().BeFalse("only a migration changes the seeded roles; a start of the API does not");
    }

    [Fact]
    public async Task Seeding_RestoresExactlyOneRoleForNewUsers()
    {
        await Data.ExecuteAsync(async dbContext =>
        {
            foreach (var role in await dbContext.Roles.ToListAsync())
            {
                role.IsDefault = false;
            }
        });

        await Factory.Services.SeedTaskFlowAsync();

        (await Data.QueryAsync(dbContext => dbContext.Roles.Where(role => role.IsDefault).Select(role => role.Name).ToListAsync()))
            .Should().Equal(DefaultRoleBundles.UserRoleName);
    }

    [Fact]
    public async Task Seeding_SetsTheConfiguredAdministratorPassword_OnlyWhileTheAdministratorHasNone()
    {
        await SeedWithAdministratorPasswordAsync("First-password-1");
        var hashAfterFirstSeed = (await Data.AdminAsync()).PasswordHash;

        await SeedWithAdministratorPasswordAsync("Second-password-2");

        hashAfterFirstSeed.Should().NotBeNull();
        (await Data.AdminAsync()).PasswordHash.Should().Be(hashAfterFirstSeed, "a password that is already set is never overwritten");
        var login = await Factory.CreateClient().PostAsJsonAsync("/api/auth/login",
            new { email = AdminEmail, password = "First-password-1" }, TestJson.Options);
        await login.ShouldHaveStatusAsync(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Seeding_NeverPromotesAnAccountThatSomebodyRegisteredWithTheAdministratorAddress()
    {
        var administrator = await Data.AdminAsync();
        await Data.ExecuteAsync(dbContext => dbContext.Users
            .Where(user => user.Id == administrator.Id)
            .ExecuteUpdateAsync(setters => setters.SetProperty(user => user.DateTimeDeleted, DateTime.UtcNow)));
        var registration = await Factory.CreateClient().PostAsJsonAsync("/api/auth/register",
            new { email = AdminEmail, displayName = "Not the administrator", password = "Takeover-1" }, TestJson.Options);
        await registration.ShouldHaveStatusAsync(HttpStatusCode.Created);

        await Factory.Services.SeedTaskFlowAsync();

        var registered = await Data.QueryAsync(dbContext => dbContext.Users
            .Where(user => user.UserName == AdminEmail)
            .Select(user => user.UserRoles.Select(userRole => userRole.Role!.Name).ToList())
            .SingleAsync());
        registered.Should().Equal(DefaultRoleBundles.UserRoleName);
    }

    [Fact]
    public async Task Seeding_RestoresTheAdminRoleOfItsOwnAccount_WhileThatHasNoPassword()
    {
        var administrator = await Data.AdminAsync();
        await Data.ExecuteAsync(dbContext => dbContext.UserRoles.Where(userRole => userRole.UserId == administrator.Id).ExecuteDeleteAsync());

        await Factory.Services.SeedTaskFlowAsync();

        (await Data.QueryAsync(dbContext => dbContext.UserRoles.CountAsync(userRole => userRole.UserId == administrator.Id))).Should().Be(1);
    }

    [Fact]
    public async Task Seeding_RefusesAWeakAdministratorPassword()
    {
        var seed = () => SeedWithAdministratorPasswordAsync("weak");

        await seed.Should().ThrowAsync<InvalidOperationException>();
        (await Data.AdminAsync()).PasswordHash.Should().BeNull();
    }

    private async Task SeedWithAdministratorPasswordAsync(string password)
    {
        await using var dbContext = Fixture.CreateDbContext();
        var seedSettings = Options.Create(new SeedSettings
        {
            AdministratorEmail = AdminEmail,
            AdministratorDisplayName = "Test Administrator",
            AdministratorPassword = password
        });

        var seeder = new TaskFlowSeeder(
            dbContext, seedSettings, Factory.Services.GetRequiredService<IPasswordHasher>(), NullLogger<TaskFlowSeeder>.Instance);
        await seeder.SeedAsync();
    }

    private Task<int[]> CountRowsAsync()
    {
        return Data.QueryAsync(async dbContext => new[]
        {
            await dbContext.Packages.CountAsync(),
            await dbContext.Features.CountAsync(),
            await dbContext.Claims.CountAsync(),
            await dbContext.Roles.CountAsync(),
            await dbContext.RolePackages.CountAsync(),
            await dbContext.RoleClaims.CountAsync(),
            await dbContext.Users.CountAsync(),
            await dbContext.UserRoles.CountAsync()
        });
    }
}
