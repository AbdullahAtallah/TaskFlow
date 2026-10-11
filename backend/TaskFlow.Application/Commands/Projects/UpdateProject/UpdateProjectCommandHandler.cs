using FluentValidation;
using Microsoft.Extensions.Logging;
using TaskFlow.Application.Common.Extensions;
using TaskFlow.Application.Common.Logging;
using TaskFlow.Application.Common.Notifying;
using TaskFlow.Application.Common.Scopes;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Constants;
using TaskFlow.Domain.Entities.Projects;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Application.Commands.Projects.UpdateProject;

public sealed class UpdateProjectCommandHandler(
    IRepository<ProjectEntity> projectRepository,
    IValidator<UpdateProjectCommand> validator,
    IUnitOfWork unitOfWork,
    ISendNotificationCommandHandler sendNotificationCommandHandler,
    ILogger<UpdateProjectCommandHandler> logger)
{
    public async Task Handle(UpdateProjectCommand command)
    {
        try
        {
            await validator.ValidateAndThrowAsync(command);

            var project = await projectRepository.FindInScopeAsync(command.Id, ProjectScope.VisibleTo(command.UserCommandContext))
                ?? throw new TaskFlowNotFoundException(ErrorMessages.ProjectNotFound);

            project.EnsureUnchangedSince(command.RowVersion);

            project.Name = command.Name.Trim();
            project.Description = command.Description.TrimToNull();

            await projectRepository.UpdateAsync(project);
            await unitOfWork.SaveChangesAsync();

            await sendNotificationCommandHandler.SendDoneAsync(command, EntityType.Project, ActionType.Modified, project.Id);
        }
        catch (Exception exception)
        {
            logger.LogCommandFailure(exception, command);

            await sendNotificationCommandHandler.SendErrorAsync(command, EntityType.Project, ActionType.Modified, exception, command.Id);
        }
    }
}
