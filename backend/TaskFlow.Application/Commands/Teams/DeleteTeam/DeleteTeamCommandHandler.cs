using FluentValidation;
using Microsoft.Extensions.Logging;
using TaskFlow.Application.Common.Extensions;
using TaskFlow.Application.Common.Logging;
using TaskFlow.Application.Common.Notifying;
using TaskFlow.Application.Common.Scopes;
using TaskFlow.Application.Interfaces;
using TaskFlow.Application.Interfaces.DataReaders;
using TaskFlow.Domain.Constants;
using TaskFlow.Domain.Entities.Teams;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Application.Commands.Teams.DeleteTeam;

public sealed class DeleteTeamCommandHandler(
    IRepository<TeamEntity> teamRepository,
    IRepository<TeamMemberEntity> teamMemberRepository,
    ITeamDataReader teamDataReader,
    IValidator<DeleteTeamCommand> validator,
    IUnitOfWork unitOfWork,
    ISendNotificationCommandHandler sendNotificationCommandHandler,
    ILogger<DeleteTeamCommandHandler> logger)
{
    public async Task Handle(DeleteTeamCommand command)
    {
        try
        {
            await validator.ValidateAndThrowAsync(command);

            var team = await teamRepository.FindInScopeAsync(command.Id, TeamScope.VisibleTo(command.UserCommandContext))
                ?? throw new TaskFlowNotFoundException(ErrorMessages.TeamNotFound);

            await EnsureTeamOwnsNoProjectsAsync(team.Id);

            await DeleteTeamAsync(team);

            await sendNotificationCommandHandler.SendDoneAsync(command, EntityType.Team, ActionType.Deleted, team.Id);
        }
        catch (Exception exception)
        {
            logger.LogCommandFailure(exception, command);

            await sendNotificationCommandHandler.SendErrorAsync(command, EntityType.Team, ActionType.Deleted, exception, command.Id);
        }
    }

    private async Task EnsureTeamOwnsNoProjectsAsync(Guid teamId)
    {
        if (await teamDataReader.HasProjectsAsync(teamId))
        {
            throw new TaskFlowException(ErrorMessages.TeamStillOwnsProjects);
        }
    }

    private async Task DeleteTeamAsync(TeamEntity team)
    {
        var memberships = await teamMemberRepository.FindAsync(member => member.TeamId == team.Id);
        teamMemberRepository.DeleteRange(memberships);

        await teamRepository.DeleteAsync(team);
        await unitOfWork.SaveChangesAsync();
    }
}
