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

namespace TaskFlow.Application.Commands.Tasks.DeleteTask;

public sealed class DeleteTaskCommandHandler(
    IRepository<TaskEntity> taskRepository,
    IValidator<DeleteTaskCommand> validator,
    IUnitOfWork unitOfWork,
    ISendNotificationCommandHandler sendNotificationCommandHandler,
    ILogger<DeleteTaskCommandHandler> logger)
{
    public async Task Handle(DeleteTaskCommand command)
    {
        try
        {
            await validator.ValidateAndThrowAsync(command);

            var task = await taskRepository.FindInScopeAsync(command.Id, TaskScope.VisibleTo(command.UserCommandContext))
                ?? throw new TaskFlowNotFoundException(ErrorMessages.TaskNotFound);

            await taskRepository.DeleteAsync(task);
            await unitOfWork.SaveChangesAsync();

            await sendNotificationCommandHandler.SendDoneAsync(command, EntityType.Task, ActionType.Deleted, task.Id);
        }
        catch (Exception exception)
        {
            logger.LogCommandFailure(exception, command);

            await sendNotificationCommandHandler.SendErrorAsync(command, EntityType.Task, ActionType.Deleted, exception, command.Id);
        }
    }
}
