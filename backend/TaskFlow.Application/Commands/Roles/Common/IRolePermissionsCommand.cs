namespace TaskFlow.Application.Commands.Roles.Common;

/// <summary>The packages and claims of a role: what the Permissions tab of the role details page switches on and off.</summary>
public interface IRolePermissionsCommand
{
    List<string> Packages { get; }
    List<string> Claims { get; }
}
