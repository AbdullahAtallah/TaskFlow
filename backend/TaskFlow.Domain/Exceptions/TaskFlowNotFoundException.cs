namespace TaskFlow.Domain.Exceptions;

/// <summary>The record does not exist or lies outside the user's scope; both are treated as not found.</summary>
public class TaskFlowNotFoundException(string? message) : TaskFlowException(message);
