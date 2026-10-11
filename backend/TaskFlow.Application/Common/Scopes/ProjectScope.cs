using System.Linq.Expressions;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Constants;
using TaskFlow.Domain.Entities.Projects;
using TaskFlow.Domain.Entities.Teams;

namespace TaskFlow.Application.Common.Scopes;

/// <summary>Projects: the projects of your teams — every project with Projects.View.All.</summary>
public static class ProjectScope
{
    public static Expression<Func<ProjectEntity, bool>> VisibleTo(IUserScope user)
    {
        if (user.HasClaim(ClaimTypes.ProjectsViewAll))
        {
            return _ => true;
        }

        var userId = user.Id;
        return project => project.Team!.Members.Any(member => member.UserId == userId);
    }

    /// <summary>The teams whose projects the user sees, and in which they may create projects.</summary>
    public static Expression<Func<TeamEntity, bool>> TeamsVisibleTo(IUserScope user)
    {
        if (user.HasClaim(ClaimTypes.ProjectsViewAll))
        {
            return _ => true;
        }

        var userId = user.Id;
        return team => team.Members.Any(member => member.UserId == userId);
    }
}
