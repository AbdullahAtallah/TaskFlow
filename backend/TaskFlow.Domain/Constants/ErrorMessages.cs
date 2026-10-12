namespace TaskFlow.Domain.Constants;

/// <summary>Messages of business-rule failures; they reach the user in the Error notification or the response.</summary>
public static class ErrorMessages
{
    // Any record
    public const string ChangedByAnotherUser = "Someone else changed this in the meantime. Reload it and try again.";
    public const string ValueAlreadyInUse = "This name or e-mail address was just taken by someone else.";

    // Signing in
    public const string InvalidCredentials = "The e-mail address or password is incorrect.";
    public const string TooManySignInAttempts = "Too many failed sign-ins for this e-mail address. Try again later.";

    // Teams
    public const string TeamNotFound = "The team could not be found.";
    public const string TeamNameAlreadyInUse = "A team with this name already exists.";
    public const string TeamStillOwnsProjects = "This team still owns active projects. Move or delete them first.";
    public const string TeamMemberNotFound = "This user is not a member of the team.";

    // Projects
    public const string ProjectNotFound = "The project could not be found.";

    // Tasks
    public const string TaskNotFound = "The task could not be found.";
    public const string AssigningRequiresClaim = "Assigning a task requires the Tasks.Assign permission.";
    public const string AssigneeMustBeTeamMember = "The assignee must be a member of the project's team.";

    // Users
    public const string UserNotFound = "The user could not be found.";
    public const string UsersNotFound = "One or more users could not be found.";
    public const string UserNameAlreadyInUse = "A user with this e-mail address already exists.";
    public const string CannotDeactivateOwnAccount = "You cannot deactivate your own account.";
    public const string CannotDeleteOwnAccount = "You cannot delete your own account.";
    public const string CannotRemoveOwnManagementAccess = "You cannot remove your own access to user and role management.";

    // Roles
    public const string RoleNotFound = "The role could not be found.";
    public const string RolesNotFound = "One or more roles could not be found.";
    public const string RoleNameAlreadyInUse = "A role with this name already exists.";
    public const string DefaultRoleCannotBeDeleted = "Default roles cannot be deleted.";
    public const string RoleForNewUsersCannotBeDeleted = "The role for new users cannot be deleted. Make another role the default first.";
    public const string RoleForNewUsersCannotGrantManagement = "The role for new users cannot grant user and role management.";
    public const string RoleStillAssigned = "This role is still assigned to users. Give them another role first.";
}
