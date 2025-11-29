using System.Text.Json;
using System.Text.Json.Serialization;
using Tellurian.Trains.Interfaces.Json;
using Tellurian.Trains.Protocols.XpressNet.Json.Converters;

namespace Tellurian.Trains.Protocols.XpressNet.Json;

/// <summary>
/// Provides JSON serialization options for XpressNet protocol messages.
/// Extends the base options from Tellurian.Trains.Interfaces with XpressNet-specific converters.
/// </summary>
public static class XpressNetJsonSerializationOptions
{
    private static readonly Lazy<JsonSerializerOptions> _default = new(CreateDefaultOptions);

    /// <summary>
    /// Default JSON serializer options configured for XpressNet message types.
    /// Includes support for polymorphic serialization of commands and notifications.
    /// </summary>
    public static JsonSerializerOptions Default => _default.Value;

    private static JsonSerializerOptions CreateDefaultOptions()
    {
        var baseOptions = JsonSerializationOptions.Default;
        var options = new JsonSerializerOptions(baseOptions);

        // Add XpressNet-specific converters
        options.Converters.Add(new LocoSpeedConverter());
        options.Converters.Add(new XpressNetMessageConverter());

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
