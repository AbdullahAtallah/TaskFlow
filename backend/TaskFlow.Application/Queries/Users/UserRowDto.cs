using TaskFlow.Application.Common.DTOs;

namespace TaskFlow.Application.Queries.Users;

public sealed record UserRowDto
{
    public Guid Id { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public IReadOnlyList<EntityReferenceDto> Roles { get; init; } = [];
    public IReadOnlyList<EntityReferenceDto> Teams { get; init; } = [];
    public DateTime DateTimeCreated { get; init; }

    /// <summary>Send this back with an edit: a save over a newer change is then refused.</summary>
    public byte[] RowVersion { get; init; } = [];
}
