using System.Text.Json;
using Tellurian.Trains.Communications.Interfaces.Json;
using Tellurian.Trains.Protocols.LocoNet.Json.Converters;

namespace Tellurian.Trains.Protocols.LocoNet.Json;

/// <summary>
/// Provides JSON serialization options for LocoNet protocol messages.
/// Extends the base options from Tellurian.Trains.Communications.Interfaces with LocoNet-specific converters.
/// </summary>
public static class LocoNetJsonSerializationOptions
{
    private static readonly Lazy<JsonSerializerOptions> _default = new(CreateDefaultOptions);

    /// <summary>
    /// Default JSON serializer options configured for LocoNet message types.
    /// Includes support for polymorphic serialization of commands and notifications.
    /// </summary>
    public static JsonSerializerOptions Default => _default.Value;

    private static JsonSerializerOptions CreateDefaultOptions()
    {
        var baseOptions = JsonSerializationOptions.Default;
        var options = new JsonSerializerOptions(baseOptions);

        // Add LocoNet-specific converters
        options.Converters.Add(new LocoNetMessageConverter());

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
