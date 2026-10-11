using System.Text.Json.Serialization;
using TaskFlow.Api.Shared;

namespace TaskFlow.Api.Configurations;

public static class ControllersConfiguration
{
    public static IServiceCollection AddControllersConfiguration(this IServiceCollection services)
    {
        services
            .AddControllers(options =>
            {
                // Commands are validated by their FluentValidation validator in the handler, not by model binding.
                options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
                options.Filters.Add<ApiExceptionFilter>();
            })
            .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        return services;
    }
}
