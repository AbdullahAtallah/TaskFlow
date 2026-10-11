using FluentValidation;
using Microsoft.Extensions.Logging;
using TaskFlow.Application.Common.Extensions;
using TaskFlow.Application.Common.Logging;
using TaskFlow.Application.Common.Notifying;
using TaskFlow.Application.Common.Scopes;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Constants;
using TaskFlow.Domain.Entities.Teams;
using TaskFlow.Domain.Entities.Users;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Application.Commands.Teams.AddTeamMembers;

public sealed class AddTeamMembersCommandHandler(
    IRepository<TeamEntity> teamRepository,
    IRepository<TeamMemberEntity> teamMemberRepository,
    IRepository<UserEntity> userRepository,
    IValidator<AddTeamMembersCommand> validator,
    IUnitOfWork unitOfWork,
    ISendNotificationCommandHandler sendNotificationCommandHandler,
    ILogger<AddTeamMembersCommandHandler> logger)
{
    public async Task Handle(AddTeamMembersCommand command)
    {
        try
        {
            await validator.ValidateAndThrowAsync(command);

            var team = await teamRepository.FindInScopeAsync(command.TeamId, TeamScope.VisibleTo(command.UserCommandContext))
                ?? throw new TaskFlowNotFoundException(ErrorMessages.TeamNotFound);

            await EnsureUsersExistAsync(command.UserIds);

            await AddNewMembersAsync(team.Id, command.UserIds);

            await sendNotificationCommandHandler.SendDoneAsync(command, EntityType.TeamMember, ActionType.Created, team.Id);
        }
        catch (Exception exception)
        {
            logger.LogCommandFailure(exception, command);

            await sendNotificationCommandHandler.SendErrorAsync(command, EntityType.TeamMember, ActionType.Created, exception, command.TeamId);
        }
    }

    private async Task EnsureUsersExistAsync(List<Guid> userIds)
    {
        var users = await userRepository.FindAsync(user => userIds.Contains(user.Id));

        if (users.Count != userIds.Count)
        {
            throw new TaskFlowNotFoundException(ErrorMessages.UsersNotFound);
        }
    }

    private async Task AddNewMembersAsync(Guid teamId, List<Guid> userIds)
    {
        var existingMembers = await teamMemberRepository.FindAsync(member => member.TeamId == teamId && userIds.Contains(member.UserId));
        var existingUserIds = existingMembers.Select(member => member.UserId).ToHashSet();

        foreach (var userId in userIds.Where(userId => !existingUserIds.Contains(userId)))
        {
            await teamMemberRepository.AddAsync(new TeamMemberEntity { TeamId = teamId, UserId = userId });
        }

        await unitOfWork.SaveChangesAsync();
    }
}
