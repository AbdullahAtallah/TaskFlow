using FluentValidation;
using Microsoft.Extensions.Logging;
using TaskFlow.Application.Common.Extensions;
using TaskFlow.Application.Common.Logging;
using TaskFlow.Application.Common.Notifying;
using TaskFlow.Application.Common.Scopes;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Constants;
using TaskFlow.Domain.Entities.Tasks;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Application.Commands.Tasks.ChangeTaskStatus;

public sealed class ChangeTaskStatusCommandHandler(
    IRepository<TaskEntity> taskRepository,
    IValidator<ChangeTaskStatusCommand> validator,
    IUnitOfWork unitOfWork,
    ISendNotificationCommandHandler sendNotificationCommandHandler,
    ILogger<ChangeTaskStatusCommandHandler> logger)
{
    public async Task Handle(ChangeTaskStatusCommand command)
    {
        try
        {
            await validator.ValidateAndThrowAsync(command);

            var task = await taskRepository.FindInScopeAsync(command.Id, TaskScope.VisibleTo(command.UserCommandContext))
                ?? throw new TaskFlowNotFoundException(ErrorMessages.TaskNotFound);

            task.Status = command.Status;

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
