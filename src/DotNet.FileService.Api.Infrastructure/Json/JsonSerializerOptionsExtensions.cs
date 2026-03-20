using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotNet.FileService.Api.Infrastructure.Json;

/// <summary>
/// Extension methods for configuring <see cref="JsonSerializerOptions"/> with standard project settings.
/// </summary>
public static class JsonSerializerOptionsExtensions
{
    /// <summary>
    /// Applies the standard JSON serialization settings used across all services in this project.
    /// <list type="bullet">
    ///   <item><description>Property names serialized as camelCase.</description></item>
    ///   <item><description>Property name matching is case-insensitive during deserialization.</description></item>
    ///   <item><description>Null properties are omitted from serialized output.</description></item>
    ///   <item><description>Enums are serialized as camelCase strings instead of integers.</description></item>
    ///   <item><description>Circular object references are ignored instead of throwing.</description></item>
    /// </list>
    /// </summary>
    /// <param name="options">The options instance to configure.</param>
    /// <returns>The same options instance for chaining.</returns>
    public static JsonSerializerOptions ConfigureStandardOptions(this JsonSerializerOptions options)
    {
        options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.PropertyNameCaseInsensitive = true;
        options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));

        return options;
    }
}
