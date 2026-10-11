using FluentValidation;
using Microsoft.Extensions.Logging;
using TaskFlow.Application.Common.Extensions;
using TaskFlow.Application.Common.Logging;
using TaskFlow.Application.Common.Notifying;
using TaskFlow.Application.Common.Scopes;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Constants;
using TaskFlow.Domain.Entities.Projects;
using TaskFlow.Domain.Entities.Teams;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Application.Commands.Projects.CreateProject;

public sealed class CreateProjectCommandHandler(
    IRepository<ProjectEntity> projectRepository,
    IRepository<TeamEntity> teamRepository,
    IValidator<CreateProjectCommand> validator,
    IUnitOfWork unitOfWork,
    ISendNotificationCommandHandler sendNotificationCommandHandler,
    ILogger<CreateProjectCommandHandler> logger)
{
    public async Task Handle(CreateProjectCommand command)
    {
        try
        {
            await validator.ValidateAndThrowAsync(command);

            var team = await teamRepository.FindInScopeAsync(command.TeamId, ProjectScope.TeamsVisibleTo(command.UserCommandContext))
                ?? throw new TaskFlowNotFoundException(ErrorMessages.TeamNotFound);

            var project = new ProjectEntity
            {
                Id = Guid.NewGuid(),
                TeamId = team.Id,
                Name = command.Name.Trim(),
                Description = command.Description.TrimToNull()
            };

            await projectRepository.AddAsync(project);
            await unitOfWork.SaveChangesAsync();

            await sendNotificationCommandHandler.SendDoneAsync(command, EntityType.Project, ActionType.Created, project.Id);
        }
        catch (Exception exception)
        {
            logger.LogCommandFailure(exception, command);

            await sendNotificationCommandHandler.SendErrorAsync(command, EntityType.Project, ActionType.Created, exception);
        }
    }
}
