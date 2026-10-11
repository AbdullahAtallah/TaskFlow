using Microsoft.AspNetCore.Authorization;
using TaskFlow.Api.Configurations;
using TaskFlow.Application;
using TaskFlow.Infrastructure;
using TaskFlow.Infrastructure.Identity;
using TaskFlow.Infrastructure.Middleware;
using TaskFlow.Infrastructure.SignalR;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

var connectionString = builder.Configuration.GetConnectionString(TaskFlowInfrastructureDependencyInjection.ConnectionStringName)
    ?? throw new InvalidOperationException(
        $"The connection string '{TaskFlowInfrastructureDependencyInjection.ConnectionStringName}' is missing. Set it with " +
        $"'dotnet user-secrets set \"ConnectionStrings:{TaskFlowInfrastructureDependencyInjection.ConnectionStringName}\" \"<connection string>\"'.");

builder.Services.ConfigureTaskFlowApplication();
builder.Services.ConfigureTaskFlowInfrastructure(connectionString);
builder.Services.AddAuthenticationConfiguration();
builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
builder.Services.AddCorsConfiguration();
builder.Services.AddRateLimitingConfiguration();
builder.Services.AddControllersConfiguration();
builder.Services.AddSignalRConfiguration();
builder.Services.AddSwaggerConfiguration();
builder.Services.AddProblemDetails();
builder.Host.ConfigureWolverine(connectionString, builder.Environment);

var app = builder.Build();

await app.Services.SeedTaskFlowAsync();

app.UseExceptionHandler();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseCors(CorsConfiguration.FrontEndPolicy);
app.UseRateLimiter();

// As in Spinnr, Swagger is served only where it is known to be wanted: locally and in the integration tests.
if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment(WolverineConfiguration.TestingEnvironment))
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // The API has no page of its own: opening its address in a browser shows Swagger instead of a 401.
    app.MapGet("/", () => Results.Redirect("/swagger")).AllowAnonymous().ExcludeFromDescription();
}

app.UseAuthentication();
app.UseMiddleware<NotificationHubAccessMiddleware>();
app.UseAuthorization();

app.MapControllers();
// A notification connection lives no longer than the access token it was opened with.
app.MapHub<NotificationHub>(NotificationHubPaths.Hub, options => options.CloseOnAuthenticationExpiration = true);

await app.RunAsync();

public partial class Program;
