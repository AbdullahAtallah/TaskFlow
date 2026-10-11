using JasperFx;
using JasperFx.Core;
using Microsoft.Data.SqlClient;
using TaskFlow.Application.Commands.Teams.CreateTeam;
using Wolverine;
using Wolverine.ErrorHandling;
using Wolverine.SqlServer;

namespace TaskFlow.Api.Configurations;

public static class WolverineConfiguration
{
    public const string TestingEnvironment = "Testing";

    private const string MessageStorageSchema = "wolverine";

    public static IHostBuilder ConfigureWolverine(this IHostBuilder hostBuilder, string connectionString, IHostEnvironment environment)
    {
        hostBuilder.UseWolverine(options =>
        {
            // The integration tests run the same handlers, with an in-memory inbox.
            if (!environment.IsEnvironment(TestingEnvironment))
            {
                // Durable inbox in the TaskFlow database, in its own schema; Wolverine manages these tables itself.
                options.PersistMessagesWithSqlServer(connectionString, MessageStorageSchema);
                options.AutoBuildMessageStorageOnStartup = AutoCreate.CreateOrUpdate;
            }

            // Every command handler of the Application assembly, found by naming convention.
            options.Discovery.IncludeAssembly(typeof(CreateTeamCommandHandler).Assembly);

            options.Policies.UseDurableLocalQueues();

            options.Policies
                .OnException<SqlException>()
                .Or<TimeoutException>()
                .RetryWithCooldown(50.Milliseconds(), 100.Milliseconds(), 250.Milliseconds());

            options.Policies
                .OnException<Exception>()
                .MoveToErrorQueue();
        }, ExtensionDiscovery.ManualOnly);

        return hostBuilder;
    }
}
