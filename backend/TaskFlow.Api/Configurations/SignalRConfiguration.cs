using System.Text.Json.Serialization;

namespace TaskFlow.Api.Configurations;

public static class SignalRConfiguration
{
    public static IServiceCollection AddSignalRConfiguration(this IServiceCollection services)
    {
        services
            .AddSignalR()
            .AddJsonProtocol(options => options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        return services;
    }
}
