using FluentValidation;
using Microsoft.Extensions.Logging;
using TaskFlow.Application.Common.Extensions;
using TaskFlow.Application.Common.Logging;
using TaskFlow.Application.Common.Notifying;
using TaskFlow.Application.Common.Scopes;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Constants;
using TaskFlow.Domain.Entities.Tasks;
using TaskFlow.Domain.Entities.Teams;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Application.Commands.Teams.RemoveTeamMember;

public sealed class RemoveTeamMemberCommandHandler(
    IRepository<TeamEntity> teamRepository,
    IRepository<TeamMemberEntity> teamMemberRepository,
    IRepository<TaskEntity> taskRepository,
    IValidator<RemoveTeamMemberCommand> validator,
    IUnitOfWork unitOfWork,
    ISendNotificationCommandHandler sendNotificationCommandHandler,
    ILogger<RemoveTeamMemberCommandHandler> logger)
{
    public async Task Handle(RemoveTeamMemberCommand command)
    {
        try
        {
            await validator.ValidateAndThrowAsync(command);

            var team = await teamRepository.FindInScopeAsync(command.TeamId, TeamScope.VisibleTo(command.UserCommandContext))
                ?? throw new TaskFlowNotFoundException(ErrorMessages.TeamNotFound);

            var memberships = await teamMemberRepository.FindAsync(member => member.TeamId == team.Id && member.UserId == command.UserId);
            if (memberships.Count == 0)
            {
                throw new TaskFlowNotFoundException(ErrorMessages.TeamMemberNotFound);
            }

            teamMemberRepository.DeleteRange(memberships);
            await UnassignTasksInTeamProjectsAsync(team.Id, command.UserId);
            await unitOfWork.SaveChangesAsync();

            await sendNotificationCommandHandler.SendDoneAsync(command, EntityType.TeamMember, ActionType.Deleted, team.Id);
        }
        catch (Exception exception)
        {
            logger.LogCommandFailure(exception, command);

            await sendNotificationCommandHandler.SendErrorAsync(command, EntityType.TeamMember, ActionType.Deleted, exception, command.TeamId);
        }
    }

    /// <summary>The assignee of a task must be a member of the project's team, so a former member loses their tasks.</summary>
    private async Task UnassignTasksInTeamProjectsAsync(Guid teamId, Guid userId)
    {
        var tasks = await taskRepository.FindAsync(task => task.AssigneeId == userId && task.Project!.TeamId == teamId);

        foreach (var task in tasks)
        {
            task.AssigneeId = null;
        }
    }
}
