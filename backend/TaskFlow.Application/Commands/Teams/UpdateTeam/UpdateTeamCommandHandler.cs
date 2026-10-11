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

namespace TaskFlow.Application.Commands.Teams.UpdateTeam;

public sealed class UpdateTeamCommandHandler(
    IRepository<TeamEntity> teamRepository,
    ITeamDataReader teamDataReader,
    IValidator<UpdateTeamCommand> validator,
    IUnitOfWork unitOfWork,
    ISendNotificationCommandHandler sendNotificationCommandHandler,
    ILogger<UpdateTeamCommandHandler> logger)
{
    public async Task Handle(UpdateTeamCommand command)
    {
        try
        {
            await validator.ValidateAndThrowAsync(command);

            var team = await teamRepository.FindInScopeAsync(command.Id, TeamScope.VisibleTo(command.UserCommandContext))
                ?? throw new TaskFlowNotFoundException(ErrorMessages.TeamNotFound);

            team.EnsureUnchangedSince(command.RowVersion);

            var name = command.Name.Trim();
            await EnsureNameIsAvailableAsync(name, team.Id);

            team.Name = name;
            team.Description = command.Description.TrimToNull();

            await teamRepository.UpdateAsync(team);
            await unitOfWork.SaveChangesAsync();

            await sendNotificationCommandHandler.SendDoneAsync(command, EntityType.Team, ActionType.Modified, team.Id);
        }
        catch (Exception exception)
        {
            logger.LogCommandFailure(exception, command);

            await sendNotificationCommandHandler.SendErrorAsync(command, EntityType.Team, ActionType.Modified, exception, command.Id);
        }
    }

    private async Task EnsureNameIsAvailableAsync(string name, Guid teamId)
    {
        if (await teamDataReader.IsNameInUseAsync(name, teamId))
        {
            throw new TaskFlowException(ErrorMessages.TeamNameAlreadyInUse);
        }
    }
}
