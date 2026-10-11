using System.Net;
using System.Text.Json;
using AwesomeAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using TaskFlow.IntegrationTests.Infrastructure;

namespace TaskFlow.IntegrationTests.Api;

public sealed class SwaggerTests(TaskFlowFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task SwaggerDocument_DescribesEveryEndpoint_WithoutTheServerSideCommandFields()
    {
        var response = await Factory.CreateClient().GetAsync("/swagger/v1/swagger.json");

        await response.ShouldHaveStatusAsync(HttpStatusCode.OK);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var paths = document.RootElement.GetProperty("paths").EnumerateObject().Select(path => path.Name).ToList();
        paths.Should().Contain([
            "/api/auth/register", "/api/auth/login", "/api/auth/logout",
            "/api/user/current", "/api/user/select-list", "/api/user", "/api/user/{id}", "/api/user/{id}/password", "/api/user/{id}/permissions",
            "/api/role", "/api/role/{id}", "/api/role/{id}/user",
            "/api/team", "/api/team/{id}", "/api/team/{id}/member", "/api/team/{id}/member/{userId}",
            "/api/project", "/api/project/{id}", "/api/project/counts-per-team",
            "/api/task", "/api/task/{id}", "/api/task/{id}/status", "/api/task/{id}/assignee",
            "/api/task/my-tasks", "/api/task/status-counts", "/api/task/board"
        ]);

        var createTeam = document.RootElement.GetProperty("components").GetProperty("schemas").GetProperty("CreateTeamCommand").GetProperty("properties");
        createTeam.TryGetProperty("name", out _).Should().BeTrue();
        createTeam.TryGetProperty("userCommandContext", out _).Should().BeFalse();
        createTeam.TryGetProperty("correlationId", out _).Should().BeFalse();
        paths.Should().NotContain("/", "the redirect to Swagger is no part of the API");
    }

    [Fact]
    public async Task RootAddress_RedirectsToSwagger_WithoutAToken()
    {
        var client = Factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await client.GetAsync("/");

        await response.ShouldHaveStatusAsync(HttpStatusCode.Redirect);
        response.Headers.Location!.ToString().Should().Be("/swagger");
    }
}
