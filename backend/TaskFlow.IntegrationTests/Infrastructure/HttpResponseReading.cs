using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace TaskFlow.IntegrationTests.Infrastructure;

public static class HttpResponseReading
{
    /// <summary>The messages per field of a 400 validation problem.</summary>
    public static async Task<IDictionary<string, string[]>> ReadValidationErrorsAsync(this HttpResponseMessage response)
    {
        return (await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(TestJson.Options))!.Errors;
    }

    /// <summary>The reason of a 401 with a body, such as account_revoked.</summary>
    public static async Task<string?> ReadErrorAsync(this HttpResponseMessage response)
    {
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.GetProperty("error").GetString();
    }
}
