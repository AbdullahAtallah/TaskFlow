using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using TaskFlow.Application.Common.Interfaces;

namespace TaskFlow.Api.Swagger;

/// <summary>
/// The controller fills the user context and correlation id of a command; a client never sends them,
/// so they are left out of the request body in the documentation.
/// </summary>
internal sealed class HideUserCommandMetadataSchemaFilter : ISchemaFilter
{
    private static readonly string[] ServerSideProperties = ["userCommandContext", "correlationId"];

    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (!typeof(IUserCommand).IsAssignableFrom(context.Type))
        {
            return;
        }

        foreach (var property in ServerSideProperties)
        {
            schema.Properties.Remove(property);
            schema.Required.Remove(property);
        }
    }
}
