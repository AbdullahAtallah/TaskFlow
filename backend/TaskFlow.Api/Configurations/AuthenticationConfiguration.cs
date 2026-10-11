using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TaskFlow.Infrastructure.Identity;
using TaskFlow.Infrastructure.SignalR;

namespace TaskFlow.Api.Configurations;

public static class AuthenticationConfiguration
{
    /// <summary>
    /// Validates the access tokens TaskFlow issues: signature (HMAC-SHA256 only, with the configured key), issuer,
    /// audience and expiry, without clock skew. A missing or invalid token ends in 401 through the fallback policy.
    /// </summary>
    public static IServiceCollection AddAuthenticationConfiguration(this IServiceCollection services)
    {
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtSettings>>((options, jwtSettings) =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = jwtSettings.Value.Issuer,
                    ValidAudience = jwtSettings.Value.Audience,
                    IssuerSigningKey = jwtSettings.Value.CreateSigningKey(),
                    ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
                    NameClaimType = AccessTokenClaims.UserId,
                    ClockSkew = TimeSpan.Zero
                };
                options.Events = new JwtBearerEvents { OnMessageReceived = ReadNotificationHubAccessToken };
            });

        return services;
    }

    /// <summary>Browsers cannot send headers on WebSocket connections, so SignalR passes the token in the query string.</summary>
    private static Task ReadNotificationHubAccessToken(MessageReceivedContext context)
    {
        var accessToken = context.Request.Query["access_token"];

        if (!string.IsNullOrEmpty(accessToken) && context.HttpContext.Request.Path.StartsWithSegments(NotificationHubPaths.Hub))
        {
            context.Token = accessToken;
        }

        return Task.CompletedTask;
    }
}
