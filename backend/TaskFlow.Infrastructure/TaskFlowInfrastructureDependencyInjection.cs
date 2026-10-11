using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using TaskFlow.Application.Identity;
using TaskFlow.Application.Interfaces;
using TaskFlow.Application.Interfaces.DataReaders;
using TaskFlow.Infrastructure.Identity;
using TaskFlow.Infrastructure.Persistence;
using TaskFlow.Infrastructure.Persistence.DataReaders;
using TaskFlow.Infrastructure.Services;
using TaskFlow.Infrastructure.SignalR;

namespace TaskFlow.Infrastructure;

public static class TaskFlowInfrastructureDependencyInjection
{
    /// <summary>The one connection string of TaskFlow; set it with User Secrets locally.</summary>
    public const string ConnectionStringName = "TaskFlowDb";

    public static IServiceCollection ConfigureTaskFlowInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<TaskFlowDbContext>(options => options
            .UseSqlServer(connectionString)
            // Intended: a soft delete removes or soft-deletes the dependent rows in the same command,
            // so a filtered principal never leaves a visible dependent behind.
            .ConfigureWarnings(warnings => warnings.Ignore(CoreEventId.PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning)));
        services.AddScoped<IUnitOfWork>(serviceProvider => serviceProvider.GetRequiredService<TaskFlowDbContext>());
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        services.AddScoped<ITeamDataReader, TeamDataReader>();
        services.AddScoped<IProjectDataReader, ProjectDataReader>();
        services.AddScoped<ITaskDataReader, TaskDataReader>();
        services.AddScoped<IUserDataReader, UserDataReader>();
        services.AddScoped<IRoleDataReader, RoleDataReader>();

        services.AddOptions<JwtSettings>()
            .BindConfiguration(JwtSettings.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddOptions<SeedSettings>().BindConfiguration(SeedSettings.SectionName);
        services.AddOptions<RateLimitingSettings>().BindConfiguration(RateLimitingSettings.SectionName);
        services.AddOptions<CalendarSettings>()
            .BindConfiguration(CalendarSettings.SectionName)
            .Validate(settings => TimeZoneInfo.TryFindSystemTimeZoneById(settings.TimeZone, out _),
                $"{CalendarSettings.SectionName}:TimeZone is not a known time zone.")
            .ValidateOnStart();

        services.AddHttpContextAccessor();
        services.AddMemoryCache();
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<BusinessCalendar>();
        services.AddSingleton<IPasswordHasher, IdentityPasswordHasher>();
        services.AddSingleton<IAccessTokenIssuer, AccessTokenIssuer>();
        services.AddSingleton<ISignInAttemptLimiter, SignInAttemptLimiter>();
        services.AddScoped<CurrentUserDataReader>();
        services.AddScoped<ICurrentAuthContextResolver, CurrentAuthContextResolver>();
        services.AddScoped<ICurrentUserRetriever, CurrentUserRetriever>();
        services.AddScoped<TaskFlowSeeder>();

        services.AddSingleton<IUserIdProvider, TokenUserIdProvider>();
        services.AddScoped<ISendNotificationCommandHandler, SendNotificationCommandHandler>();

        return services;
    }
}
