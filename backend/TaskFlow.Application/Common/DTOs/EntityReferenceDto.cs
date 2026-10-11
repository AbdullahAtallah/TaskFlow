namespace TaskFlow.Application.Common.DTOs;

/// <summary>The id and name of a related role or team.</summary>
public sealed record EntityReferenceDto(Guid Id, string Name);
