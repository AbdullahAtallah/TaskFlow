using FluentValidation;
using Microsoft.Extensions.Logging;
using TaskFlow.Application.Commands.Tasks.Common;
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

namespace TaskFlow.Application.Commands.Tasks.AssignTask;

public sealed class AssignTaskCommandHandler(
    IRepository<TaskEntity> taskRepository,
    IRepository<ProjectEntity> projectRepository,
    TaskAssigneeGuard taskAssigneeGuard,
    IValidator<AssignTaskCommand> validator,
    IUnitOfWork unitOfWork,
    ISendNotificationCommandHandler sendNotificationCommandHandler,
    ILogger<AssignTaskCommandHandler> logger)
{
    public async Task Handle(AssignTaskCommand command)
    {
        try
        {
            await validator.ValidateAndThrowAsync(command);

            var task = await taskRepository.FindInScopeAsync(command.Id, TaskScope.VisibleTo(command.UserCommandContext))
                ?? throw new TaskFlowNotFoundException(ErrorMessages.TaskNotFound);

            var project = await projectRepository.GetByIdAsync(task.ProjectId)
                ?? throw new TaskFlowNotFoundException(ErrorMessages.ProjectNotFound);

            await taskAssigneeGuard.EnsureCanAssignAsync(command.UserCommandContext, project.TeamId, command.AssigneeId);

            task.AssigneeId = command.AssigneeId;

            await taskRepository.UpdateAsync(task);
            await unitOfWork.SaveChangesAsync();

            await sendNotificationCommandHandler.SendDoneAsync(command, EntityType.Task, ActionType.Modified, task.Id);
        }
        catch (Exception exception)
        {
            logger.LogCommandFailure(exception, command);

            await sendNotificationCommandHandler.SendErrorAsync(command, EntityType.Task, ActionType.Modified, exception, command.Id);
        }
    }
}
