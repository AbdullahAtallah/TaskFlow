namespace TaskFlow.Application.Commands.Roles.Common;

/// <summary>All the fields of a new role: its name, description, default flag and permissions.</summary>
public interface IRoleFieldsCommand : IRolePermissionsCommand
{
    string Name { get; }
    string? Description { get; }
    bool IsDefault { get; }
}
