using Microsoft.AspNetCore.Cors.Infrastructure;

namespace TaskFlow.Api.Configurations;

public static class CorsConfiguration
{
    public const string FrontEndPolicy = "FrontEnd";

    private const string AllowedOriginsKey = "Cors:AllowedOrigins";

    /// <summary>Only the configured frontend origins; credentials are allowed for the SignalR connection.</summary>
    public static IServiceCollection AddCorsConfiguration(this IServiceCollection services)
    {
        services.AddCors();

        services.AddOptions<CorsOptions>().Configure<IConfiguration>((options, configuration) =>
        {
            var allowedOrigins = configuration.GetSection(AllowedOriginsKey).Get<string[]>() ?? [];

            options.AddPolicy(FrontEndPolicy, policy => policy
                .WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials());
        });

        return services;
    }
}
