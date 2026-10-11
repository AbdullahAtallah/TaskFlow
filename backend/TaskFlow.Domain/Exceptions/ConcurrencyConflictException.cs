using TaskFlow.Domain.Constants;

namespace TaskFlow.Domain.Exceptions;

/// <summary>The record was saved by someone else after this user loaded it, so this save is refused.</summary>
public sealed class ConcurrencyConflictException(Exception? innerException = null)
    : TaskFlowException(ErrorMessages.ChangedByAnotherUser, innerException);
