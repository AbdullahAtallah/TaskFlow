using TaskFlow.Domain.Constants;

namespace TaskFlow.Domain.Exceptions;

/// <summary>
/// A unique index refused the save: a parallel request took the same name or e-mail address between the check and
/// the save.
/// </summary>
public sealed class DuplicateValueException(Exception? innerException = null)
    : TaskFlowException(ErrorMessages.ValueAlreadyInUse, innerException);
