namespace TaskFlow.Application.Commands.Roles.Common;

/// <summary>The fields the role wizard and the role details page edit; shared by the create and update commands.</summary>
public interface IRoleFieldsCommand
{
    string Name { get; }
    string? Description { get; }
    bool IsDefault { get; }
    List<string> Packages { get; }
    List<string> Claims { get; }
}
