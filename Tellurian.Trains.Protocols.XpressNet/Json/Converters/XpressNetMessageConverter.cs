using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tellurian.Trains.Protocols.XpressNet.Json.Converters;

/// <summary>
/// JSON converter for XpressNet <see cref="Message"/> that serializes all public properties.
/// Supports polymorphic serialization with type discrimination.
/// </summary>
public sealed class XpressNetMessageConverter : JsonConverter<Message>
{
    public override Message? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // XpressNet messages are typically created from binary data, not JSON.
        // This converter supports serialization for logging/debugging purposes.
        // Deserialization would require additional context (like the binary buffer).
        throw new NotSupportedException(
            "Deserialization of XpressNet messages from JSON is not supported. " +
            "Use NotificationFactory.Create() or command constructors with binary data.");
    }

    public override void Write(Utf8JsonWriter writer, Message value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        // Write the type discriminator
        writer.WriteString("$type", value.GetType().Name);

        // Write common properties
        writer.WriteNumber("header", value.Header);
        writer.WriteNumber("length", value.Length);
        writer.WriteString("dataHex", value.DataHex);

        // Write type-specific properties using reflection
        WriteTypeSpecificProperties(writer, value, options);

        writer.WriteEndObject();
    }

    private static void WriteTypeSpecificProperties(Utf8JsonWriter writer, Message value, JsonSerializerOptions options)
    {
        var type = value.GetType();
        var baseProperties = typeof(Message).GetProperties().Select(p => p.Name).ToHashSet();

        foreach (var property in type.GetProperties())
        {
            // Skip properties defined in the base class (already written)
            if (baseProperties.Contains(property.Name))
                continue;

            // Skip properties with JsonIgnore attribute
            if (property.GetCustomAttributes(typeof(JsonIgnoreAttribute), true).Length > 0)
                continue;

            // Skip indexers
            if (property.GetIndexParameters().Length > 0)
                continue;

            try
            {
                var propertyValue = property.GetValue(value);
                var propertyName = GetPropertyName(property.Name, options);

                if (propertyValue == null)
                {
                    if (options.DefaultIgnoreCondition != JsonIgnoreCondition.WhenWritingNull)
                    {
                        writer.WriteNull(propertyName);
                    }
                }
                else
                {
                    writer.WritePropertyName(propertyName);
                    JsonSerializer.Serialize(writer, propertyValue, propertyValue.GetType(), options);
                }
            }
            catch
            {
                // Skip properties that throw exceptions when accessed
            }
        }
    }

    private static string GetPropertyName(string propertyName, JsonSerializerOptions options)
    {
        if (options.PropertyNamingPolicy != null)
        {
            return options.PropertyNamingPolicy.ConvertName(propertyName);
        }
        return propertyName;
    }
}
