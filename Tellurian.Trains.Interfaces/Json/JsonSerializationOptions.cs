using System.Text.Json;
using System.Text.Json.Serialization;
using Tellurian.Trains.Interfaces.Json.Converters;

namespace Tellurian.Trains.Interfaces.Json;

/// <summary>
/// Provides centralized JSON serialization options for the train control library.
/// Use <see cref="Default"/> for standard serialization/deserialization of all message types.
/// </summary>
public static class JsonSerializationOptions
{
    private static readonly Lazy<JsonSerializerOptions> _default = new(CreateDefaultOptions);

    /// <summary>
    /// Default JSON serializer options configured for train control message types.
    /// Includes support for polymorphic serialization of notifications, commands, and responses.
    /// </summary>
    public static JsonSerializerOptions Default => _default.Value;

    private static JsonSerializerOptions CreateDefaultOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNameCaseInsensitive = true,
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase),
                new LocoAddressConverter(),
                new AccessoryAddressConverter(),
                new SpeedConverter(),
                new FunctionConverter(),
            }
        };
        return options;
    }

    /// <summary>
    /// Creates a copy of the default options with indented formatting for debugging/logging.
    /// </summary>
    public static JsonSerializerOptions CreateIndented()
    {
        var options = new JsonSerializerOptions(Default)
        {
            WriteIndented = true
        };
        return options;
    }
}
