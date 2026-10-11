using FluentValidation;
using Microsoft.Extensions.Logging;
using TaskFlow.Application.Common.Extensions;
using TaskFlow.Application.Common.Logging;
using TaskFlow.Application.Common.Notifying;
using TaskFlow.Application.Interfaces;
using TaskFlow.Application.Interfaces.DataReaders;
using TaskFlow.Domain.Constants;
using TaskFlow.Domain.Entities.Teams;
using TaskFlow.Domain.Entities.Users;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Application.Commands.Teams.CreateTeam;

public sealed class CreateTeamCommandHandler(
    IRepository<TeamEntity> teamRepository,
    IRepository<TeamMemberEntity> teamMemberRepository,
    IRepository<UserEntity> userRepository,
    ITeamDataReader teamDataReader,
    IValidator<CreateTeamCommand> validator,
    IUnitOfWork unitOfWork,
    ISendNotificationCommandHandler sendNotificationCommandHandler,
    ILogger<CreateTeamCommandHandler> logger)
{
    public async Task Handle(CreateTeamCommand command)
    {
        try
        {
            await validator.ValidateAndThrowAsync(command);

            var name = command.Name.Trim();
            await EnsureNameIsAvailableAsync(name);

            var memberIds = command.MemberIds ?? [];
            await EnsureUsersExistAsync(memberIds);

            var team = await CreateTeamAsync(command, name, memberIds);

            await sendNotificationCommandHandler.SendDoneAsync(command, EntityType.Team, ActionType.Created, team.Id);
        }
        catch (Exception exception)
        {
            logger.LogCommandFailure(exception, command);

            await sendNotificationCommandHandler.SendErrorAsync(command, EntityType.Team, ActionType.Created, exception);
        }
    }

    private async Task EnsureNameIsAvailableAsync(string name)
    {
        if (await teamDataReader.IsNameInUseAsync(name, excludeTeamId: null))
        {
            throw new TaskFlowException(ErrorMessages.TeamNameAlreadyInUse);
        }
    }

    private async Task EnsureUsersExistAsync(List<Guid> userIds)
    {
        if (userIds.Count == 0)
        {
            return;
        }

        var users = await userRepository.FindAsync(user => userIds.Contains(user.Id));

        if (users.Count != userIds.Count)
        {
            throw new TaskFlowNotFoundException(ErrorMessages.UsersNotFound);
        }
    }

    private async Task<TeamEntity> CreateTeamAsync(CreateTeamCommand command, string name, List<Guid> memberIds)
    {
        var team = new TeamEntity
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = command.Description.TrimToNull()
        };

        await teamRepository.AddAsync(team);

        foreach (var userId in memberIds)
        {
            await teamMemberRepository.AddAsync(new TeamMemberEntity { TeamId = team.Id, UserId = userId });
        }

        await unitOfWork.SaveChangesAsync();

        return team;
    }
}
