using System.Threading.RateLimiting;
using Microsoft.Extensions.Options;
using TaskFlow.Infrastructure.Identity;

namespace TaskFlow.Api.Configurations;

public static class RateLimitingConfiguration
{
    public const string SignInPolicy = "sign-in";

    /// <summary>
    /// Slows down password guessing: register and login allow a limited number of attempts per client IP address in
    /// each window (RateLimiting section); more answer 429 Too Many Requests. The login handler adds a limit per e-mail
    /// address. Behind a reverse proxy, enable forwarded headers so that the client's address is used, not the proxy's.
    /// </summary>
    public static IServiceCollection AddRateLimitingConfiguration(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy(SignInPolicy, httpContext =>
            {
                var settings = httpContext.RequestServices.GetRequiredService<IOptions<RateLimitingSettings>>().Value;

                return RateLimitPartition.GetFixedWindowLimiter(
                    httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = settings.SignInPermitLimit,
                        Window = settings.SignInWindow,
                        QueueLimit = 0
                    });
            });
        });

        return services;
    }
}
