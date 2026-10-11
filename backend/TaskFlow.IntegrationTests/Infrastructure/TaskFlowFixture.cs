using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Respawn;
using Respawn.Graph;
using Testcontainers.MsSql;
using TaskFlow.Infrastructure.Identity;
using TaskFlow.Infrastructure.Persistence;
using TaskFlow.Infrastructure.Services;

namespace TaskFlow.IntegrationTests.Infrastructure;

/// <summary>
/// One SQL Server 2022 container for the whole test run. The schema is created with the EF Core migrations, and
/// before every test Respawn empties the tables and the seeder restores the catalogue, default roles and admin.
/// </summary>
public sealed class TaskFlowFixture : IAsyncLifetime
{
    private const string DatabaseName = "TaskFlowTests";

    private readonly MsSqlContainer _sqlContainer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();
    private Respawner _respawner = null!;

    public string ConnectionString { get; private set; } = string.Empty;

    public TaskFlowApiFactory Factory { get; private set; } = null!;

    /// <summary>Today as the API sees it: in the configured time zone of the team.</summary>
    public DateOnly Today => Factory.Services.GetRequiredService<BusinessCalendar>().Today;

    public async Task InitializeAsync()
    {
        await _sqlContainer.StartAsync();

        ConnectionString = new SqlConnectionStringBuilder(_sqlContainer.GetConnectionString())
        {
            InitialCatalog = DatabaseName
        }.ConnectionString;

        await using (var dbContext = CreateDbContext())
        {
            await dbContext.Database.MigrateAsync();
        }

        Factory = new TaskFlowApiFactory(ConnectionString);
        _ = Factory.Server;

        await using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();
        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.SqlServer,
            TablesToIgnore = [new Table("__EFMigrationsHistory")]
        });
    }

    public async Task ResetAsync()
    {
        await using (var connection = new SqlConnection(ConnectionString))
        {
            await connection.OpenAsync();
            await _respawner.ResetAsync(connection);
        }

        await RestoreSeedDataAsync();
        await Factory.Services.SeedTaskFlowAsync();

        // The failed sign-ins per address live in memory; every test starts without any.
        ((MemoryCache)Factory.Services.GetRequiredService<IMemoryCache>()).Clear();
        Factory.Notifications.Clear();
    }

    /// <summary>Respawn empties the rows the seed migration inserted too; this puts them back the way the migration left them.</summary>
    private async Task RestoreSeedDataAsync()
    {
        await using var dbContext = CreateDbContext();

        dbContext.Packages.AddRange(Seeds.GetPackageSeed());
        dbContext.Features.AddRange(Seeds.GetFeatureSeed());
        dbContext.Claims.AddRange(Seeds.GetClaimSeed());
        dbContext.Roles.AddRange(Seeds.GetRoleSeed());
        dbContext.RolePackages.AddRange(Seeds.GetRolePackageSeed());
        dbContext.RoleClaims.AddRange(Seeds.GetRoleClaimSeed());

        await dbContext.SaveChangesAsync();
    }

    public TaskFlowDbContext CreateDbContext()
    {
        return new TaskFlowDbContext(new DbContextOptionsBuilder<TaskFlowDbContext>().UseSqlServer(ConnectionString).Options);
    }

    public async Task DisposeAsync()
    {
        await Factory.DisposeAsync();
        await _sqlContainer.DisposeAsync();
    }
}

[CollectionDefinition(Name)]
public sealed class TaskFlowCollection : ICollectionFixture<TaskFlowFixture>
{
    public const string Name = "TaskFlow";
}
