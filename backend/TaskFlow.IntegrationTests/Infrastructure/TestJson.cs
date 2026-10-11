using System.Text.Json;
using System.Text.Json.Serialization;

namespace TaskFlow.IntegrationTests.Infrastructure;

public static class TestJson
{
    public static JsonSerializerOptions Options { get; } = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };
}
