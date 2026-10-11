using System.Linq.Expressions;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Constants;
using TaskFlow.Domain.Entities.Teams;

namespace TaskFlow.Application.Common.Scopes;

/// <summary>Teams: the teams you are a member of — every team with Teams.View.All.</summary>
public static class TeamScope
{
    public static Expression<Func<TeamEntity, bool>> VisibleTo(IUserScope user)
    {
        if (user.HasClaim(ClaimTypes.TeamsViewAll))
        {
            return _ => true;
        }

        var userId = user.Id;
        return team => team.Members.Any(member => member.UserId == userId);
    }
}
