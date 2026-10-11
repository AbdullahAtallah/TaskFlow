namespace TaskFlow.Domain.Exceptions;

/// <summary>A business-rule failure whose message may be shown to the user.</summary>
public class TaskFlowException(string? message, Exception? innerException = null) : Exception(message, innerException);
