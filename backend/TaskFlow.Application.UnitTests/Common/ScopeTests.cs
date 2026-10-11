using AwesomeAssertions;
using TaskFlow.Application.Common;
using TaskFlow.Application.Common.Scopes;
using TaskFlow.Domain.Constants;
using TaskFlow.Domain.Entities.Projects;
using TaskFlow.Domain.Entities.Tasks;
using TaskFlow.Domain.Entities.Teams;

namespace TaskFlow.Application.UnitTests.Common;

/// <summary>The scope rules evaluated in memory; the integration tests run the same expressions in SQL.</summary>
public sealed class ScopeTests
{
    private static readonly Guid MemberId = Guid.NewGuid();

    private readonly TeamEntity _ownTeam = TeamWithMember(MemberId);
    private readonly TeamEntity _otherTeam = TeamWithMember(Guid.NewGuid());

    [Fact]
    public void TeamScope_ShowsOnlyOwnTeams_UnlessTeamsViewAll()
    {
        var member = TeamScope.VisibleTo(User()).Compile();
        var viewAll = TeamScope.VisibleTo(User(ClaimTypes.TeamsViewAll)).Compile();

        member(_ownTeam).Should().BeTrue();
        member(_otherTeam).Should().BeFalse();
        viewAll(_otherTeam).Should().BeTrue();
    }

    [Fact]
    public void ProjectScope_ShowsOnlyProjectsOfOwnTeams_UnlessProjectsViewAll()
    {
        var member = ProjectScope.VisibleTo(User()).Compile();
        var teamsViewAll = ProjectScope.VisibleTo(User(ClaimTypes.TeamsViewAll)).Compile();
        var projectsViewAll = ProjectScope.VisibleTo(User(ClaimTypes.ProjectsViewAll)).Compile();

        member(ProjectOf(_ownTeam)).Should().BeTrue();
        member(ProjectOf(_otherTeam)).Should().BeFalse();
        teamsViewAll(ProjectOf(_otherTeam)).Should().BeFalse();
        projectsViewAll(ProjectOf(_otherTeam)).Should().BeTrue();
    }

    [Fact]
    public void TaskScope_ShowsOnlyTasksOfProjectsInScope_UnlessTasksOrProjectsViewAll()
    {
        var member = TaskScope.VisibleTo(User()).Compile();
        var tasksViewAll = TaskScope.VisibleTo(User(ClaimTypes.TasksViewAll)).Compile();
        var projectsViewAll = TaskScope.VisibleTo(User(ClaimTypes.ProjectsViewAll)).Compile();

        member(TaskOf(_ownTeam)).Should().BeTrue();
        member(TaskOf(_otherTeam)).Should().BeFalse();
        tasksViewAll(TaskOf(_otherTeam)).Should().BeTrue();
        projectsViewAll(TaskOf(_otherTeam)).Should().BeTrue();
    }

    [Fact]
    public void ProjectsForNewProjectsAndTasks_FollowTheSameRules()
    {
        ProjectScope.TeamsVisibleTo(User()).Compile()(_otherTeam).Should().BeFalse();
        ProjectScope.TeamsVisibleTo(User(ClaimTypes.ProjectsViewAll)).Compile()(_otherTeam).Should().BeTrue();
        TaskScope.ProjectsVisibleTo(User()).Compile()(ProjectOf(_otherTeam)).Should().BeFalse();
        TaskScope.ProjectsVisibleTo(User(ClaimTypes.TasksViewAll)).Compile()(ProjectOf(_otherTeam)).Should().BeTrue();
    }

    private static UserCommandContext User(params string[] claims)
    {
        return new UserCommandContext(MemberId, "member@taskflow.test", claims);
    }

    private static TeamEntity TeamWithMember(Guid userId)
    {
        var team = new TeamEntity { Id = Guid.NewGuid(), Name = "Team" };
        team.Members.Add(new TeamMemberEntity { TeamId = team.Id, UserId = userId });
        return team;
    }

    private static ProjectEntity ProjectOf(TeamEntity team)
    {
        return new ProjectEntity { Id = Guid.NewGuid(), TeamId = team.Id, Team = team, Name = "Project" };
    }

    private static TaskEntity TaskOf(TeamEntity team)
    {
        var project = ProjectOf(team);
        return new TaskEntity { Id = Guid.NewGuid(), ProjectId = project.Id, Project = project, Title = "Task" };
    }
}
