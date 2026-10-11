using System.Security.Cryptography;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TaskFlow.Api.Configurations;
using TaskFlow.Application.Identity;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities.Users;
using TaskFlow.Infrastructure;
using TaskFlow.Infrastructure.SignalR;

namespace TaskFlow.IntegrationTests.Infrastructure;

/// <summary>
/// Runs the real API (Program.cs, the JWT validation, Wolverine, SignalR, the seeder) against the test database.
/// Tests sign in with tokens from the API's own token issuer, so every request goes through the production validation.
/// </summary>
public sealed class TaskFlowApiFactory(string connectionString) : WebApplicationFactory<Program>
{
    public const string AdministratorEmail = "admin@taskflow.test";

    /// <summary>A signing key for this test run only.</summary>
    public string SigningKey { get; } = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));

    public NotificationStore Notifications { get; } = new();

    /// <summary>A token as the login endpoint issues it, without the password check.</summary>
    public string TokenFor(UserEntity user)
    {
        return Services.GetRequiredService<IAccessTokenIssuer>().Issue(user).AccessToken;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(WolverineConfiguration.TestingEnvironment);
        builder.UseSetting($"ConnectionStrings:{TaskFlowInfrastructureDependencyInjection.ConnectionStringName}", connectionString);
        builder.UseSetting("Jwt:SigningKey", SigningKey);
        builder.UseSetting("Seed:AdministratorEmail", AdministratorEmail);
        builder.UseSetting("Seed:AdministratorDisplayName", "Test Administrator");
        builder.UseSetting("RateLimiting:SignInPermitLimit", "100000");
        builder.UseSetting("Logging:LogLevel:Default", "Warning");

        // Without Seed:AdministratorPassword the seeder warns before every test; the password tests set one themselves.
        builder.UseSetting("Logging:LogLevel:TaskFlow.Infrastructure.Identity.TaskFlowSeeder", "Error");

        builder.ConfigureTestServices(services =>
        {
            services.AddSingleton(Notifications);
            services.RemoveAll<ISendNotificationCommandHandler>();
            services.AddScoped<SendNotificationCommandHandler>();
            services.AddScoped<ISendNotificationCommandHandler, RecordingNotificationSender>();
        });
    }
}
