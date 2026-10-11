using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using TaskFlow.Domain.Constants;
using TaskFlow.Domain.Enums;
using TaskFlow.IntegrationTests.Infrastructure;

namespace TaskFlow.IntegrationTests.Authorization;

/// <summary>Every endpoint against the grants it needs: 401 without a token, 403 without its package or claim.</summary>
public sealed class EndpointAuthorizationTests(TaskFlowFixture fixture) : IntegrationTestBase(fixture)
{
    /// <summary>Direct: answered right away instead of with 202 Accepted and a notification.</summary>
    private sealed record Endpoint(
        HttpMethod Method,
        string Url,
        string? Package,
        string[] Claims,
        object? Body = null,
        string? Feature = null,
        bool Direct = false);

    private static readonly Guid AnyId = Guid.NewGuid();
    private static readonly byte[] AnyRowVersion = [0, 0, 0, 0, 0, 0, 0, 1];

    private static readonly Dictionary<string, Endpoint> Endpoints = new()
    {
        ["POST /api/auth/logout"] = new(HttpMethod.Post, "/api/auth/logout", null, [], Direct: true),

        ["GET /api/user/current"] = new(HttpMethod.Get, "/api/user/current", null, []),
        ["GET /api/user/select-list"] = new(HttpMethod.Get, "/api/user/select-list", null,
            [ClaimTypes.TeamsManage, ClaimTypes.TeamsMembersManage, ClaimTypes.TasksAssign]),

        ["GET /api/team"] = new(HttpMethod.Get, "/api/team", PackageTypes.Teams, []),
        ["GET /api/team/{id}"] = new(HttpMethod.Get, $"/api/team/{AnyId}", PackageTypes.Teams, []),
        ["GET /api/team/{id}/member"] = new(HttpMethod.Get, $"/api/team/{AnyId}/member", PackageTypes.Teams, []),
        ["POST /api/team"] = new(HttpMethod.Post, "/api/team", PackageTypes.Teams, [ClaimTypes.TeamsManage], new { name = "Team" }),
        ["PUT /api/team/{id}"] = new(HttpMethod.Put, $"/api/team/{AnyId}", PackageTypes.Teams, [ClaimTypes.TeamsManage],
            new { name = "Team", rowVersion = AnyRowVersion }),
        ["DELETE /api/team/{id}"] = new(HttpMethod.Delete, $"/api/team/{AnyId}", PackageTypes.Teams, [ClaimTypes.TeamsManage]),
        ["POST /api/team/{id}/member"] = new(HttpMethod.Post, $"/api/team/{AnyId}/member", PackageTypes.Teams,
            [ClaimTypes.TeamsMembersManage], new { userIds = new[] { AnyId } }),
        ["DELETE /api/team/{id}/member/{userId}"] = new(HttpMethod.Delete, $"/api/team/{AnyId}/member/{AnyId}", PackageTypes.Teams,
            [ClaimTypes.TeamsMembersManage]),

        ["GET /api/project"] = new(HttpMethod.Get, "/api/project", PackageTypes.Projects, []),
        ["GET /api/project/{id}"] = new(HttpMethod.Get, $"/api/project/{AnyId}", PackageTypes.Projects, []),
        ["GET /api/project/counts-per-team"] = new(HttpMethod.Get, "/api/project/counts-per-team", PackageTypes.Projects, []),
        ["POST /api/project"] = new(HttpMethod.Post, "/api/project", PackageTypes.Projects, [ClaimTypes.ProjectsCreate],
            new { teamId = AnyId, name = "Project" }),
        ["PUT /api/project/{id}"] = new(HttpMethod.Put, $"/api/project/{AnyId}", PackageTypes.Projects, [ClaimTypes.ProjectsManage],
            new { name = "Project", rowVersion = AnyRowVersion }),
        ["DELETE /api/project/{id}"] = new(HttpMethod.Delete, $"/api/project/{AnyId}", PackageTypes.Projects, [ClaimTypes.ProjectsManage]),

        ["GET /api/task"] = new(HttpMethod.Get, "/api/task", PackageTypes.Tasks, []),
        ["GET /api/task/{id}"] = new(HttpMethod.Get, $"/api/task/{AnyId}", PackageTypes.Tasks, []),
        ["GET /api/task/my-tasks"] = new(HttpMethod.Get, "/api/task/my-tasks", PackageTypes.Tasks, []),
        ["GET /api/task/status-counts"] = new(HttpMethod.Get, "/api/task/status-counts", PackageTypes.Tasks, []),
        ["POST /api/task"] = new(HttpMethod.Post, "/api/task", PackageTypes.Tasks, [ClaimTypes.TasksCreate],
            new { projectId = AnyId, title = "Task", status = "ToDo", priority = "Medium" }),
        ["PUT /api/task/{id}"] = new(HttpMethod.Put, $"/api/task/{AnyId}", PackageTypes.Tasks, [ClaimTypes.TasksManage],
            new { title = "Task", status = "ToDo", priority = "Medium", rowVersion = AnyRowVersion }),
        ["PUT /api/task/{id}/status"] = new(HttpMethod.Put, $"/api/task/{AnyId}/status", PackageTypes.Tasks, [ClaimTypes.TasksManage],
            new { status = "Done" }),
        ["PUT /api/task/{id}/assignee"] = new(HttpMethod.Put, $"/api/task/{AnyId}/assignee", PackageTypes.Tasks, [ClaimTypes.TasksAssign],
            new { assigneeId = (Guid?)null }),
        ["DELETE /api/task/{id}"] = new(HttpMethod.Delete, $"/api/task/{AnyId}", PackageTypes.Tasks, [ClaimTypes.TasksManage]),
        ["GET /api/task/board"] = new(HttpMethod.Get, $"/api/task/board?projectId={AnyId}", PackageTypes.Tasks, [ClaimTypes.TasksBoardView],
            Feature: FeatureTypes.TasksBoard),

        ["GET /api/user"] = new(HttpMethod.Get, "/api/user", PackageTypes.Management, []),
        ["GET /api/user/{id}"] = new(HttpMethod.Get, $"/api/user/{AnyId}", PackageTypes.Management, []),
        ["POST /api/user"] = new(HttpMethod.Post, "/api/user", PackageTypes.Management, [],
            new { userName = "new@taskflow.test", displayName = "New", password = "Initial-pass-1", roleIds = new[] { AnyId } }, Direct: true),
        ["PUT /api/user/{id}"] = new(HttpMethod.Put, $"/api/user/{AnyId}", PackageTypes.Management, [],
            new { displayName = "Name", isActive = true, rowVersion = AnyRowVersion }),
        ["PUT /api/user/{id}/password"] = new(HttpMethod.Put, $"/api/user/{AnyId}/password", PackageTypes.Management, [],
            new { password = "Brand-new-2" }, Direct: true),
        ["PUT /api/user/{id}/permissions"] = new(HttpMethod.Put, $"/api/user/{AnyId}/permissions", PackageTypes.Management, [],
            new { roleIds = new[] { AnyId } }),
        ["DELETE /api/user/{id}"] = new(HttpMethod.Delete, $"/api/user/{AnyId}", PackageTypes.Management, []),

        ["GET /api/role"] = new(HttpMethod.Get, "/api/role", PackageTypes.Management, []),
        ["GET /api/role/{id}"] = new(HttpMethod.Get, $"/api/role/{AnyId}", PackageTypes.Management, []),
        ["GET /api/role/{id}/user"] = new(HttpMethod.Get, $"/api/role/{AnyId}/user", PackageTypes.Management, []),
        ["POST /api/role"] = new(HttpMethod.Post, "/api/role", PackageTypes.Management, [],
            new { name = "Role", isDefault = false, packages = Array.Empty<string>(), claims = Array.Empty<string>() }),
        ["PUT /api/role/{id}"] = new(HttpMethod.Put, $"/api/role/{AnyId}", PackageTypes.Management, [],
            new { name = "Role", isDefault = false, packages = Array.Empty<string>(), claims = Array.Empty<string>(), rowVersion = AnyRowVersion }),
        ["DELETE /api/role/{id}"] = new(HttpMethod.Delete, $"/api/role/{AnyId}", PackageTypes.Management, [])
    };

    public static TheoryData<string> AllEndpoints => new(Endpoints.Keys);

    public static TheoryData<string> PackageEndpoints => new(Endpoints.Where(endpoint => endpoint.Value.Package is not null).Select(endpoint => endpoint.Key));

    public static TheoryData<string> ClaimEndpoints => new(Endpoints.Where(endpoint => endpoint.Value.Claims.Length > 0).Select(endpoint => endpoint.Key));

    [Theory]
    [MemberData(nameof(AllEndpoints))]
    public async Task Endpoint_WithoutAToken_Returns401(string name)
    {
        var endpoint = Endpoints[name];
        var request = new HttpRequestMessage(endpoint.Method, endpoint.Url)
        {
            Content = endpoint.Body is null ? null : System.Net.Http.Json.JsonContent.Create(endpoint.Body, options: TestJson.Options)
        };

        var response = await Factory.CreateClient().SendAsync(request);

        await response.ShouldHaveStatusAsync(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [MemberData(nameof(AllEndpoints))]
    public async Task Endpoint_WithAnInvalidToken_Returns401(string name)
    {
        var endpoint = Endpoints[name];
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "not-a-valid-token");

        var response = await client.SendAsync(new HttpRequestMessage(endpoint.Method, endpoint.Url));

        await response.ShouldHaveStatusAsync(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [MemberData(nameof(PackageEndpoints))]
    public async Task Endpoint_WithoutItsPackage_Returns403(string name)
    {
        var endpoint = Endpoints[name];
        var everyOtherPackage = PackageTypes.All.Where(package => package != endpoint.Package).ToList();
        var user = await Data.UserWithGrantsAsync(everyOtherPackage, ClaimTypes.All.Select(claim => claim.Id).Where(claimId => !claimId.StartsWith($"{endpoint.Package}.")).ToList());

        var response = await SendAsync(user.UserName, endpoint.Method, endpoint.Url, endpoint.Body);

        await response.ShouldHaveStatusAsync(HttpStatusCode.Forbidden);
        Factory.Notifications.All.Should().BeEmpty("a refused command is never published");
    }

    [Theory]
    [MemberData(nameof(ClaimEndpoints))]
    public async Task Endpoint_WithItsPackageButWithoutItsClaim_Returns403(string name)
    {
        var endpoint = Endpoints[name];
        var user = await Data.UserWithGrantsAsync(PackageTypes.All, ClaimTypes.All.Select(claim => claim.Id).Except(endpoint.Claims).ToList());

        var response = await SendAsync(user.UserName, endpoint.Method, endpoint.Url, endpoint.Body);

        await response.ShouldHaveStatusAsync(HttpStatusCode.Forbidden);
        Factory.Notifications.All.Should().BeEmpty("a refused command is never published");
    }

    [Theory]
    [MemberData(nameof(AllEndpoints))]
    public async Task Endpoint_WithExactlyItsGrants_IsAllowed(string name)
    {
        var endpoint = Endpoints[name];
        var packages = endpoint.Package is null ? Array.Empty<string>() : [endpoint.Package];
        var user = await Data.UserWithGrantsAsync(packages, endpoint.Claims.Take(1).ToList());

        var response = await SendAsync(user.UserName, endpoint.Method, endpoint.Url, endpoint.Body);

        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
        if (endpoint.Method != HttpMethod.Get && !endpoint.Direct)
        {
            await response.ShouldHaveStatusAsync(HttpStatusCode.Accepted);
            await Factory.Notifications.WaitForOutcomeAsync(Factory.Notifications.LastConfirmedCorrelationId(user.Id));
        }
    }

    [Theory]
    [InlineData("/api/auth/register")]
    [InlineData("/api/auth/login")]
    public async Task SignInEndpoints_NeedNoToken(string url)
    {
        var response = await Factory.CreateClient().PostAsJsonAsync(url, new { });

        await response.ShouldHaveStatusAsync(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Board_WithTheFeatureSwitchedOff_Returns403EvenWithTheClaim()
    {
        await Data.ExecuteAsync(async dbContext =>
        {
            var feature = await dbContext.Features.FindAsync(FeatureTypes.TasksBoard);
            feature!.IsEnabled = false;
        });

        var response = await SendAsync(AdminEmail, HttpMethod.Get, Endpoints["GET /api/task/board"].Url);

        await response.ShouldHaveStatusAsync(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Command_WithAnInvalidBody_FromAUserWithoutTheClaim_Returns403BeforeValidation()
    {
        var user = await Data.UserAsync();

        var response = await SendAsync(user.UserName, HttpMethod.Post, "/api/team", new { name = 42, memberIds = "not-a-list" });

        await response.ShouldHaveStatusAsync(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Command_WithAnInvalidBody_FromAnAuthorizedUser_Returns400()
    {
        var response = await SendAsync(AdminEmail, HttpMethod.Post, "/api/team", new { name = "Team", memberIds = "not-a-list" });

        await response.ShouldHaveStatusAsync(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RefusedCommand_LeavesNoTraceInTheDatabase()
    {
        var user = await Data.UserAsync();
        var team = await Data.TeamAsync([user]);

        var response = await SendAsync(user.UserName, HttpMethod.Delete, $"/api/team/{team.Id}");

        await response.ShouldHaveStatusAsync(HttpStatusCode.Forbidden);
        (await Data.FindIncludingDeletedAsync<TaskFlow.Domain.Entities.Teams.TeamEntity>(team.Id))!.DateTimeDeleted.Should().BeNull();
        Factory.Notifications.All.Should().NotContain(captured => captured.Notification.NotificationType == NotificationType.Confirmation);
    }
}
