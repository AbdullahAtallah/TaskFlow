using TaskFlow.Application.Common;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Constants;
using TaskFlow.Domain.Entities.Teams;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Application.Commands.Tasks.Common;

public sealed class TaskAssigneeGuard(IRepository<TeamMemberEntity> teamMemberRepository)
{
    /// <summary>
    /// Changing the assignee (also clearing it) requires Tasks.Assign, and a new assignee must be a member of the project's team.
    /// </summary>
    public async Task EnsureCanAssignAsync(UserCommandContext userCommandContext, Guid teamId, Guid? assigneeId)
    {
        if (!userCommandContext.HasClaim(ClaimTypes.TasksAssign))
        {
            throw new TaskFlowException(ErrorMessages.AssigningRequiresClaim);
        }

        if (assigneeId is not { } userId)
        {
            return;
        }

        var memberships = await teamMemberRepository.FindAsync(member => member.TeamId == teamId && member.UserId == userId);

        if (memberships.Count == 0)
        {
            throw new TaskFlowException(ErrorMessages.AssigneeMustBeTeamMember);
        }
    }
}
