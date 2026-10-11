using FluentValidation;
using Microsoft.Extensions.Logging;
using TaskFlow.Application.Common.Extensions;
using TaskFlow.Application.Common.Logging;
using TaskFlow.Application.Common.Notifying;
using TaskFlow.Application.Common.Scopes;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Constants;
using TaskFlow.Domain.Entities.Projects;
using TaskFlow.Domain.Entities.Tasks;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Application.Commands.Projects.DeleteProject;

public sealed class DeleteProjectCommandHandler(
    IRepository<ProjectEntity> projectRepository,
    IRepository<TaskEntity> taskRepository,
    IValidator<DeleteProjectCommand> validator,
    IUnitOfWork unitOfWork,
    ISendNotificationCommandHandler sendNotificationCommandHandler,
    ILogger<DeleteProjectCommandHandler> logger)
{
    public async Task Handle(DeleteProjectCommand command)
    {
        try
        {
            await validator.ValidateAndThrowAsync(command);

            var project = await projectRepository.FindInScopeAsync(command.Id, ProjectScope.VisibleTo(command.UserCommandContext))
                ?? throw new TaskFlowNotFoundException(ErrorMessages.ProjectNotFound);

            await DeleteProjectWithTasksAsync(project);

            await sendNotificationCommandHandler.SendDoneAsync(command, EntityType.Project, ActionType.Deleted, project.Id);
        }
        catch (Exception exception)
        {
            logger.LogCommandFailure(exception, command);

            await sendNotificationCommandHandler.SendErrorAsync(command, EntityType.Project, ActionType.Deleted, exception, command.Id);
        }
    }

    /// <summary>Both are soft-deleted: the rows stay in the database with DateTimeDeleted set.</summary>
    private async Task DeleteProjectWithTasksAsync(ProjectEntity project)
    {
        var tasks = await taskRepository.FindAsync(task => task.ProjectId == project.Id);
        taskRepository.DeleteRange(tasks);

        await projectRepository.DeleteAsync(project);
        await unitOfWork.SaveChangesAsync();
    }
}
