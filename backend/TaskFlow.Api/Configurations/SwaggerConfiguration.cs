using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using TaskFlow.Api.Swagger;

namespace TaskFlow.Api.Configurations;

public static class SwaggerConfiguration
{
    public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(setup =>
        {
            setup.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "TaskFlow API",
                Version = "v1",
                Description = "Sign in with POST /api/auth/login (or register), click Authorize and paste the accessToken; " +
                              "every other endpoint needs it. Most write endpoints answer 202 Accepted; the result " +
                              "arrives as a SignalR \"ReceiveNotification\" message on /notifications."
            });

            var bearerScheme = new OpenApiSecurityScheme
            {
                Scheme = "bearer",
                BearerFormat = "JWT",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Description = "Paste only the accessToken from /api/auth/login, without \"Bearer\".",
                Reference = new OpenApiReference
                {
                    Id = JwtBearerDefaults.AuthenticationScheme,
                    Type = ReferenceType.SecurityScheme
                }
            };

            setup.AddSecurityDefinition(bearerScheme.Reference.Id, bearerScheme);
            setup.AddSecurityRequirement(new OpenApiSecurityRequirement { { bearerScheme, Array.Empty<string>() } });

            setup.SchemaFilter<HideUserCommandMetadataSchemaFilter>();
        });

        return services;
    }
}
