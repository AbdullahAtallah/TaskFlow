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

namespace TaskFlow.Application.Commands.Tasks.UpdateTask;

public sealed class UpdateTaskCommandHandler(
    IRepository<TaskEntity> taskRepository,
    IRepository<ProjectEntity> projectRepository,
    TaskAssigneeGuard taskAssigneeGuard,
    IValidator<UpdateTaskCommand> validator,
    IUnitOfWork unitOfWork,
    ISendNotificationCommandHandler sendNotificationCommandHandler,
    ILogger<UpdateTaskCommandHandler> logger)
{
    public async Task Handle(UpdateTaskCommand command)
    {
        try
        {
            await validator.ValidateAndThrowAsync(command);

            var task = await taskRepository.FindInScopeAsync(command.Id, TaskScope.VisibleTo(command.UserCommandContext))
                ?? throw new TaskFlowNotFoundException(ErrorMessages.TaskNotFound);

            task.EnsureUnchangedSince(command.RowVersion);

            if (task.AssigneeId != command.AssigneeId)
            {
                var project = await projectRepository.GetByIdAsync(task.ProjectId)
                    ?? throw new TaskFlowNotFoundException(ErrorMessages.ProjectNotFound);

                await taskAssigneeGuard.EnsureCanAssignAsync(command.UserCommandContext, project.TeamId, command.AssigneeId);
            }

            task.Title = command.Title.Trim();
            task.Description = command.Description.TrimToNull();
            task.Status = command.Status;
            task.Priority = command.Priority;
            task.DueDate = command.DueDate;
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
