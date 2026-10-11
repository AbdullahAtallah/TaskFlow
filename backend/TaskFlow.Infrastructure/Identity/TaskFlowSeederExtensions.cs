using Microsoft.Extensions.DependencyInjection;

namespace TaskFlow.Infrastructure.Identity;

public static class TaskFlowSeederExtensions
{
    public static async Task SeedTaskFlowAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();

        await scope.ServiceProvider.GetRequiredService<TaskFlowSeeder>().SeedAsync(cancellationToken);
    }
}
