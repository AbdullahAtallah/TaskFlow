using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TaskFlow.Application.Commands.Auth.Login;
using TaskFlow.Application.Commands.Auth.Logout;
using TaskFlow.Application.Commands.Auth.Register;
using TaskFlow.Application.Commands.Roles.Common;
using TaskFlow.Application.Commands.Tasks.Common;
using TaskFlow.Application.Commands.Users.CreateUser;
using TaskFlow.Application.Commands.Users.SetUserPassword;

namespace TaskFlow.Application;

public static class TaskFlowApplicationDependencyInjection
{
    /// <summary>
    /// Registers the validators, the services the command handlers share and the handlers that controllers call directly.
    /// Wolverine discovers the other handlers (a Handle method) itself.
    /// </summary>
    public static IServiceCollection ConfigureTaskFlowApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<TaskAssigneeGuard>();

        services.AddScoped<TaskAssigneeGuard>();
        services.AddScoped<ManagementAccessGuard>();
        services.AddScoped<RolePermissionsUpdater>();

        // Answered right away and never a message: these commands carry a password, or come before there is a token.
        services.AddScoped<RegisterCommandHandler>();
        services.AddScoped<LoginCommandHandler>();
        services.AddScoped<LogoutCommandHandler>();
        services.AddScoped<CreateUserCommandHandler>();
        services.AddScoped<SetUserPasswordCommandHandler>();

        return services;
    }
}
